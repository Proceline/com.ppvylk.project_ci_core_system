using System;
using System.Collections.Generic;

namespace ProjectCI.CoreSystem.DependencyInjection
{
    /// <summary>
    /// Dependency Injection Container for managing service registrations and resolutions
    /// </summary>
    public class DIContainer
    {
        private static DIContainer _instance;
        public static DIContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DIContainer();
                }
                return _instance;
            }
        }

        private readonly Dictionary<Type, ServiceDescriptor> _services = new();
        private readonly Dictionary<Type, object> _singletonInstances = new();

        /// <summary>
        /// Resolve a service by type
        /// </summary>
        /// <param name="serviceType">Service type to resolve</param>
        /// <returns>Resolved service instance</returns>
        public object Resolve(Type serviceType)
        {
            if (!_services.TryGetValue(serviceType, out var descriptor))
            {
                throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
            }

            switch (descriptor.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    return ResolveSingleton(descriptor);
                case ServiceLifetime.Transient:
                    return ResolveTransient(descriptor);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        /// <typeparam name="TService">Service type to check</typeparam>
        /// <returns>True if service is registered</returns>
        public bool IsRegistered<TService>()
        {
            return _services.ContainsKey(typeof(TService));
        }

        /// <summary>
        /// Clear all registrations
        /// </summary>
        public void Clear()
        {
            _services.Clear();
            _singletonInstances.Clear();
        }

        /// <summary>
        /// Register a service with singleton lifetime by type using factory
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="factory">Factory function to create the service</param>
        public void RegisterSingleton(Type serviceType, Func<object> factory)
        {
            var descriptor = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = serviceType,
                Lifetime = ServiceLifetime.Singleton,
                Factory = factory
            };

            _services[serviceType] = descriptor;
        }

        private object ResolveSingleton(ServiceDescriptor descriptor)
        {
            if (_singletonInstances.TryGetValue(descriptor.ServiceType, out var instance))
            {
                return instance;
            }

            instance = CreateInstance(descriptor);
            _singletonInstances[descriptor.ServiceType] = instance;
            return instance;
        }

        private object ResolveTransient(ServiceDescriptor descriptor)
        {
            return CreateInstance(descriptor);
        }

        private object CreateInstance(ServiceDescriptor descriptor)
        {
            if (descriptor.Instance != null)
            {
                return descriptor.Instance;
            }

            if (descriptor.Factory != null)
            {
                return descriptor.Factory();
            }

            throw new NullReferenceException("ERROR: No Instance neither Factory method (constructor)");
        }
    }
} 