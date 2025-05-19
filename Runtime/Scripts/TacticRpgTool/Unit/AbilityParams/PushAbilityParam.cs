using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    [CreateAssetMenu(fileName = "NewPushParam", menuName = "ProjectCI Tools/Ability/Parameters/ Create PushAbilityParam", order = 1)]
    public class PushAbilityParam : AbilityParam
    {
        public int m_Distance;

        public override void ApplyTo(GridUnit InCaster, LevelCellBase InCell)
        {
            GridUnit TargetUnit = InCell.GetUnitOnCell();

            if (TargetUnit && InCaster)
            {
                CompassDir PushDirection = InCaster.GetCell().GetDirectionToAdjacentCell( InCell );

                LevelCellBase targetCell = TargetUnit.GetCell();
                for (int i = 0; i < m_Distance; i++)
                {
                    LevelCellBase dirCell = targetCell.GetAdjacentCell(PushDirection);
                    if(dirCell && dirCell.IsCellAccesible())
                    {
                        targetCell = dirCell;
                    }
                }

                TargetUnit.MoveTo(targetCell);
            }
        }

        public override string GetAbilityInfo()
        {
            return "Push Back: " + m_Distance.ToString() + " Space" + ((m_Distance > 1) ? "s" : "");
        }
    }
}
