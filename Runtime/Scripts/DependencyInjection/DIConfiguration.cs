using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.DependencyInjection
{
    /// <summary>
    /// ScriptableObject-based DI configuration that automatically manages the DI container
    /// </summary>
    [CreateAssetMenu(fileName = "DIConfiguration", menuName = "ProjectCI/DI Configuration")]
    public class DIConfiguration : ScriptableObject
    {
        [Header("UnityObject Service Registrations")]
        [SerializeField] private List<UnityObjectServiceRegistration> unityObjectRegistrations = new();

        private static DIConfiguration _instance;
        private DIContainer Container => DIContainer.Instance;

        /// <summary>
        /// Register all services from this configuration to the DI container
        /// </summary>
        /// <param name="container">The DI container to register services to</param>
        private void RegisterServices(DIContainer container)
        {
            foreach (var registration in unityObjectRegistrations)
            {
                RegisterSingletonObjectService(container, registration);
            }
        }

        private void RegisterSingletonObjectService<T>(DIContainer container, T registration)
            where T : ObjectServiceRegistration
        {
            if (!registration.IsValid())
            {
                Debug.LogWarning($"Invalid ScriptableObject service registration: {registration.ServiceTypeName}");
                return;
            }

            if (!registration.IsInstanceValid())
            {
                Debug.LogWarning(
                    $"ScriptableObject instance does not implement service type: {registration.ServiceTypeName}");
                return;
            }

            var serviceType = registration.GetServiceType();
            var instance = registration.ObjectInstance;

            container.RegisterSingleton(serviceType, () => instance);
            Debug.Log($"Registered ScriptableObject service: {serviceType.Name} -> {instance}");
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
        private void Inject(object target)
        {
            Container.Inject(target);
        }
    }
} 