using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    [CreateAssetMenu(fileName = "NewDamageAbilityParam", menuName = "ProjectCI Tools/Ability/Parameters/ Create DamageAbilityParam", order = 1)]
    public class DamageAbilityParam : AbilityParam
    {
        public int m_Damage;
        public bool m_bMagicalDamage;

        public override void ApplyTo(GridPawnUnit InCaster, GridObject InObject)
        {
            BattleHealth healthComp = InObject.GetComponent<BattleHealth>();
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
            return "Damage" + (m_bMagicalDamage ? "(Magical)" : "") + " " + m_Damage.ToString();
        }
    }
}
