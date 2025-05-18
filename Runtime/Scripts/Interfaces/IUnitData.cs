namespace ProjectCI_CoreSystem.Runtime.Scripts.Interfaces
{
    /// <summary>
    /// Represents the data configuration for a unit
    /// </summary>
    public interface IUnitData
    {
        /// <summary>
        /// Gets the name of the unit
        /// </summary>
        string UnitName { get; }

        /// <summary>
        /// Gets whether the unit is flying
        /// </summary>
        bool IsFlying { get; }

        /// <summary>
        /// Gets whether the unit should look at targets
        /// </summary>
        bool ShouldLookAtTargets { get; }

        /// <summary>
        /// Gets the height offset of the unit
        /// </summary>
        float HeightOffset { get; }

        /// <summary>
        /// Gets the maximum movement points of the unit
        /// </summary>
        int MaxMovementPoints { get; }

        /// <summary>
        /// Gets the maximum ability points of the unit
        /// </summary>
        int MaxAbilityPoints { get; }

        /// <summary>
        /// Gets the maximum health of the unit
        /// </summary>
        int MaxHealth { get; }

        /// <summary>
        /// Gets the team the unit belongs to
        /// </summary>
        int Team { get; }
    }
} 