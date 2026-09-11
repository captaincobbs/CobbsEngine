using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Engine.Core.Collections;

namespace Engine.Core.Services
{
    public sealed class ServiceManager : IServiceProvider
    {
        private readonly Dictionary<Type, IService> _services = [];
        private readonly List<IService> _initializationOrder = [];
        private readonly ServiceRegistrar _registrar;
        private readonly ServiceEnvironment _environment;

        private readonly object _stateLock = new();
        private LifecycleState _lifecycleState = LifecycleState.NotStarted;
        public LifecycleState State { 
            get
            {
                lock (_stateLock)
                {
                    return _lifecycleState;
                }
            } 
        }


        public ServiceManager(ServiceRegistrar registrar, ServiceEnvironment environment)
        {
            _registrar = registrar;
            _environment = environment;
        }

        /// <summary>Chooses the most appropriate services based on their attributes, sorts them into dependency tiers, then initializes them.</summary>
        /// <param name="totalProgress">Optional argument to track the initialization progress of every service</param>
        public async Task InitializeAsync(IProgress<ServiceInitializationProgress>? progress, CancellationToken cancellation)
        {
            BeginInitialization();

            try
            {
                Dictionary<Type, ServiceRegistrationInfo> selected = SelectServices();
                ValidateFailureBehaviors(selected.Values);
                DependencyGraph<ServiceRegistrationInfo> dependencyGraph = BuildDependencyGraph(selected);
                List<List<ServiceRegistrationInfo>> tiers = dependencyGraph.CreateTiers();

                ServiceProgressTracker tracker = new(selected.Values, progress);
                foreach (List<ServiceRegistrationInfo> tier in tiers)
                {
                    cancellation.ThrowIfCancellationRequested();
                    await InitializeTierAsync(tier, tracker, cancellation);
                }
                cancellation.ThrowIfCancellationRequested();

                tracker.Complete();
                SetState(LifecycleState.Initialized);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                SetState(LifecycleState.ShuttingDown);
                ShutdownInitializedServices();
                SetState(LifecycleState.Cancelled);
                throw;
            }
            catch
            {
                SetState(LifecycleState.ShuttingDown);
                ShutdownInitializedServices();
                SetState(LifecycleState.Failed);
                throw;
            }
        }

        private void BeginInitialization()
        {
            lock (_stateLock)
            {
                if (_lifecycleState != LifecycleState.NotStarted)
                {
                    throw new InvalidOperationException($"Cannot initialize services while manager state is {_lifecycleState}.");
                }

                _lifecycleState = LifecycleState.Initializing;
            }
        }

        private void SetState(LifecycleState state)
        {
            lock (_stateLock)
            {
                _lifecycleState = state;
            }
        }

        private Dictionary<Type, ServiceRegistrationInfo> SelectServices()
        {
            Dictionary<Type, ServiceRegistrationInfo> selected = [];
            foreach (Type serviceType in _registrar.RegisteredServiceTypes)
            {
                IReadOnlyList<ServiceRegistrationInfo> registrations = _registrar.GetRegisteredServices(serviceType);
                selected.Add(serviceType, SelectService(serviceType, registrations));
            }

            return selected;
        }

        private static DependencyGraph<ServiceRegistrationInfo> BuildDependencyGraph(IReadOnlyDictionary<Type, ServiceRegistrationInfo> selected)
        {
            DependencyGraph<ServiceRegistrationInfo> graph = new();

            foreach (ServiceRegistrationInfo registration in selected.Values)
            {
                graph.Add(registration);

                foreach (Type dependencyType in registration.Dependencies)
                {
                    if (!selected.TryGetValue(dependencyType, out ServiceRegistrationInfo? dependency))
                    {
                        throw new InvalidOperationException($"{registration.ServiceType.Name} requires " + $"{dependencyType.Name}, but no compatible service was selected.");
                    }

                    graph.AddDependency(registration, dependency);
                }
            }

            return graph;
        }

        private static IService CreateService(ServiceRegistrationInfo registration)
        {
            object? instance = Activator.CreateInstance(registration.ImplementationType);
            return instance as IService ?? throw new InvalidOperationException($"Could not create {registration.ImplementationType.FullName}.");
        }

        private async Task InitializeTierAsync(IReadOnlyList<ServiceRegistrationInfo> tier, ServiceProgressTracker progressTracker, CancellationToken cancellation)
        {
            using CancellationTokenSource tierCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            List<(ServiceRegistrationInfo Registration, IService Service, Task Task)> pending = [];

            foreach (ServiceRegistrationInfo registration in tier)
            {
                IService service = CreateService(registration);
                Task initialization = InitializeServiceAsync(service, registration, progressTracker, tierCancellation);

                pending.Add((registration, service, initialization));
            }

            try
            {
                await Task.WhenAll(pending.Select(item => item.Task));

                foreach ((ServiceRegistrationInfo registration, IService service, Task task) in pending)
                {
                    _services.Add(registration.ServiceType, service);
                    _initializationOrder.Add(service);
                }
            }
            catch
            {
                foreach ((ServiceRegistrationInfo registration, IService service, Task task) in pending)
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        _services[registration.ServiceType] = service;
                        _initializationOrder.Add(service);
                    }
                }

