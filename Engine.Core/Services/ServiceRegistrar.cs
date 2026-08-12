using System.Reflection;

using Engine.Core.Services.Attributes;

namespace Engine.Core.Services
{
    public sealed class ServiceRegistrar
    {
        private readonly Dictionary<Type, List<ServiceRegistrationInfo>> _registeredServices = [];

        public void ScanAssemblies(Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                ScanAssembly(assembly);
            }
        }

        public void ScanAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                RegisterServiceAttribute? registerServiceAttribute = type.GetCustomAttribute<RegisterServiceAttribute>();
                if (registerServiceAttribute != null)
                {
                    RegisterService(registerServiceAttribute.ServiceType, type, registerServiceAttribute.Priority, registerServiceAttribute.FailureBehavior);
                }
            }
        }

        public void RegisterService(Type serviceType, Type implementationType, int priority, ServiceFailureBehavior failureBehavior)
        {
            ArgumentNullException.ThrowIfNull(serviceType, nameof(serviceType));
            ArgumentNullException.ThrowIfNull(implementationType, nameof(implementationType));

            if (!serviceType.IsInterface)
            {
                throw new ArgumentException($"{serviceType.FullName} is not an interface.", nameof(serviceType));
            }

            if (!typeof(IService).IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"{serviceType.Name} must implement {nameof(IService)}", nameof(serviceType));
            }

            if (!serviceType.IsAssignableFrom(implementationType))
            {
                throw new ArgumentException($"{implementationType.Name} does not implement {serviceType.Name}.", nameof(serviceType));
            }

            if (serviceType == implementationType)
            {
                throw new ArgumentException($"{serviceType.Name} cannot be registered as its own implementation type.", nameof(serviceType));
            }

            if (implementationType.IsAbstract)
            {
                throw new ArgumentException($"{implementationType.Name} is abstract and cannot be registered.", nameof(implementationType));
            }

            if (!_registeredServices.TryGetValue(serviceType, out List<ServiceRegistrationInfo> services))
            {
                services = [];
                _registeredServices[serviceType] = services;
            }
        }

        public void RemoveAssembly(Assembly assembly)
        {
            foreach (List<ServiceRegistrationInfo> services in _registeredServices.Values)
            {
                services.RemoveAll(s => s.SourceAssembly == assembly);
            }
        }

        public IReadOnlyList<ServiceRegistrationInfo> GetRegisteredServices(Type serviceType)
        {
            if (_registeredServices.TryGetValue(serviceType, out List<ServiceRegistrationInfo>? services))
            {
                return services;
            }
            else
            {
                return Array.Empty<ServiceRegistrationInfo>();
            }
        }
    }
}
