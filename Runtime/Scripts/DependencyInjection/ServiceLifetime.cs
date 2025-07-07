namespace ProjectCI.CoreSystem.DependencyInjection
{
    /// <summary>
    /// Defines the lifetime of a service in the DI container
    /// </summary>
    public enum ServiceLifetime
    {
        /// <summary>
        /// A new instance is created every time the service is requested
        /// </summary>
        Transient,

        /// <summary>
        /// A single instance is created and reused for all requests
        /// </summary>
        Singleton
    }
} 