                throw;
            }
        }

        private async Task InitializeServiceAsync(IService service, ServiceRegistrationInfo registration, ServiceProgressTracker progressTracker, CancellationTokenSource cancellationSource)
        {
            try
            {
                await service.InitializeAsync(this, progressTracker.CreateServiceProgress(registration), cancellationSource.Token);
            }
            catch
            {
                cancellationSource.Cancel();
                throw;
            }
        }

        /// <summary></summary>
        public void Shutdown()
        {
            lock (_stateLock)
            {
                if (_lifecycleState == LifecycleState.Shutdown || _lifecycleState == LifecycleState.NotStarted)
                {
                    return;
                }

                if (_lifecycleState == LifecycleState.Initializing)
                {
                    throw new InvalidOperationException("Cannot synchronously shut down while services are initializing.");
                }

                _lifecycleState = LifecycleState.ShuttingDown;
            }

            try
            {
                ShutdownInitializedServices();
            }
            finally
            {
                SetState(LifecycleState.Shutdown);
            }
        }

        private void ShutdownInitializedServices()
        {
            for (int index = _initializationOrder.Count - 1; index >= 0; index--)
            {
                try
                {
                    _initializationOrder[index].Shutdown();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            _initializationOrder.Clear();
            _services.Clear();
        }

        /// <summary>Gets an instance of the specified service type.</summary>
        /// <typeparam name="T">The implementation type of the service, typically the interface type.</typeparam>
        /// <exception cref="KeyNotFoundException">Thrown when the specified service type is not found.</exception>
        /// <returns>The instance of the service.</returns>
        public T Get<T>() where T : class, IService => (T)_services[typeof(T)];

        public object? GetService(Type serviceType)
        {
            return _services.GetValueOrDefault(serviceType);
        }

        /// <summary>Tries to get an instance of the specified service type.</summary>
        /// <typeparam name="T">The implementation type of the service, typically the interface type.</typeparam>
        /// <param name="service">When this method returns, contains the instance of the service if it was found; otherwise, null.</param>
        /// <returns>true if the service was found; otherwise, false.</returns>
        public bool TryGet<T>(out T? service) where T : class, IService
        {
            service = _services.TryGetValue(typeof(T), out IService? value) ? (T)value : null;
            return service != null;
        }

        private ServiceRegistrationInfo SelectService(Type serviceType, IReadOnlyList<ServiceRegistrationInfo> implementations)
        {
            List<ServiceRegistrationInfo> candidates = [.. implementations];

            foreach (ServiceRegistrationInfo registration in implementations)
            {
                // If Application not in required mode, remove from candidacy
                if ((_environment.ApplicationMode & registration.ApplicationMode) == 0)
                {
                    candidates.Remove(registration);
                    continue;
                }

                // If not using the required RuntimePlatform, remove from candidacy
                if ((_environment.Platform & registration.TargetPlatforms) == 0)
                {
                    candidates.Remove(registration);
                    continue;
                }

                // If not using the required GraphicsAPI, remove from candidacy
                if (registration.GraphicsBackend != null && _environment.GraphicsBackend != registration.GraphicsBackend)
                {
                    candidates.Remove(registration);
                    continue;
                }
            }

            if (candidates.Count == 0)
            {
                throw new InvalidOperationException($"No valid service candidates for type '{serviceType.Name}' found after filtering.");
            }

            int highestPriority = candidates.Max(candidate => candidate.Priority);
            List<ServiceRegistrationInfo> highestPriorityCandidates = [.. candidates.Where(candidate => candidate.Priority == highestPriority)];

            if (highestPriorityCandidates.Count > 1)
            {
                string conflicts = string.Join(", ", highestPriorityCandidates.Select(candidate => candidate.ImplementationType.FullName));

                throw new InvalidOperationException($"Multiple compatible implementations of '{serviceType.FullName}' have the same highest priority ({highestPriority}): {conflicts}.");
            }

            return highestPriorityCandidates[0];
        }

        private static void ValidateFailureBehaviors(IEnumerable<ServiceRegistrationInfo> selectedServices)
        {
            ServiceRegistrationInfo[] unsupported = [.. selectedServices.Where(service => service.FailureBehavior != ServiceFailureBehavior.StopInitialization)];

            if (unsupported.Length == 0)
            {
                return;
            }

            string services = string.Join(", ", unsupported.Select(service => $"{service.ServiceType.Name} ({service.FailureBehavior})"));
            throw new NotSupportedException($"Only {nameof(ServiceFailureBehavior.StopInitialization)} " + $"is supported during service initialization. Unsupported: {services}.");
        }
    }
}
