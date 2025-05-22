using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;


namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    [CreateAssetMenu(fileName = "NewHealAbilityParam", menuName = "ProjectCI Tools/Ability/Parameters/ Create HealAbilityParam", order = 1)]
    public class HealParam : AbilityParam
    {
        public int m_HealAmount;

        public override void ApplyTo(GridPawnUnit InCaster, GridObject InObject)
        {
            if(InObject)
            {
                BattleHealth health = InObject.GetComponent<BattleHealth>();
                if(health)
                {
                    health.Heal(m_HealAmount);
                }
            }
        }

        public override string GetAbilityInfo()
        {
            return "Heal Target by: " + m_HealAmount;
        }
    }
}
