using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    [CreateAssetMenu(fileName = "NewAbilityShape", menuName = "TurnBasedTools/Ability/Shapes/Create AllTeammatesAbilityShape", order = 1)]
    public class AllTeammatesAbilityShape : AbilityShape
    {
        public override List<ILevelCell> GetCellList(GridUnit InCaster, ILevelCell InCell, int InRange, bool bAllowBlocked, GameTeam m_EffectedTeam)
        {
            List<GridUnit> TeamUnits = TacticBattleManager.GetUnitsOnTeam( InCaster.GetTeam() );

            List<ILevelCell> cells = new List<ILevelCell>();
            foreach ( var unit in TeamUnits )
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
