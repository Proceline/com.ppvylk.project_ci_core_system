using UnityEngine;
using ProjectCI_CoreSystem.Runtime.Scripts.Units.Interfaces;
using ProjectCI_CoreSystem.Runtime.Scripts.Interfaces;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Abilities.Params
{
    /// <summary>
    /// Base class for all ability parameters
    /// </summary>
    public abstract class BattleAbilityParam : ScriptableObject
    {
        public abstract void ApplyTo(IUnit InCaster, IObject InTarget);
        
        public abstract void ApplyTo(IUnit InCaster, Vector2Int InCell);

        public abstract string GetAbilityInfo();
    }
} 