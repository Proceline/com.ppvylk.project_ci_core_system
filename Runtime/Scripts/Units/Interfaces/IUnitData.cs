namespace ProjectCI.CoreSystem.Runtime.Units.Interfaces
{
    /// <summary>
    /// Represents the data configuration for a unit
    /// </summary>
    public interface IUnitData
    {
        /// <summary>
        /// Name of the unit
        /// </summary>
        string UnitName { get; }

        /// <summary>
        /// Whether the unit can fly
        /// </summary>
        bool IsFlying { get; }

        /// <summary>
        /// Whether the unit should look at its targets
        /// </summary>
        bool ShouldLookAtTargets { get; }

        /// <summary>
        /// Height offset for the unit
        /// </summary>
        float HeightOffset { get; }

        /// <summary>
        /// Maximum movement points the unit can have
        /// </summary>
        int MaxMovementPoints { get; }

        /// <summary>
        /// Maximum ability points the unit can have
        /// </summary>
        int MaxAbilityPoints { get; }

        /// <summary>
        /// Maximum health the unit can have
        /// </summary>
        float MaxHealth { get; }

        /// <summary>
        /// Team the unit belongs to
        /// </summary>
        int Team { get; }
    }
} 