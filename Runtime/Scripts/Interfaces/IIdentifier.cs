using System;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Interfaces
{
    /// <summary>
    /// Represents an object that can be uniquely identified
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