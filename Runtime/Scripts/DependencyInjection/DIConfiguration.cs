using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ProjectCI.CoreSystem.DependencyInjection
{
    /// <summary>
    /// ScriptableObject-based DI configuration that automatically manages the DI container
    /// </summary>
    [CreateAssetMenu(fileName = "DIConfiguration", menuName = "ProjectCI/DI Configuration")]
    public class DIConfiguration : ScriptableObject
    {
        [Header("Service Registrations")]
        [SerializeField] private List<ServiceRegistration> serviceRegistrations = new List<ServiceRegistration>();
        
        [Header("ScriptableObject Service Registrations")]
        [SerializeField] private List<ScriptableObjectServiceRegistration> scriptableObjectRegistrations = new List<ScriptableObjectServiceRegistration>();

        private static DIConfiguration _instance;
        public DIContainer Container => DIContainer.Instance;

        /// <summary>
        /// Register all services from this configuration to the DI container
        /// </summary>
        /// <param name="container">The DI container to register services to</param>
        public void RegisterServices(DIContainer container)
        {
            // Register manually configured services
            foreach (var registration in serviceRegistrations)
            {
                RegisterService(container, registration);
            }

            // Register ScriptableObject services
            foreach (var registration in scriptableObjectRegistrations)
            {
                RegisterScriptableObjectService(container, registration);
            }
        }

        private void RegisterService(DIContainer container, ServiceRegistration registration)
        {
            var serviceType = registration.GetServiceType();
            if (serviceType == null)
            {
                Debug.LogWarning($"Could not resolve service type: {registration.ServiceTypeName}");
                return;
            }

            if (registration.UseFactory)
            {
                RegisterWithFactory(container, registration, serviceType);
            }
            else
            {
                RegisterWithImplementation(container, registration, serviceType);
            }
        }

        private void RegisterWithImplementation(DIContainer container, ServiceRegistration registration, Type serviceType)
        {
            var implementationType = registration.GetImplementationType();
            if (implementationType == null)
            {
                Debug.LogWarning($"Could not resolve implementation type: {registration.ImplementationTypeName}");
                return;
            }

            if (registration.Lifetime == ServiceLifetime.Singleton)
            {
                container.RegisterSingleton(serviceType, implementationType);
            }
            else
            {
                container.RegisterTransient(serviceType, implementationType);
            }
        }

        private void RegisterWithFactory(DIContainer container, ServiceRegistration registration, Type serviceType)
        {
            // Try to find factory method in this configuration
            var factoryMethod = GetType().GetMethod(registration.FactoryMethodName, 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (factoryMethod != null && factoryMethod.ReturnType == serviceType)
            {
                var factory = (Func<object>)factoryMethod.CreateDelegate(typeof(Func<object>), this);
                
                if (registration.Lifetime == ServiceLifetime.Singleton)
                {
                    container.RegisterSingleton(serviceType, factory);
                }
                else
                {
                    container.RegisterTransient(serviceType, factory);
                }
            }
            else
            {
                Debug.LogWarning($"Could not find factory method: {registration.FactoryMethodName}");
            }
        }

        private void RegisterScriptableObjectService(DIContainer container, ScriptableObjectServiceRegistration registration)
        {
            if (!registration.IsValid())
            {
                Debug.LogWarning($"Invalid ScriptableObject service registration: {registration.ServiceTypeName}");
                return;
            }

            if (!registration.IsInstanceValid())
            {
                Debug.LogWarning($"ScriptableObject instance does not implement service type: {registration.ServiceTypeName}");
                return;
            }

            var serviceType = registration.GetServiceType();
            var instance = registration.ScriptableObjectInstance;

            // ScriptableObjects are always registered as singletons since they are assets
            container.RegisterSingleton(serviceType, () => instance);
            Debug.Log($"Registered ScriptableObject service: {serviceType.Name} -> {instance.name}");
        }

        #region Scene Management

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            if (_instance == null)
            {
                var instances = Resources.LoadAll<DIConfiguration>("");
                if (instances.Length != 1)
                {
                    Debug.LogWarning("Multiple/No DIConfiguration found in Resources folder. Please create ONE only.");
                }
                else
                {
                    _instance = instances[0];
                    _instance.RegisterServices(_instance.Container);
                }
            }
        }

        #endregion

        public static void InjectFromConfiguration<T>(T target) where T : new()
        {
            if (_instance == null)
            {
                Debug.LogWarning("No DIConfiguration found in Resources folder. Please create ONE only.");
                return;
            }

            _instance.Inject(target);
        }

        /// <summary>
        /// Inject dependencies into any object
        /// </summary>
        /// <param name="target">The object to inject into</param>
        public void Inject(object target)
        {
            Container.Inject(target);
        }

        /// <summary>
        /// Resolve a service from the container
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Resolved service instance</returns>
        public T Resolve<T>() where T : class
        {
            return Container.Resolve<T>();
        }

        /// <summary>
        /// Try to resolve a service, returns null if not registered
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>Resolved service or null</returns>
        public T TryResolve<T>() where T : class
        {
            return Container.TryResolve<T>();
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <returns>True if registered</returns>
        public bool IsRegistered<T>()
        {
            return Container.IsRegistered<T>();
        }

        /// <summary>
        /// Clear all service registrations
        /// </summary>
        public void Clear()
        {
            Container.Clear();
        }
    }
} 