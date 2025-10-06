using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Data;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Attributes
{
    public class UnitAttributeContainer
    {
        private UnitDiscreteResource _health;
        private UnitDiscreteResource _stamina;

        public UnitDiscreteResource Health 
            => _health ??= new UnitDiscreteResource(0, 0);
        public UnitDiscreteResource Stamina 
            => _stamina ??= new UnitDiscreteResource(0, 0);

        // Base attributes
        private readonly Dictionary<AttributeType, int> _generalAttributes = new();
        protected Dictionary<AttributeType, int> GeneralAttributes => _generalAttributes;

        // Accumulated modifiers for each attribute type
        private Dictionary<AttributeType, AttributeModifiers> _attributeModifiers = new Dictionary<AttributeType, AttributeModifiers>();
        protected Dictionary<AttributeType, AttributeModifiers> AttributeModifiers => _attributeModifiers;

        // Get final attribute value (including all modifiers)
        public virtual int GetAttributeValue(AttributeType type)
        {
            var baseValue = _generalAttributes.GetValueOrDefault(type, 0);
            return _attributeModifiers.TryGetValue(type, out var modifiers)
                ? modifiers.CalculateFinalValue(baseValue)
                : baseValue;
        }

        // Set base attribute value
        public virtual void SetGeneralAttribute(AttributeType type, int value)
        {
            var baseValue = _generalAttributes.GetValueOrDefault(type, 0);
            _generalAttributes[type] = baseValue + value;
        }

        // Add attribute modifier
        public virtual void AddModifier(AttributeType type, AttributeModifier modifier)
        {
            if (!_attributeModifiers.ContainsKey(type))
            {
                _attributeModifiers[type] = new AttributeModifiers();
            }

            _attributeModifiers[type].AddModifier(modifier);
        }

        // Clear all modifiers for a specific attribute type
        public virtual void ClearModifiers(AttributeType type)
        {
            if (_attributeModifiers.ContainsKey(type))
            {
                _attributeModifiers[type] = new AttributeModifiers();
            }
        }

        // Clear all modifiers
        public void ClearAllModifiers()
        {
            _attributeModifiers.Clear();
        }
    }

    // Accumulated modifiers for a single attribute type
    public class AttributeModifiers
    {
        private const float MinPercent = -1f;
        private const float MaxPercent = 999f;

        public int FlatValue { get; private set; } = 0;
        public float PercentAddValue { get; private set; } = 0f;
        public float PercentExtra { get; private set; } = 1f;

        public void AddModifier(AttributeModifier modifier)
        {
            switch (modifier.Type)
            {
                case AttributeModifier.ModifierType.Flat:
                    FlatValue += modifier.Value;
                    break;
                case AttributeModifier.ModifierType.PercentAdd:
                    float percentAdd = (float)modifier.Value / 100f;
                    PercentAddValue = Mathf.Clamp(PercentAddValue + percentAdd, MinPercent, MaxPercent);
                    break;
                case AttributeModifier.ModifierType.PercentExtra:
                    float percentExtra = (float)modifier.Value / 100f;
                    PercentExtra = 
                        Mathf.Clamp(PercentExtra + percentExtra, MinPercent, MaxPercent);
                    break;
            }
        }

        public int CalculateFinalValue(int baseValue)
        {
            // Apply all modifiers in order: Flat -> PercentAdd -> PercentMultiply
            float finalValue = Mathf.Floor((float)(baseValue + FlatValue) * (1f + PercentAddValue));
            finalValue *= PercentExtra;
            return Mathf.FloorToInt(finalValue);
        }
    }

    // Attribute modifier
    public class AttributeModifier
    {
        public enum ModifierType
        {
            Flat,           // Direct addition/subtraction
            PercentAdd,     // Percentage addition
            PercentExtra // Percentage multiplication
        }

        public ModifierType Type { get; private set; }
        public int Value { get; private set; }

        public AttributeModifier(ModifierType type, int value)
        {
            Type = type;
            Value = value;
        }
    }
} 