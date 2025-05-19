using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    [CreateAssetMenu(fileName = "NewAbilityShape", menuName = "ProjectCI Tools/Ability/Shapes/Create AllEnemiesAbilityShape", order = 1)]
    public class AllEnemiesAbilityShape : AbilityShape
    {
        public override List<LevelCellBase> GetCellList(GridUnit InCaster, LevelCellBase InCell, int InRange, bool bAllowBlocked, GameTeam m_EffectedTeam)
        {
            GameTeam OtherTeam = TacticBattleManager.GetTeamAffinity( InCaster.GetTeam(), GameTeam.Hostile );

            List<GridUnit> EnemyUnits = TacticBattleManager.GetUnitsOnTeam( OtherTeam );

            List<LevelCellBase> cells = new List<LevelCellBase>();
            foreach ( var unit in EnemyUnits )
            {
                if ( unit )
                {
                    LevelCellBase unitCell = unit.GetCell();
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
