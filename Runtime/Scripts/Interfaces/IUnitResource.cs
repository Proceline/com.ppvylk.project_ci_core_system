namespace ProjectCI_CoreSystem.Runtime.Scripts.Interfaces
{
    /// <summary>
    /// Represents a resource that a unit can have (e.g., health, mana, energy)
    /// </summary>
    public interface IUnitResource
    {
        /// <summary>
        /// Gets the current value of the resource
        /// </summary>
        float CurrentValue { get; }

        /// <summary>
        /// Gets the maximum value of the resource
        /// </summary>
        float MaxValue { get; }

        /// <summary>
        /// Gets the minimum value of the resource
        /// </summary>
        float MinValue { get; }

        /// <summary>
        /// Modifies the current value of the resource
        /// </summary>
        /// <param name="amount">Amount to modify (positive for increase, negative for decrease)</param>
        void ModifyValue(float amount);

        /// <summary>
        /// Sets the current value of the resource
        /// </summary>
        /// <param name="value">Value to set</param>
        void SetValue(float value);

        /// <summary>
        /// Resets the resource to its maximum value
        /// </summary>
        void ResetToMax();

        /// <summary>
        /// Resets the resource to its minimum value
        /// </summary>
        void ResetToMin();
    }
} 