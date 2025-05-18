using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Params
{
    [CreateAssetMenu(fileName = "NewBattleDamageParam", menuName = "ProjectCI/Ability/Parameters/Create BattleDamageParam", order = 1)]
    public class BattleDamageParam : BattleAbilityParam
    {
        public int m_Damage;
        public bool m_bMagicalDamage;

        public override void ApplyTo(IUnit InCaster, IObject InTarget)
        {
            // TODO: Implement damage logic
            // if (m_bMagicalDamage)
            // {
            //     InTarget.TakeMagicalDamage(m_Damage);
            // }
            // else
            // {
            //     InTarget.TakeDamage(m_Damage);
            // }
        }

        public override void ApplyTo(IUnit InCaster, Vector2Int InCell)
        {
            // TODO: Implement cell-based damage logic
        }

        public override string GetAbilityInfo()
        {
            return "Damage" + (m_bMagicalDamage ? "(Magical)" : "") + " " + m_Damage.ToString();
        }
    }
} 