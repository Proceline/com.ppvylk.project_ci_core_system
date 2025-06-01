using System;

namespace com.ppvylk.project_ci_core_system.ServiceLocator
{
    /// <summary>
    /// Base interface for all services
    /// </summary>
    public interface IService : IDisposable
    {
        /// <summary>
        /// Initialize the service
        /// </summary>
        void Initialize();

        /// <summary>
        /// Cleanup the service
        /// </summary>
        void Cleanup();
    }
} 