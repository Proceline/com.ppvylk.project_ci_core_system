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
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
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

        /// <summary>
        /// Inject dependencies into static class
        /// </summary>
        /// <param name="container">The DI container</param>
        /// <param name="typeFullName"></param>
        public static void StaticInject(DIContainer container, string typeFullName)
        {
            var exactType = Type.GetType(typeFullName);
            if (exactType == null)
            {
                return;
            }
            
            var staticFields = exactType.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                   BindingFlags.DeclaredOnly | BindingFlags.Static);
            foreach (var field in staticFields)
            {
                if (field.GetCustomAttribute<InjectAttribute>(false) == null)
                {
                    continue;
                }

                var typeOfField = field.FieldType;
                var service = container.Resolve(typeOfField);

                field.SetValue(null, service);

                Debug.Log(
                    $"<color=#0F0>Static Injection Context </color> is injecting {exactType.Name}.{field.Name} as {field.FieldType.Name}");
            }

        }
    }
} 