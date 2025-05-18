using UnityEngine;
using ProjectCI_CoreSystem.Runtime.Scripts.Units.Interfaces;
using ProjectCI_CoreSystem.Runtime.Scripts.Interfaces;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Abilities.Params
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