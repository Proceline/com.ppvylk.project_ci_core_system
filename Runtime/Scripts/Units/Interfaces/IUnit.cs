using ProjectCI.CoreSystem.Runtime.Interfaces;
using ProjectCI.CoreSystem.Runtime.Enums;

namespace ProjectCI.CoreSystem.Runtime.Units.Interfaces
{
    /// <summary>
    /// Represents a unit in the game that can perform actions, move, and interact with other units
    /// </summary>
    public interface IUnit : IIdentifier, IObject
    {
        /// <summary>
        /// Whether the unit is currently moving
        /// </summary>
        bool IsMoving { get; }

        /// <summary>
        /// Whether the unit is currently using an ability
        /// </summary>
        bool IsUsingAbility { get; }

        /// <summary>
        /// Whether the unit is currently a target
        /// </summary>
        bool IsTarget { get; }

        /// <summary>
        /// Whether the unit is currently activated
        /// </summary>
        bool IsActivated { get; }

        /// <summary>
        /// Whether the unit is dead
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Clean up resources when the unit is destroyed
        /// </summary>
        void CleanUp();
    }
} 