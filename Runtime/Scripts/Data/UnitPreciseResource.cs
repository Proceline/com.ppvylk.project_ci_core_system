using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Data
{
    /// <summary>
    /// Represents a resource that uses float values for precise calculations
    /// </summary>
    public class UnitPreciseResource : IUnitResource<float>
    {
        private float currentValue;
        private float maxValue;
        private float minValue;

        public float CurrentValue => currentValue;
        public float MaxValue => maxValue;
        public float MinValue => minValue;

        public UnitPreciseResource(float initialValue, float minValue = 0f)
        {
            this.currentValue = initialValue;
            this.maxValue = initialValue;
            this.minValue = minValue;
        }

        public void ModifyValue(float amount)
        {
            currentValue = Mathf.Clamp(currentValue + amount, minValue, maxValue);
        }

        public void SetValue(float value)
        {
            currentValue = Mathf.Clamp(value, minValue, maxValue);
        }

        public void SetValue(float current, float max)
        {
            maxValue = max;
            currentValue = Mathf.Clamp(current, minValue, maxValue);
        }

        public void ResetToMax()
        {
            currentValue = maxValue;
        }

        public void ResetToMin()
        {
            currentValue = minValue;
        }
    }
} 