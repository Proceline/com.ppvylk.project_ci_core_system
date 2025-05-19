using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    [CreateAssetMenu(fileName = "NewDamageCasterAbilityParam", menuName = "TurnBasedTools/Ability/Parameters/ Create DamageCasterAbilityParam", order = 1)]
    public class DamageCasterParam : AbilityParam
    {
        public int m_Damage;
        public bool m_bMagicalDamage;

        public override void ApplyTo(GridUnit InCaster, ILevelCell InObject)
        {
            BattleHealth healthComp = InCaster.GetComponent<BattleHealth>();
            if (healthComp)
            {
                if (m_bMagicalDamage)
                {
                    healthComp.MagicDamage(m_Damage);
                }
                else
                {
                    healthComp.Damage(m_Damage);
                }
            }
        }

        public override string GetAbilityInfo()
        {
            return "Damage Self " + (m_bMagicalDamage ? "(Magical)" : "") + " " + m_Damage.ToString();
        }
    }
}
