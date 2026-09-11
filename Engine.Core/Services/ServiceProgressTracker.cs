namespace Engine.Core.Services
{
    internal sealed class ServiceProgressTracker
    {
        private readonly object _sync = new();
        private readonly Dictionary<Type, ServiceRegistrationInfo> _registrations;
        private readonly Dictionary<Type, float> _localProgress;
        private readonly IProgress<ServiceInitializationProgress>? _overallProgress;
        private readonly float _totalWeight;

        public ServiceProgressTracker(IEnumerable<ServiceRegistrationInfo> registrations, IProgress<ServiceInitializationProgress>? overallProgress)
        {
            ArgumentNullException.ThrowIfNull(registrations);

            _registrations = registrations.ToDictionary(registration => registration.ServiceType);
            _localProgress = _registrations.Keys.ToDictionary(serviceType => serviceType, _ => 0f);
            _overallProgress = overallProgress;
            _totalWeight = _registrations.Values.Sum(registration => registration.LoadingWeight);

            if (_totalWeight <= 0f)
            {
                throw new InvalidOperationException("The total loading weight of selected services must be greater than zero.");
            }
        }

        public IProgress<float> CreateServiceProgress(ServiceRegistrationInfo registration)
        {
            ArgumentNullException.ThrowIfNull(registration);
            if (!_registrations.ContainsKey(registration.ServiceType))
            {
                throw new ArgumentException("The registration is not tracked by this progress tracker", nameof(registration));
            }

            return new CallbackProgress<float>(value => Report(registration.ServiceType, value));
        }

        public void Report(Type serviceType, float localProgress)
        {
            if (!float.IsFinite(localProgress))
            {
                throw new ArgumentOutOfRangeException(nameof(localProgress), "Progress must be a finite number.");
            }

            ServiceInitializationProgress update;
            lock (_sync)
            {
                if (!_registrations.ContainsKey(serviceType))
                {
                    throw new KeyNotFoundException(
                        $"No selected service is registered for {serviceType.FullName}.");
                }

                float clamped = Math.Clamp(localProgress, 0f, 1f);

                // Never let a service make the loading bar move backward.
                _localProgress[serviceType] = Math.Max(
                    _localProgress[serviceType],
                    clamped);

                float weightedProgress = 0f;

                foreach ((Type type, ServiceRegistrationInfo registration) in _registrations)
                {
                    weightedProgress +=
                        registration.LoadingWeight * _localProgress[type];
                }

                update = new ServiceInitializationProgress(
                    weightedProgress / _totalWeight,
                    serviceType,
                    _localProgress[serviceType]);
            }

            _overallProgress?.Report(update);
        }

        public void Complete()
        {
            foreach (Type serviceType in _registrations.Keys)
            {
                Report(serviceType, 1f);
            }
        }

        private sealed class CallbackProgress<T> : IProgress<T>
        {
            private readonly Action<T> _report;

            public CallbackProgress(Action<T> report)
            {
                _report = report;
            }

            public void Report(T value)
            {
                _report(value);
            }
        }
    }
}