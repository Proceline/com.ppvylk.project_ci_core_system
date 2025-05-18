using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Params
{
    [CreateAssetMenu(fileName = "NewBattleSelfDamageParam", menuName = "ProjectCI/Ability/Parameters/Create BattleSelfDamageParam", order = 1)]
    public class BattleSelfDamageParam : BattleAbilityParam
    {
        public int m_Damage;
        public bool m_bMagicalDamage;

        public override void ApplyTo(IUnit InCaster, IObject InTarget)
        {
            // TODO: Implement self-damage logic
            // if (m_bMagicalDamage)
            // {
            //     InCaster.TakeMagicalDamage(m_Damage);
            // }
            // else
            // {
            //     InCaster.TakeDamage(m_Damage);
            // }
        }

        public override void ApplyTo(IUnit InCaster, Vector2Int InCell)
        {
            // TODO: Implement cell-based self-damage logic
        }

        public override string GetAbilityInfo()
        {
            return "Damage Self " + (m_bMagicalDamage ? "(Magical)" : "") + " " + m_Damage.ToString();
        }
    }
} 