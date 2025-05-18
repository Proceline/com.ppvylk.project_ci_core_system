using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Params
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