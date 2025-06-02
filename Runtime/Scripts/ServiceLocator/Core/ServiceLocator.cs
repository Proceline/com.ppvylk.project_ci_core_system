using System;

namespace ProjectCI.CoreSystem.Runtime.Services
{
    /// <summary>
    /// Static class that provides global access to services
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly ServiceContainer _container = new();

        /// <summary>
        /// Register a service instance
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="service">Service instance</param>
        /// <returns>True if registration was successful, false if service is null or already registered</returns>
        public static bool Register<T>(T service) where T : class, IService
        {
            return _container.Register(service);
        }

        /// <summary>
        /// Get a service instance
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance or null if not found</returns>
        public static T Get<T>() where T : class, IService
        {
            return _container.Get<T>();
        }

        /// <summary>
        /// Remove a service instance
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>True if service was removed, false if it wasn't registered</returns>
        public static bool Remove<T>() where T : class, IService
        {
            return _container.Remove<T>();
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>True if service is registered, false otherwise</returns>
        public static bool HasService<T>() where T : class, IService
        {
            return _container.HasService<T>();
        }

        /// <summary>
        /// Reset all services
        /// </summary>
        public static void Reset()
        {
            _container.Reset();
        }
    }
} 