using System;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Interfaces
{
    /// <summary>
    /// Interface for objects that require unique identification
    /// </summary>
    public interface IIdentifier
    {
        /// <summary>
        /// Gets the unique identifier of the object
        /// </summary>
        Guid ID { get; }

        /// <summary>
        /// Generates a new unique identifier for the object
        /// </summary>
        void GenerateNewID();
    }
} 