using ProjectCI_CoreSystem.Runtime.Scripts.Units.Interfaces;
using UnityEngine;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Units.Data
{
    /// <summary>
    /// Represents a resource that uses integer values for discrete calculations
    /// </summary>
    public class UnitDiscreteResource : IUnitResource<int>
    {
        private int currentValue;
        private int maxValue;
        private int minValue;

        public int CurrentValue => currentValue;
        public int MaxValue => maxValue;
        public int MinValue => minValue;

        public UnitDiscreteResource(int initialValue, int minValue = 0)
        {
            this.currentValue = initialValue;
            this.maxValue = initialValue;
            this.minValue = minValue;
        }

        public void ModifyValue(int amount)
        {
            currentValue = Mathf.Clamp(currentValue + amount, minValue, maxValue);
        }

        public void SetValue(int value)
        {
            currentValue = Mathf.Clamp(value, minValue, maxValue);
        }

        public void SetValue(int current, int max)
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