using System;
using UnityEngine;

namespace ProjectCI.CoreSystem.DependencyInjection
{
    /// <summary>
    /// Serializable ScriptableObject service registration for DI configuration
    /// </summary>
    [Serializable]
    public class ScriptableObjectServiceRegistration
    {
        [SerializeField] private string serviceTypeName;
        [SerializeField] private ScriptableObject scriptableObjectInstance;
        [SerializeField] private ServiceLifetime lifetime;

        public string ServiceTypeName => serviceTypeName;
        public ScriptableObject ScriptableObjectInstance => scriptableObjectInstance;
        public ServiceLifetime Lifetime => lifetime;

        public ScriptableObjectServiceRegistration()
        {
            serviceTypeName = "";
            scriptableObjectInstance = null;
            lifetime = ServiceLifetime.Singleton;
        }

        public ScriptableObjectServiceRegistration(Type serviceType, ScriptableObject instance, ServiceLifetime lifetime)
        {
            serviceTypeName = serviceType?.FullName ?? "";
            scriptableObjectInstance = instance;
            this.lifetime = lifetime;
        }

        public Type GetServiceType()
        {
            return Type.GetType(serviceTypeName);
        }

        /// <summary>
        /// Check if this registration is valid
        /// </summary>
        /// <returns>True if both service type and instance are valid</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(serviceTypeName) && 
                   scriptableObjectInstance != null && 
                   GetServiceType() != null;
        }

        /// <summary>
        /// Check if the ScriptableObject instance implements the service type
        /// </summary>
        /// <returns>True if the instance implements the service type</returns>
        public bool IsInstanceValid()
        {
            var serviceType = GetServiceType();
            if (serviceType == null || scriptableObjectInstance == null)
                return false;

            return serviceType.IsAssignableFrom(scriptableObjectInstance.GetType());
        }
    }
} 