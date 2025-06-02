using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Services
{
    /// <summary>
    /// Container class that manages service instances
    /// </summary>
    public class ServiceContainer
    {
        private readonly Dictionary<Type, IService> _services = new();
        private readonly object _lock = new();

        /// <summary>
        /// Register a service instance
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <param name="service">Service instance</param>
        /// <returns>True if registration was successful, false if service is null or already registered</returns>
        public bool Register<T>(T service) where T : class, IService
        {
            if (service == null)
            {
                Debug.LogWarning("[ServiceContainer] Cannot register null service");
                return false;
            }

            var type = typeof(T);
            lock (_lock)
            {
                if (_services.ContainsKey(type))
                {
                    Debug.LogWarning($"[ServiceContainer] Service of type {type.Name} is already registered");
                    return false;
                }

                _services[type] = service;
                service.Initialize();
                Debug.Log($"[ServiceContainer] Registered service: {type.Name}");
                return true;
            }
        }

        /// <summary>
        /// Get a service instance
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Service instance or null if not found</returns>
        public T Get<T>() where T : class, IService
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (!_services.TryGetValue(type, out var service))
                {
                    Debug.LogWarning($"[ServiceContainer] Service of type {type.Name} is not registered");
                    return null;
                }

                return (T)service;
            }
        }

        /// <summary>
        /// Remove a service instance
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>True if service was removed, false if it wasn't registered</returns>
        public bool Remove<T>() where T : class, IService
        {
            var type = typeof(T);
            lock (_lock)
            {
                if (!_services.TryGetValue(type, out var service))
                {
                    Debug.LogWarning($"[ServiceContainer] Service of type {type.Name} is not registered");
                    return false;
                }

                service.Cleanup();
                service.Dispose();
                _services.Remove(type);
                Debug.Log($"[ServiceContainer] Removed service: {type.Name}");
                return true;
            }
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>True if service is registered, false otherwise</returns>
        public bool HasService<T>() where T : class, IService
        {
            var type = typeof(T);
            lock (_lock)
            {
                return _services.ContainsKey(type);
            }
        }

        /// <summary>
        /// Reset the container by cleaning up and removing all services
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                foreach (var service in _services.Values)
                {
                    service.Cleanup();
                    service.Dispose();
                }
                _services.Clear();
                Debug.Log("[ServiceContainer] Reset all services");
            }
        }
    }
} 