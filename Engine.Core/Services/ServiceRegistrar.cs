using System.Reflection;

using Engine.Core.Platform;
using Engine.Core.Services.Attributes;

namespace Engine.Core.Services
{
    /// <summary>
    /// Discovers and stores service-registration metadata. It never constructs or initializes services.
    /// </summary>
    public sealed class ServiceRegistrar
    {
        private readonly Dictionary<Type, List<ServiceRegistrationInfo>> _registeredServices = [];
        private readonly HashSet<Assembly> _scannedAssemblies = [];

        public IReadOnlyCollection<Type> RegisteredServiceTypes => _registeredServices.Keys;

        public void ScanAssemblies(IEnumerable<Assembly> assemblies)
        {
            ArgumentNullException.ThrowIfNull(assemblies);

            foreach (Assembly assembly in assemblies)
            {
                ScanAssembly(assembly);
            }
        }

        public void ScanAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            if (!_scannedAssemblies.Add(assembly))
            {
                return;
            }

            try
            {
                foreach (Type implementationType in assembly.GetTypes())
                {
                    foreach (RegisterServiceAttribute registration in implementationType.GetCustomAttributes<RegisterServiceAttribute>(inherit: true))
                    {
                        RegisterService(
                            registration.ServiceType,
                            implementationType,
                            registration.Priority,
                            registration.LoadingWeight,
                            registration.FailureBehavior);
                    }
                }
            }
            catch
            {
                RemoveAssembly(assembly);
                throw;
            }
        }

        /// <summary>
        /// Records service metadata only. It does not choose, construct, or initialize a service.
        /// </summary>
        public void RegisterService(Type serviceType, Type implementationType, int priority, int loadingWeight, ServiceFailureBehavior failureBehavior)
        {
            ArgumentNullException.ThrowIfNull(serviceType, nameof(serviceType));
            ArgumentNullException.ThrowIfNull(implementationType, nameof(implementationType));
            ArgumentOutOfRangeException.ThrowIfNegative(loadingWeight, nameof(loadingWeight));

            if (!serviceType.IsInterface)
            {
                throw new ArgumentException($"{serviceType.FullName} is not an interface.", nameof(serviceType));
            }

            if (!typeof(IService).IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"{serviceType.Name} must implement {nameof(IService)}.", nameof(serviceType));
            }

            if (!serviceType.IsAssignableFrom(implementationType))
            {
                throw new ArgumentException($"{implementationType.Name} does not implement {serviceType.Name}.", nameof(implementationType));
            }

            if (implementationType.IsAbstract || implementationType.IsInterface)
            {
                throw new ArgumentException($"{implementationType.Name} cannot be registered as an implementation.", nameof(implementationType));
            }

            Type[] dependencies = [.. implementationType
                .GetCustomAttributes<DependsOnAttribute>(inherit: true)
                .SelectMany(attribute => attribute.Dependencies)
                .Distinct()];

            foreach (Type dependency in dependencies)
            {
                if (!dependency.IsInterface || !typeof(IService).IsAssignableFrom(dependency))
                {
                    throw new ArgumentException(
                        $"Dependency {dependency.FullName} on {implementationType.FullName} must be an {nameof(IService)} interface.",
                        nameof(implementationType));
                }
            }

            RegisterService(new ServiceRegistrationInfo(
                serviceType,
                implementationType,
                implementationType.Assembly,
                priority,
                loadingWeight,
                failureBehavior,
                applicationMode: implementationType.GetCustomAttribute <ApplicationModeAttribute>(inherit: true)?.Mode ?? ApplicationMode.All,
                targetPlatforms: implementationType.GetCustomAttribute<RuntimePlatformAttribute>(inherit: true)?.Platforms ?? RuntimePlatform.All,
                graphicsBackend: implementationType.GetCustomAttribute<GraphicsBackendAttribute>(inherit: true)?.Backend,
                dependencies));
        }

        public void RegisterService(ServiceRegistrationInfo registration)
        {
            ArgumentNullException.ThrowIfNull(registration);

            if (!_registeredServices.TryGetValue(registration.ServiceType, out List<ServiceRegistrationInfo>? services))
            {
                services = [];
                _registeredServices.Add(registration.ServiceType, services);
            }

            if (services.Any(service =>
                service.ImplementationType == registration.ImplementationType &&
                service.Priority == registration.Priority))
            {
                throw new InvalidOperationException(
                    $"{registration.ImplementationType.FullName} is already registered for " +
                    $"{registration.ServiceType.FullName} with priority {registration.Priority}.");
            }

            services.Add(registration);
        }

        public void RemoveAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            foreach (List<ServiceRegistrationInfo> services in _registeredServices.Values)
            {
                services.RemoveAll(service => service.SourceAssembly == assembly);
            }

            _scannedAssemblies.Remove(assembly);
        }

        public IReadOnlyList<ServiceRegistrationInfo> GetRegisteredServices(Type serviceType)
        {
            ArgumentNullException.ThrowIfNull(serviceType);

            return _registeredServices.TryGetValue(serviceType, out List<ServiceRegistrationInfo>? services)
                ? services.AsReadOnly()
                : Array.Empty<ServiceRegistrationInfo>();
        }

        public IReadOnlyList<ServiceRegistrationInfo> GetRegisteredServices<T>() where T : class, IService => GetRegisteredServices(typeof(T));
    }
}