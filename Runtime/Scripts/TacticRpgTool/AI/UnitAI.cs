using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI
{
    [CreateAssetMenu(fileName = "NewUnitAI", menuName = "TurnBasedTools/AI/Create UnitAIData", order = 1)]
    public class UnitAI : ScriptableObject
    {
        public bool m_bActivateOnStart;
        public int m_ActivationRange = 5;

        public virtual IEnumerator RunOnUnit(GridUnit InAIUnit)
        {
            yield return new WaitForSeconds(0.0f);

            if ( InAIUnit )
            {
                CheckActivation( InAIUnit );

                if ( InAIUnit.IsActivated() )
                {
                    //Calculate target
                    GridUnit target = CalculateTargetUnit( InAIUnit );

                    //Calculate ability
                    int abilityIndex = CalculateAbilityIndex( InAIUnit );

                    if( target )
                    {
                        // Do movement.
                        ILevelCell targetMovementCell = CalculateMoveToCell( InAIUnit, target, abilityIndex );
                        if( targetMovementCell )
                        {
                            if( targetMovementCell != InAIUnit.GetCell() )
                            {
                                UnityEvent OnMovementComplete = new UnityEvent();
                                List<ILevelCell> AllowedCells = InAIUnit.GetAllowedMovementCells();
                                yield return TacticBattleManager.Get().StartCoroutine(InAIUnit.EnumeratorTraverseTo(targetMovementCell, OnMovementComplete, AllowedCells));
                            }
                        }

                        if ( !InAIUnit.IsDead() ) //Unit can die while walking around.
                        {
                            // Do ability.
                            if (abilityIndex >= 0)
                            {
                                UnitAbility selectedAbility = InAIUnit.GetAbilities()[abilityIndex].unitAbility;
                                if (selectedAbility)
                                {
                                    List<ILevelCell> abilityCells = selectedAbility.GetAbilityCells(InAIUnit);
                                    if (abilityCells.Contains(target.GetCell()))
                                    {
                                        yield return TacticBattleManager.Get().StartCoroutine(AIManager.ExecuteAbility(InAIUnit, target.GetCell(), selectedAbility));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void CheckActivation(GridUnit InUnit)
        {
            if (!InUnit.IsActivated())
            {
                if (m_bActivateOnStart)
                {
                    InUnit.SetActivated(true);
                }
                else
                {
                    AIRadiusInfo radiusInfo = new AIRadiusInfo(InUnit.GetCell(), m_ActivationRange)
                    {
                        Caster = InUnit,
                        bAllowBlocked = true,
                        bStopAtBlockedCell = true,
                        EffectedTeam = GameTeam.Hostile
                    };

                    List<ILevelCell> ActivationCells = AIManager.GetRadius(radiusInfo);
                    foreach (var cell in ActivationCells)
                    {
                        GridObject objOnCell = cell.GetObjectOnCell();
                        if (objOnCell)
                        {
                            if (TacticBattleManager.GetTeamAffinity(InUnit.GetTeam(), objOnCell.GetTeam()) == GameTeam.Hostile)
                            {
                                InUnit.SetActivated(true);
                            }
                        }
                    }
                }
            }
        }

        protected GridUnit CalculateTargetUnit(GridUnit InAIUnit)
        {
            GameTeam SelectedTeam = GameTeam.Hostile;

            if (InAIUnit.GetTeam() == GameTeam.Hostile)
            {
                SelectedTeam = GameTeam.Friendly;
            }

            List<GridUnit> AIUnits = TacticBattleManager.GetUnitsOnTeam(SelectedTeam);

            int closestIndex = int.MaxValue;
            GridUnit selectedTarget = null;
            foreach (GridUnit currUnit in AIUnits)
            {
                if ( currUnit && !currUnit.IsDead() )
                {
                    AIPathInfo pathInfo = new AIPathInfo
                    {
                        StartCell = InAIUnit.GetCell(),
                        TargetCell = currUnit.GetCell(),
                        bIgnoreUnits = false,
                        bTakeWeightIntoAccount = true
                    };

                    List<ILevelCell> unitPath = AIManager.GetPath(pathInfo);
                    if (unitPath.Count < closestIndex)
                    {
                        closestIndex = unitPath.Count;
                        selectedTarget = currUnit;
                    }
                }
            }

            return selectedTarget;
        }

        protected int CalculateAbilityIndex(GridUnit InAIUnit)
        {
            if (InAIUnit)
            {
                int allowedAP = InAIUnit.GetCurrentAbilityPoints();
                List<UnitAbilityPlayerData> abilities = InAIUnit.GetAbilities();
                if (abilities.Count > 0)
                {
                    for (int i = 0; i < abilities.Count; i++)
                    {
                        if (abilities[i].unitAbility && abilities[i].unitAbility.GetEffectedTeam() == GameTeam.Hostile)
                        {
                            if (abilities[i].unitAbility.GetActionPointCost() <= allowedAP)
                            {
                                return i;
                            }
                        }
                    }
                }
            }

            return -1;
        }

        protected ILevelCell CalculateMoveToCell(GridUnit InAIUnit, GridUnit InTarget, int InAbilityIndex)
        {
            if (InTarget == null)
            {
                return InAIUnit.GetCell();
            }

            List<UnitAbilityPlayerData> AIUnitAbilities = InAIUnit.GetAbilities();
            if (AIUnitAbilities.Count < 0)
            {
                return InAIUnit.GetCell();
            }

            List<ILevelCell> AllowedMovementCells = InAIUnit.GetAllowedMovementCells();
            List<ILevelCell> OverlapCells = new List<ILevelCell>();

            if ( InAbilityIndex != -1 )
            {
                UnitAbility SelectedAbility = AIUnitAbilities[InAbilityIndex].unitAbility;
                List<ILevelCell> CellsAroundUnitToAttack = SelectedAbility.GetShape().GetCellList(InTarget, InTarget.GetCell(), SelectedAbility.GetRadius(), SelectedAbility.DoesAllowBlocked(), SelectedAbility.GetEffectedTeam());

                //If you can attack from where you are, do so.
                List<ILevelCell> AbilityCells = SelectedAbility.GetShape().GetCellList(InAIUnit, InAIUnit.GetCell(), SelectedAbility.GetRadius(), SelectedAbility.DoesAllowBlocked(), SelectedAbility.GetEffectedTeam());
                if (AbilityCells.Contains(InTarget.GetCell()))
                {
                    return InAIUnit.GetCell();
                }

                //Find cells that you can move to and attack.
                foreach (ILevelCell levelCell in CellsAroundUnitToAttack)
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
                ILevelCell selectedCell = null;
                foreach ( ILevelCell levelCell in OverlapCells )
                {
                    AIPathInfo pathInfo = new AIPathInfo
                    {
                        StartCell = levelCell,
                        TargetCell = InTarget.GetCell(),
                        bIgnoreUnits = false,
                        bTakeWeightIntoAccount = true
                    };

                    List<ILevelCell> levelPath = AIManager.GetPath( pathInfo );
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
                ILevelCell selectedCell = null;
                foreach (ILevelCell levelCell in AllowedMovementCells)
                {
                    AIPathInfo pathInfo = new AIPathInfo
                    {
                        StartCell = levelCell,
                        TargetCell = InTarget.GetCell(),
                        bIgnoreUnits = false,
                        bTakeWeightIntoAccount = true
                    };

                    int cellDistance = AIManager.GetPath(pathInfo).Count - 1;
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
