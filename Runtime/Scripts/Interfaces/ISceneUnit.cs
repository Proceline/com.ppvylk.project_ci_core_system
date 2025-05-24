using ProjectCI.CoreSystem.Runtime.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.Units.Interfaces
{
    /// <summary>
    /// Represents a unit in the game that can perform actions, move, and interact with other units
    /// </summary>
    public interface ISceneUnit : IIdentifier, IObject
    {
        /// <summary>
        /// Clean up resources when the unit is destroyed
        /// </summary>
        void CleanUp();
    }
} 