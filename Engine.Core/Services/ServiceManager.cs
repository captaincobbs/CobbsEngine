using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core.Services
{
    public sealed class ServiceManager
    {
        private readonly Dictionary<Type, IService> _services = [];

        public void Initialize()
        {

        }

        public void Shutdown()
        {

        }

        /// <summary>Gets an instance of the specified service type.</summary>
        /// <typeparam name="T">The implementation type of the service, typically the interface type.</typeparam>
        /// <exception cref="KeyNotFoundException">Thrown when the specified service type is not found.</exception>
        /// <returns>The instance of the service.</returns>
        public T Get<T>() where T : class, IService => (T)_services[typeof(T)];

        /// <summary>Tries to get an instance of the specified service type.</summary>
        /// <typeparam name="T">The implementation type of the service, typically the interface type.</typeparam>
        /// <param name="service">When this method returns, contains the instance of the service if it was found; otherwise, null.</param>
        /// <returns>true if the service was found; otherwise, false.</returns>
        public bool TryGet<T>(out T? service) where T : class, IService
        {
            service = _services.TryGetValue(typeof(T), out IService? value) ? (T)value : null;
            return service != null;
        }
    }
}
