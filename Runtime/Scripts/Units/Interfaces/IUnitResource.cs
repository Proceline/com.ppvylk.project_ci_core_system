namespace ProjectCI.CoreSystem.Runtime.Units.Interfaces
{
    /// <summary>
    /// Represents a resource that a unit can have
    /// </summary>
    /// <typeparam name="T">The type of value the resource uses</typeparam>
    public interface IUnitResource<T>
    {
        /// <summary>
        /// Current value of the resource
        /// </summary>
        T CurrentValue { get; }

        /// <summary>
        /// Maximum value the resource can have
        /// </summary>
        T MaxValue { get; }

        /// <summary>
        /// Minimum value the resource can have
        /// </summary>
        T MinValue { get; }

        /// <summary>
        /// Modify the current value by the given amount
        /// </summary>
        /// <param name="amount">Amount to modify by</param>
        void ModifyValue(T amount);

        /// <summary>
        /// Set the current value
        /// </summary>
        /// <param name="value">Value to set to</param>
        void SetValue(T value);

        /// <summary>
        /// Set both current and maximum values
        /// </summary>
        /// <param name="current">Current value to set</param>
        /// <param name="max">Maximum value to set</param>
        void SetValue(T current, T max);

        /// <summary>
        /// Reset the current value to the maximum
        /// </summary>
        void ResetToMax();

        /// <summary>
        /// Reset the current value to the minimum
        /// </summary>
        void ResetToMin();
    }
} 