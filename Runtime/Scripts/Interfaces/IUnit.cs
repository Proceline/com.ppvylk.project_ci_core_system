using ProjectCI_CoreSystem.Runtime.Scripts.Enums;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Interfaces
{
    /// <summary>
    /// Represents a unit in the game that can perform actions, move, and interact with other units
    /// </summary>
    public interface IUnit : IIdentifier
    {
        /// <summary>
        /// Gets the current state of the unit
        /// </summary>
        UnitState CurrentState { get; }

        /// <summary>
        /// Gets whether the unit is currently moving
        /// </summary>
        bool IsMoving { get; }

        /// <summary>
        /// Gets whether the unit is currently using an ability
        /// </summary>
        bool IsUsingAbility { get; }

        /// <summary>
        /// Gets whether the unit is currently a target
        /// </summary>
        bool IsTarget { get; }

        /// <summary>
        /// Gets whether the unit is currently activated
        /// </summary>
        bool IsActivated { get; }

        /// <summary>
        /// Gets whether the unit is dead
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Gets the current movement points of the unit
        /// </summary>
        int CurrentMovementPoints { get; }

        /// <summary>
        /// Gets the current ability points of the unit
        /// </summary>
        int CurrentAbilityPoints { get; }

        /// <summary>
        /// Initializes the unit with necessary components and data
        /// </summary>
        void Initialize();

        /// <summary>
        /// Performs post-initialization setup
        /// </summary>
        void PostInitialize();

        /// <summary>
        /// Selects the unit for player interaction
        /// </summary>
        void SelectUnit();

        /// <summary>
        /// Cleans up the unit's state and resources
        /// </summary>
        void CleanUp();
    }
} 