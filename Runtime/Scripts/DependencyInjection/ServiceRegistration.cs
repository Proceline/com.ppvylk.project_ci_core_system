using System;
using UnityEngine;

namespace ProjectCI.CoreSystem.DependencyInjection
{
    /// <summary>
    /// Serializable service registration for ScriptableObject configuration
    /// </summary>
    [Serializable]
    public class ServiceRegistration
    {
        [SerializeField] private string serviceTypeName;
        [SerializeField] private string implementationTypeName;
        [SerializeField] private ServiceLifetime lifetime;
        [SerializeField] private bool useFactory;
        [SerializeField] private string factoryMethodName;

        public string ServiceTypeName => serviceTypeName;
        public string ImplementationTypeName => implementationTypeName;
        public ServiceLifetime Lifetime => lifetime;
        public bool UseFactory => useFactory;
        public string FactoryMethodName => factoryMethodName;

        public ServiceRegistration()
        {
            serviceTypeName = "";
            implementationTypeName = "";
            lifetime = ServiceLifetime.Singleton;
            useFactory = false;
            factoryMethodName = "";
        }

        public ServiceRegistration(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            serviceTypeName = serviceType?.FullName ?? "";
            implementationTypeName = implementationType?.FullName ?? "";
            this.lifetime = lifetime;
            useFactory = false;
            factoryMethodName = "";
        }

        public ServiceRegistration(Type serviceType, string factoryMethodName, ServiceLifetime lifetime)
        {
            serviceTypeName = serviceType?.FullName ?? "";
            implementationTypeName = "";
            this.lifetime = lifetime;
            useFactory = true;
            this.factoryMethodName = factoryMethodName;
        }

        public Type GetServiceType()
        {
            return Type.GetType(serviceTypeName);
        }

        public Type GetImplementationType()
        {
            return Type.GetType(implementationTypeName);
        }
    }
} 