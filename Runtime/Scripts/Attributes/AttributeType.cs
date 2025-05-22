using UnityEngine;
using System;

namespace ProjectCI.CoreSystem.Runtime.Attributes
{
    [Serializable]
    public struct AttributeType
    {
        [SerializeField]
        private int value;

        public int Value => value;

        public AttributeType(int value)
        {
            this.value = value;
        }

        public static implicit operator int(AttributeType type) => type.value;
        public static implicit operator AttributeType(int value) => new AttributeType(value);

        public override bool Equals(object obj)
        {
            if (obj is AttributeType other)
            {
                return value == other.value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public static bool operator ==(AttributeType a, AttributeType b)
        {
            return a.value == b.value;
        }

        public static bool operator !=(AttributeType a, AttributeType b)
        {
            return a.value != b.value;
        }
    }
} 