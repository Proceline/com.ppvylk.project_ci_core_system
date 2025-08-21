namespace ProjectCI.CoreSystem.Runtime.Interfaces
{
    /// <summary>
    /// Represents an object that can be uniquely identified
    /// </summary>
    public interface IIdentifier
    {
        /// <summary>
        /// Gets the unique identifier of the object
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Generates a new unique identifier for the object
        /// </summary>
        void GenerateNewID();
    }
} 