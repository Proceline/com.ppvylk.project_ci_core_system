using System;

namespace ProjectCI.CoreSystem.DependencyInjection
{
    /// <summary>
    /// Describes a service registration in the DI container
    /// </summary>
    public class ServiceDescriptor
    {
        /// <summary>
        /// The service type (interface or base class)
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// The implementation type
        /// </summary>
        public Type ImplementationType { get; set; }

        /// <summary>
        /// The lifetime of the service
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }

        /// <summary>
        /// The instance (for singleton instances)
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// Factory function to create the service
        /// </summary>
        public Func<object> Factory { get; set; }
    }
} 