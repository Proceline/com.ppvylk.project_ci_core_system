using System;

namespace ProjectCI.CoreSystem.DependencyInjection
{
    /// <summary>
    /// Attribute to mark properties and fields for dependency injection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
        /// <summary>
        /// Optional service type to inject (if different from property/field type)
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public InjectAttribute() { }

        /// <summary>
        /// Constructor with specific service type
        /// </summary>
        /// <param name="serviceType">The service type to inject</param>
        public InjectAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }
    }
} 