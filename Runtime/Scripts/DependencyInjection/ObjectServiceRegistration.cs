using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectCI.CoreSystem.DependencyInjection
{
    [Serializable]
    public abstract class ObjectServiceRegistration
    {
        [SerializeField] protected string serviceTypeName;
        public abstract string ServiceTypeName { get; }
        protected internal abstract object ObjectInstance { get; }

        public Type GetServiceType()
        {
            return Type.GetType(serviceTypeName);
        }

        /// <summary>
        /// Check if this registration is valid
        /// </summary>
        /// <returns>True if both service type and instance are valid</returns>
        public virtual bool IsValid() => false;

        public virtual bool IsInstanceValid() => false;
    }
    
    /// <summary>
    /// Serializable ScriptableObject service registration for DI configuration
    /// </summary>
    [Serializable]
    public class UnityObjectServiceRegistration : ObjectServiceRegistration
    {
        [SerializeField] protected Object objectInstance;
        
        protected internal override object ObjectInstance => objectInstance;

        public override string ServiceTypeName => serviceTypeName;

        public UnityObjectServiceRegistration()
        {
            serviceTypeName = "";
            objectInstance = null;
        }

        public UnityObjectServiceRegistration(Type serviceType, Object instance, ServiceLifetime lifetime)
        {
            serviceTypeName = serviceType?.FullName ?? "";
            objectInstance = instance;
        }
        
        /// <summary>
        /// Check if this registration is valid
        /// </summary>
        /// <returns>True if both service type and instance are valid</returns>
        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(serviceTypeName) &&
                   objectInstance != null && 
                   GetServiceType() != null;
        }

        /// <summary>
        /// Check if the ScriptableObject instance implements the service type
        /// </summary>
        /// <returns>True if the instance implements the service type</returns>
        public override bool IsInstanceValid()
        {
            var serviceType = GetServiceType();
            if (serviceType == null || objectInstance == null)
                return false;

            return serviceType.IsAssignableFrom(objectInstance.GetType());
        }
    }
} 