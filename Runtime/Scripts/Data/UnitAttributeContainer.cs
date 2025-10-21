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

        // Get final attribute value (including all modifiers)
        public virtual int GetAttributeValue(AttributeType type)
        {
            return _generalAttributes.GetValueOrDefault(type, 0);
        }

        // Set base attribute value
        public virtual void SetGeneralAttribute(AttributeType type, int value)
        {
            var baseValue = _generalAttributes.GetValueOrDefault(type, 0);
            _generalAttributes[type] = baseValue + value;
        }
    }
} 