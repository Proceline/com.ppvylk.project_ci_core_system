using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI
{
    [CreateAssetMenu(fileName = "NewUnitAI", menuName = "ProjectCI Tools/AI/Create UnitAIData", order = 1)]
    public class UnitAI : ScriptableObject
    {
        public bool m_bActivateOnStart;
        public int m_ActivationRange = 5;

        public virtual async Awaitable RunOnUnit(GridPawnUnit InAIUnit)
        {
            // TODO: Check if this is needed.
            // await Awaitable.WaitForSecondsAsync(0.0f);
            await Awaitable.EndOfFrameAsync();

            if ( InAIUnit )
            {
                // CheckActivation( InAIUnit );

                if (InAIUnit)//InAIUnit.IsActivated() )
                {
                    //Calculate target
                    GridPawnUnit target = CalculateTargetUnit( InAIUnit );

                    //Calculate ability
                    int abilityIndex = CalculateAbilityIndex( InAIUnit );

                    if( target )
                    {
                        // Do movement.
                        LevelCellBase targetMovementCell = CalculateMoveToCell( InAIUnit, target, abilityIndex );
                        if( targetMovementCell )
                        {
                            if( targetMovementCell != InAIUnit.GetCell() )
                            {
                                UnityEvent OnMovementComplete = new UnityEvent();
                                List<LevelCellBase> AllowedCells = InAIUnit.GetAllowedMovementCells();
                                await InAIUnit.OnGridTraverseTo(targetMovementCell, OnMovementComplete, AllowedCells);
                            }
                        }

                        if (!InAIUnit.IsDead() ) //Unit can die while walking around.
                        {
                            // TODO: FIX THIS
                            // Do ability.
                            if (abilityIndex >= 0)
                            {
                                // UnitAbilityCore selectedAbility = InAIUnit.GetAbilities()[abilityIndex];
                                // if (selectedAbility)
                                // {
                                //     List<LevelCellBase> abilityCells = selectedAbility.GetAbilityCells(InAIUnit);
                                //     if (abilityCells.Contains(target.GetCell()))
                                //     {
                                //         // TODO: Remove this
                                //         // TacticBattleManager.Get().StartCoroutine(AStarAlgorithmUtils.ExecuteAbility(InAIUnit, target.GetCell(), selectedAbility));
                                //     }
                                // }
                            }
                        }
                    }
                }
            }
        }

        // protected void CheckActivation(GridPawnUnit InUnit)
        // {
        //     if (!InUnit.IsActivated())
        //     {
        //         if (m_bActivateOnStart)
        //         {
        //             // InUnit.SetActivated(true);
        //         }
        //         else
        //         {
        //             AIRadiusInfo radiusInfo = new AIRadiusInfo(InUnit.GetCell(), m_ActivationRange)
        //             {
        //                 Caster = InUnit,
        //                 bAllowBlocked = true,
        //                 bStopAtBlockedCell = true,
        //                 EffectedTeam = BattleTeam.Hostile
        //             };
        //
        //             List<LevelCellBase> ActivationCells = AStarAlgorithmUtils.GetRadius(radiusInfo);
        //             foreach (var cell in ActivationCells)
        //             {
        //                 GridObject objOnCell = cell.GetObjectOnCell();
        //                 if (objOnCell)
        //                 {
        //                     if (TacticBattleManager.GetTeamAffinity(InUnit.GetTeam(), objOnCell.GetTeam()) == BattleTeam.Hostile)
        //                     {
        //                         // InUnit.SetActivated(true);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }

        protected GridPawnUnit CalculateTargetUnit(GridPawnUnit InAIUnit)
        {
            BattleTeam SelectedTeam = BattleTeam.Hostile;

            if (InAIUnit.GetTeam() == BattleTeam.Hostile)
            {
                SelectedTeam = BattleTeam.Friendly;
            }

            List<GridPawnUnit> AIUnits = TacticBattleManager.GetUnitsOnTeam(SelectedTeam);

            int closestIndex = int.MaxValue;
            GridPawnUnit selectedTarget = null;
            foreach (GridPawnUnit currUnit in AIUnits)
            {
                if ( currUnit && !currUnit.IsDead() )
                {
                    AIPathInfo pathInfo = new AIPathInfo
                    {
                        StartCell = InAIUnit.GetCell(),
                        TargetCell = currUnit.GetCell(),
                        bNoDestinationUnits = false,
                        bIgnoreUnitsOnPath = true,
                        bTakeWeightIntoAccount = true
                    };

                    List<LevelCellBase> unitPath = AStarAlgorithmUtils.GetPath(pathInfo);
                    if (unitPath.Count < closestIndex)
                    {
                        closestIndex = unitPath.Count;
                        selectedTarget = currUnit;
                    }
                }
            }

            return selectedTarget;
        }

        protected int CalculateAbilityIndex(GridPawnUnit InAIUnit)
        {
            // TODO: FIX THIS
            if (InAIUnit)
            {
                // int allowedAP = InAIUnit.GetCurrentAbilityPoints();
                // List<UnitAbilityCore> abilities = InAIUnit.GetAbilities();
                // if (abilities.Count > 0)
                // {
                //     for (int i = 0; i < abilities.Count; i++)
                //     {
                //         if (abilities[i] && abilities[i].GetEffectedTeam() == BattleTeam.Hostile)
                //         {
                //             if (abilities[i].GetActionPointCost() <= allowedAP)
                //             {
                //                 return i;
                //             }
                //         }
                //     }
                // }
                return 0;
            }

            return -1;
        }

        protected LevelCellBase CalculateMoveToCell(GridPawnUnit InAIUnit, GridPawnUnit InTarget, int InAbilityIndex)
        {
            if (InTarget == null)
            {
                return InAIUnit.GetCell();
            }

            // TODO: Solve abilities problems
            List<UnitAbilityCore> AIUnitAbilities = new();//InAIUnit.GetAbilities();
            if (AIUnitAbilities.Count < 0)
            {
                return InAIUnit.GetCell();
            }

            List<LevelCellBase> AllowedMovementCells = InAIUnit.GetAllowedMovementCells();
            List<LevelCellBase> OverlapCells = new List<LevelCellBase>();

            if ( InAbilityIndex != -1 )
            {
                UnitAbilityCore SelectedAbility = AIUnitAbilities[InAbilityIndex];
                List<LevelCellBase> CellsAroundUnitToAttack = SelectedAbility.GetShape().GetCellList(InTarget, InTarget.GetCell(), SelectedAbility.GetRadius(), SelectedAbility.DoesAllowBlocked(), SelectedAbility.GetEffectedTeam());

                //If you can attack from where you are, do so.
                List<LevelCellBase> AbilityCells = SelectedAbility.GetShape().GetCellList(InAIUnit, InAIUnit.GetCell(), SelectedAbility.GetRadius(), SelectedAbility.DoesAllowBlocked(), SelectedAbility.GetEffectedTeam());
                if (AbilityCells.Contains(InTarget.GetCell()))
                {
                    return InAIUnit.GetCell();
                }

                //Find cells that you can move to and attack.
                foreach (LevelCellBase levelCell in CellsAroundUnitToAttack)
                {
                    if (AllowedMovementCells.Contains(levelCell))
                    {
                        OverlapCells.Add(levelCell);
                    }
                }
            }

            bool bAbleToAttack = OverlapCells.Count > 0;
            if ( bAbleToAttack )
            {
                //Cells exist that allow movement, and attack.
                int currDistance = -1;
                LevelCellBase selectedCell = null;
                foreach ( LevelCellBase levelCell in OverlapCells )
                {
                    AIPathInfo pathInfo = new AIPathInfo
                    {
                        StartCell = levelCell,
                        TargetCell = InTarget.GetCell(),
                        bNoDestinationUnits = false,
                        bIgnoreUnitsOnPath = true,
                        bTakeWeightIntoAccount = true
                    };

                    List<LevelCellBase> levelPath = AStarAlgorithmUtils.GetPath( pathInfo );
                    int cellDistance = levelPath.Count - 1;
                    if ( cellDistance > currDistance )
                    {
                        selectedCell = levelCell;
                        currDistance = cellDistance;
                    }
                }

                return selectedCell;
            }
            else// Move towards target
            {
                int currDistance = int.MaxValue;
                LevelCellBase selectedCell = null;
                foreach (LevelCellBase levelCell in AllowedMovementCells)
                {
                    AIPathInfo pathInfo = new AIPathInfo
                    {
                        StartCell = levelCell,
                        TargetCell = InTarget.GetCell(),
                        bNoDestinationUnits = false,
                        bIgnoreUnitsOnPath = true,
                        bTakeWeightIntoAccount = true
                    };

                    int cellDistance = AStarAlgorithmUtils.GetPath(pathInfo).Count - 1;
                    if (cellDistance < currDistance)
                    {
                        selectedCell = levelCell;
                        currDistance = cellDistance;
                    }
                }

                return selectedCell;
            }
        }
    }
}
