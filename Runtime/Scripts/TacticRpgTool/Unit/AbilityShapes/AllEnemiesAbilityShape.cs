using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    [CreateAssetMenu(fileName = "NewAbilityShape", menuName = "TurnBasedTools/Ability/Shapes/Create AllEnemiesAbilityShape", order = 1)]
    public class AllEnemiesAbilityShape : AbilityShape
    {
        public override List<ILevelCell> GetCellList(GridUnit InCaster, ILevelCell InCell, int InRange, bool bAllowBlocked, GameTeam m_EffectedTeam)
        {
            GameTeam OtherTeam = GameManager.GetTeamAffinity( InCaster.GetTeam(), GameTeam.Hostile );

            List<GridUnit> EnemyUnits = GameManager.GetUnitsOnTeam( OtherTeam );

            List<ILevelCell> cells = new List<ILevelCell>();
            foreach ( var unit in EnemyUnits )
            {
                if ( unit )
                {
                    ILevelCell unitCell = unit.GetCell();
                    if ( unitCell )
                    {
                        cells.Add( unitCell );
                    }
                }
            }

            return cells;
        }
    }
}
