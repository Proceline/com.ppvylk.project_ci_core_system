using System;
using System.Reflection;
using UnityEngine;

namespace ProjectCI.CoreSystem.DependencyInjection
{
    /// <summary>
    /// Extension methods for dependency injection
    /// </summary>
    public static class DIExtensions
    {
        /// <summary>
        /// Inject dependencies into any object using reflection
        /// </summary>
        /// <param name="container">The DI container</param>
        /// <param name="target">The object to inject into</param>
        public static void Inject(this DIContainer container, object target)
        {
            var type = target.GetType();
            
            // Inject fields
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var injectAttribute = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null)
                {
                    var serviceType = injectAttribute.ServiceType ?? field.FieldType;
                    var service = container.Resolve(serviceType);
                    field.SetValue(target, service);
                }
            }

            // Inject properties
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                var injectAttribute = property.GetCustomAttribute<InjectAttribute>();
                if (injectAttribute != null && property.CanWrite)
                {
                    var serviceType = injectAttribute.ServiceType ?? property.PropertyType;
                    var service = container.Resolve(serviceType);
                    property.SetValue(target, service);
                }
            }
        }

        /// <summary>
        /// Register a service with singleton lifetime if not already registered
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <typeparam name="TImplementation">Implementation type</typeparam>
        /// <param name="container">The DI container</param>
        public static void RegisterSingletonIfNotRegistered<TService, TImplementation>(this DIContainer container) 
            where TImplementation : class, TService
        {
            if (!container.IsRegistered<TService>())
            {
                container.RegisterSingleton<TService, TImplementation>();
            }
        }

        /// <summary>
        /// Register a service with transient lifetime if not already registered
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <typeparam name="TImplementation">Implementation type</typeparam>
        /// <param name="container">The DI container</param>
        public static void RegisterTransientIfNotRegistered<TService, TImplementation>(this DIContainer container) 
            where TImplementation : class, TService
        {
            if (!container.IsRegistered<TService>())
            {
                container.RegisterTransient<TService, TImplementation>();
            }
        }

        /// <summary>
        /// Try to resolve a service, returns null if not registered
        /// </summary>
        /// <typeparam name="TService">Service type to resolve</typeparam>
        /// <param name="container">The DI container</param>
        /// <returns>Resolved service or null if not registered</returns>
        public static TService TryResolve<TService>(this DIContainer container) where TService : class
        {
            if (container.IsRegistered<TService>())
            {
                return container.Resolve<TService>();
            }
            return null;
        }
    }
} 