using System;
using System.Collections.Generic;
using UnityEngine;

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

        private readonly Dictionary<Type, ServiceDescriptor> _services = new Dictionary<Type, ServiceDescriptor>();
        private readonly Dictionary<Type, object> _singletonInstances = new Dictionary<Type, object>();

        /// <summary>
        /// Register a service with singleton lifetime
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <typeparam name="TImplementation">Implementation type</typeparam>
        public void RegisterSingleton<TService, TImplementation>() where TImplementation : class, TService
        {
            Register<TService, TImplementation>(ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Register a service with singleton lifetime using factory
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <param name="factory">Factory function to create the service</param>
        public void RegisterSingleton<TService>(Func<TService> factory) where TService : class
        {
            Register<TService>(factory, ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Register a service with transient lifetime
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <typeparam name="TImplementation">Implementation type</typeparam>
        public void RegisterTransient<TService, TImplementation>() where TImplementation : class, TService
        {
            Register<TService, TImplementation>(ServiceLifetime.Transient);
        }

        /// <summary>
        /// Register a service with transient lifetime using factory
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <param name="factory">Factory function to create the service</param>
        public void RegisterTransient<TService>(Func<TService> factory) where TService : class
        {
            Register<TService>(factory, ServiceLifetime.Transient);
        }

        /// <summary>
        /// Register an instance as singleton
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <param name="instance">Instance to register</param>
        public void RegisterInstance<TService>(TService instance) where TService : class
        {
            var serviceType = typeof(TService);
            var descriptor = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = serviceType,
                Lifetime = ServiceLifetime.Singleton,
                Instance = instance
            };

            _services[serviceType] = descriptor;
            _singletonInstances[serviceType] = instance;
        }

        /// <summary>
        /// Resolve a service
        /// </summary>
        /// <typeparam name="TService">Service type to resolve</typeparam>
        /// <returns>Resolved service instance</returns>
        public TService Resolve<TService>() where TService : class
        {
            return (TService)Resolve(typeof(TService));
        }

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
        /// Register services from a ScriptableObject configuration
        /// </summary>
        /// <param name="configuration">The DI configuration to load</param>
        public void LoadConfiguration(DIConfiguration configuration)
        {
            if (configuration != null)
            {
                configuration.RegisterServices(this);
            }
        }

        /// <summary>
        /// Register a service with singleton lifetime by type
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="implementationType">Implementation type</param>
        public void RegisterSingleton(Type serviceType, Type implementationType)
        {
            var descriptor = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Lifetime = ServiceLifetime.Singleton
            };

            _services[serviceType] = descriptor;
        }

        /// <summary>
        /// Register a service with transient lifetime by type
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="implementationType">Implementation type</param>
        public void RegisterTransient(Type serviceType, Type implementationType)
        {
            var descriptor = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Lifetime = ServiceLifetime.Transient
            };

            _services[serviceType] = descriptor;
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

        /// <summary>
        /// Register a service with transient lifetime by type using factory
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="factory">Factory function to create the service</param>
        public void RegisterTransient(Type serviceType, Func<object> factory)
        {
            var descriptor = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = serviceType,
                Lifetime = ServiceLifetime.Transient,
                Factory = factory
            };

            _services[serviceType] = descriptor;
        }

        /// <summary>
        /// Check if a service is registered by type
        /// </summary>
        /// <param name="serviceType">Service type to check</param>
        /// <returns>True if service is registered</returns>
        public bool IsRegistered(Type serviceType)
        {
            return _services.ContainsKey(serviceType);
        }

        private void Register<TService, TImplementation>(ServiceLifetime lifetime) where TImplementation : class, TService
        {
            var serviceType = typeof(TService);
            var implementationType = typeof(TImplementation);

            var descriptor = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Lifetime = lifetime
            };

            _services[serviceType] = descriptor;
        }

        private void Register<TService>(Func<TService> factory, ServiceLifetime lifetime) where TService : class
        {
            var serviceType = typeof(TService);

            var descriptor = new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = serviceType,
                Lifetime = lifetime,
                Factory = () => factory()
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

            var implementationType = descriptor.ImplementationType;
            var constructors = implementationType.GetConstructors();

            if (constructors.Length == 0)
            {
                return Activator.CreateInstance(implementationType);
            }

            // Use the constructor with the most parameters
            var constructor = constructors[0];
            var parameters = constructor.GetParameters();
            var parameterInstances = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                parameterInstances[i] = Resolve(parameters[i].ParameterType);
            }

            return Activator.CreateInstance(implementationType, parameterInstances);
        }
    }
} 