using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    [CreateAssetMenu(fileName = "NewAbilityShape", menuName = "ProjectCI Tools/Ability/Shapes/Create DirectionalAbilityShape", order = 1)]
    public class DirectionalAbilityShape : AbilityShape
    {
        [SerializeField]
        bool m_bOnlyMyEnemies;

        public override List<LevelCellBase> GetCellList(GridPawnUnit InCaster, LevelCellBase InCell, int InRange, bool bAllowBlocked, BattleTeam m_EffectedTeam)
        {
            List<LevelCellBase> cells = new List<LevelCellBase>();

            if(InCell && InRange > 0)
            {
                cells.AddRange(GetCellsInDirection(InCell, InRange, CompassDir.N, bAllowBlocked, m_EffectedTeam));
                cells.AddRange(GetCellsInDirection(InCell, InRange, CompassDir.E, bAllowBlocked, m_EffectedTeam));
                cells.AddRange(GetCellsInDirection(InCell, InRange, CompassDir.NE, bAllowBlocked, m_EffectedTeam));
                cells.AddRange(GetCellsInDirection(InCell, InRange, CompassDir.NW, bAllowBlocked, m_EffectedTeam));
                cells.AddRange(GetCellsInDirection(InCell, InRange, CompassDir.SE, bAllowBlocked, m_EffectedTeam));
                cells.AddRange(GetCellsInDirection(InCell, InRange, CompassDir.S, bAllowBlocked, m_EffectedTeam));
                cells.AddRange(GetCellsInDirection(InCell, InRange, CompassDir.SW, bAllowBlocked, m_EffectedTeam));
                cells.AddRange(GetCellsInDirection(InCell, InRange, CompassDir.W, bAllowBlocked, m_EffectedTeam));
            }

            if ( m_bOnlyMyEnemies )
            {
                List<LevelCellBase> enemyCells = new List<LevelCellBase>();
                foreach ( var currCell in cells )
                {
                    GridPawnUnit unitOnCell = currCell.GetUnitOnCell();
                    if ( unitOnCell )
                    {
                        BattleTeam AffinityToCaster = TacticBattleManager.GetTeamAffinity( InCaster.GetTeam(), unitOnCell.GetTeam() );
                        if ( AffinityToCaster == BattleTeam.Hostile )
                        {
                            enemyCells.Add( currCell );
                        }
                    }
                }

                return enemyCells;
            }
            else
            {
                return cells;
            }
        }

        List<LevelCellBase> GetCellsInDirection(LevelCellBase StartCell, int InRange, CompassDir Dir, bool bAllowBlocked, BattleTeam m_EffectedTeam)
        {
            List<LevelCellBase> cells = new List<LevelCellBase>();

            if(InRange > 0)
            {
                LevelCellBase CurserCell = StartCell.GetAdjacentCell(Dir);

                int Count = 0;
                while (CurserCell)
                {
                    if(CurserCell.IsBlocked() && !bAllowBlocked)
                    {
                        break;
                    }

                    GridObject gridObj = CurserCell.GetObjectOnCell();
                    if (gridObj)
                    {
                        if (m_EffectedTeam == BattleTeam.None)
                        {
                            break;
                        }

                        BattleTeam ObjAffinity = TacticBattleManager.GetTeamAffinity(gridObj.GetTeam(), StartCell.GetCellTeam());
                        if (ObjAffinity == BattleTeam.Friendly && m_EffectedTeam == BattleTeam.Hostile)
                        {
                            break;
                        }
                    }

                    cells.Add(CurserCell);
                    CurserCell = CurserCell.GetAdjacentCell(Dir);
                    Count++;
                    if(InRange != -1 && Count >= InRange)
                    {
                        break;
                    }
                }
            }

            return cells;
        }
    }
}
