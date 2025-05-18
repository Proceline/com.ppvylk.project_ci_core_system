using ProjectCI_CoreSystem.Runtime.Scripts.Interfaces;
using ProjectCI_CoreSystem.Runtime.Scripts.Enums;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Units.Interfaces
{
    /// <summary>
    /// Represents a unit in the game that can perform actions, move, and interact with other units
    /// </summary>
    public interface IUnit : IIdentifier
    {
        /// <summary>
        /// Current state of the unit
        /// </summary>
        UnitState CurrentState { get; }

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
        /// Current movement points available to the unit
        /// </summary>
        int CurrentMovementPoints { get; }

        /// <summary>
        /// Current ability points available to the unit
        /// </summary>
        int CurrentAbilityPoints { get; }

        /// <summary>
        /// Initialize the unit with its data
        /// </summary>
        /// <param name="data">The unit's data configuration</param>
        void Initialize(IUnitData data);

        /// <summary>
        /// Called after initialization is complete
        /// </summary>
        void PostInitialize();

        /// <summary>
        /// Called when the unit is selected
        /// </summary>
        void OnSelected();

        /// <summary>
        /// Called when the unit is deselected
        /// </summary>
        void OnDeselected();

        /// <summary>
        /// Clean up resources when the unit is destroyed
        /// </summary>
        void Cleanup();
    }
} 