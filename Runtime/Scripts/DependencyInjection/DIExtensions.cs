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
                    if (service is GameObject gameObject)
                    {
                        var component = gameObject.GetComponent(serviceType);
                        field.SetValue(target, component);
                    }
                    else
                    {
                        field.SetValue(target, service);
                    }
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
                    if (service is GameObject gameObject)
                    {
                        var component = gameObject.GetComponent(serviceType);
                        property.SetValue(target, component);
                    }
                    else
                    {
                        property.SetValue(target, service);
                    }
                }
            }
        }
    }
} 