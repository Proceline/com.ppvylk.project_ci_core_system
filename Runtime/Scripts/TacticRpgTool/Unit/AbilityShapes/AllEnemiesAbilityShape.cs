using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    [CreateAssetMenu(fileName = "NewAbilityShape", menuName = "ProjectCI Tools/Ability/Shapes/Create AllEnemiesAbilityShape", order = 1)]
    public class AllEnemiesAbilityShape : AbilityShape
    {
        public override List<LevelCellBase> GetCellList(GridPawnUnit InCaster, LevelCellBase InCell, int InRange, bool bAllowBlocked, BattleTeam m_EffectedTeam)
        {
            BattleTeam OtherTeam = TacticBattleManager.GetTeamAffinity( InCaster.GetTeam(), BattleTeam.Hostile );

            List<GridPawnUnit> EnemyUnits = TacticBattleManager.GetUnitsOnTeam( OtherTeam );

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
