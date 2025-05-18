using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Params
{
    [CreateAssetMenu(fileName = "NewBattleHealParam", menuName = "ProjectCI/Ability/Parameters/Create BattleHealParam", order = 1)]
    public class BattleHealParam : BattleAbilityParam
    {
        public int m_HealAmount;

        public override void ApplyTo(IUnit InCaster, IObject InTarget)
        {
            // TODO: Implement heal logic
            // InTarget.Heal(m_HealAmount);
        }

        public override void ApplyTo(IUnit InCaster, Vector2Int InCell)
        {
            // TODO: Implement cell-based heal logic
        }

        public override string GetAbilityInfo()
        {
            return "Heal Target by: " + m_HealAmount;
        }
    }
} 