using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    [CreateAssetMenu(fileName = "NewAbilityShape", menuName = "ProjectCI Tools/Ability/Shapes/Create AllTeammatesAbilityShape", order = 1)]
    public class AllTeammatesAbilityShape : AbilityShape
    {
        public override List<LevelCellBase> GetCellList(GridPawnUnit InCaster, LevelCellBase InCell, int InRange, bool bAllowBlocked, BattleTeam m_EffectedTeam)
        {
            List<GridPawnUnit> TeamUnits = TacticBattleManager.GetUnitsOnTeam( InCaster.GetTeam() );

            List<LevelCellBase> cells = new List<LevelCellBase>();
            foreach ( var unit in TeamUnits )
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
