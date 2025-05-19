using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    [CreateAssetMenu(fileName = "NewAbilityShape", menuName = "TurnBasedTools/Ability/Shapes/Create RadiusAbilityShape", order = 1)]
    public class RadiusAbilityShape : AbilityShape
    {
        [SerializeField]
        bool m_bStopAtBlocked = true;
        
        [SerializeField]
        bool m_bOnlyMyEnemies;

        public override List<ILevelCell> GetCellList(GridUnit InCaster, ILevelCell InCell, int InRange, bool bAllowBlocked, GameTeam m_EffectedTeam)
        {
            GridUnit Caster = InCell.GetUnitOnCell();

            AIRadiusInfo radiusInfo = new AIRadiusInfo(InCell, InRange);
            radiusInfo.Caster = Caster;
            radiusInfo.bAllowBlocked = bAllowBlocked;
            radiusInfo.bStopAtBlockedCell = m_bStopAtBlocked;
            radiusInfo.EffectedTeam = m_EffectedTeam;

            List<ILevelCell> radCells = AIManager.GetRadius(radiusInfo);

            if ( m_bOnlyMyEnemies )
            {
                List<ILevelCell> enemyCells = new List<ILevelCell>();
                foreach ( var currCell in radCells )
                {
                    GridUnit unitOnCell = currCell.GetUnitOnCell();
                    if ( unitOnCell )
                    {
                        GameTeam AffinityToCaster = GameManager.GetTeamAffinity( InCaster.GetTeam(), unitOnCell.GetTeam() );
                        if ( AffinityToCaster == GameTeam.Hostile )
                        {
                            enemyCells.Add( currCell );
                        }
                    }
                }

                return enemyCells;
            }
            else
            {
                return AIManager.GetRadius(radiusInfo);
            }
        }
    }
}
