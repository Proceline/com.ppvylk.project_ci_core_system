using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay
{
    public partial class TacticBattleManager
    {
        #region EventStuff

        private void RegisterControlActions()
        {
            m_InputActionManager.BindConfirmAction(HandleConfirmAction);
            m_InputActionManager.BindCancelAction(HandleCancelAction);
            if (m_GameRules)
            {
                m_InputActionManager.BindCancelAction(m_GameRules.CancelActionExtension);
            }
        }

        private void UnregisterControlActions()
        {
            m_InputActionManager.UnbindConfirmAction(HandleConfirmAction);
            m_InputActionManager.UnbindCancelAction(HandleCancelAction);
            if (m_GameRules)
            {
                m_InputActionManager.UnbindCancelAction(m_GameRules.CancelActionExtension);
            }
        }

        private void HandleConfirmAction(InputAction.CallbackContext context)
        {
            if (m_CurrentHoverCell)
            {
                GridObject ObjOnCell = m_CurrentHoverCell.GetObjectOnCell();
                if (ObjOnCell)
                {
                    ObjOnCell.HandleBeingConfirmed();
                }
            }

            HandleCellClicked(m_CurrentHoverCell);
        }

        private void HandleCancelAction(InputAction.CallbackContext context)
        {
            if (m_CurrentHoverCell)
            {
                GridObject ObjOnCell = m_CurrentHoverCell.GetObjectOnCell();
                if (ObjOnCell)
                {
                    ObjOnCell.HandleBeingCanceled();
                }
            }
        }

        private void BeginHover(LevelCellBase InCell)
        {
            m_CurrentHoverCell = InCell;
            UpdateHoverCells();
        }

        public void UpdateHoverCells()
        {
            CleanupHoverCells();

            if (m_CurrentHoverCell)
            {
                GridPawnUnit hoverGrid = m_CurrentHoverCell.GetUnitOnCell();
                if (hoverGrid)
                {
                    OnUnitHover.Invoke(hoverGrid);
                }

                CurrentHoverCells.Add(m_CurrentHoverCell);

                BattleGameRules gameRules = GetRules();
                if (gameRules)
                {
                    GridPawnUnit selectedUnit = gameRules.GetSelectedUnit();
                    if (selectedUnit)
                    {
                        UnitBattleState unitState = selectedUnit.GetCurrentState();
                        switch (unitState)
                        {
                            case UnitBattleState.UsingAbility:
                            case UnitBattleState.AbilityTargeting:
                                CurrentHoverCells.AddRange(gameRules.GetAbilityHoverCells(m_CurrentHoverCell));
                                break;
                            case UnitBattleState.Moving:
                                List<LevelCellBase> allowedMovementCells = selectedUnit.GetAllowedMovementCells();

                                if (allowedMovementCells.Contains(m_CurrentHoverCell))
                                {
                                    List<LevelCellBase> pathToCursor =
                                        selectedUnit.GetPathTo(m_CurrentHoverCell, allowedMovementCells);

                                    foreach (LevelCellBase pathCell in pathToCursor)
                                    {
                                        if (pathCell)
                                        {
                                            if (allowedMovementCells.Contains(pathCell))
                                            {
                                                CurrentHoverCells.Add(pathCell);
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                foreach (LevelCellBase currCell in CurrentHoverCells)
                {
                    currCell.SetMaterial(CellState.eHover);
                }

                m_CurrentHoverCell.HandleMouseOver();
            }
        }

        void EndHover(LevelCellBase InCell)
        {
            CleanupHoverCells();

            if (InCell)
            {
                InCell.HandleMouseExit();
            }

            m_CurrentHoverCell = null;

            OnUnitHover.Invoke(null);
        }

        void CleanupHoverCells()
        {
            foreach (LevelCellBase currCell in CurrentHoverCells)
            {
                if (currCell)
                {
                    currCell.SetMaterial(currCell.GetCellState());
                }
            }

            CurrentHoverCells.Clear();
        }

        void HandleCellClicked(LevelCellBase InCell)
        {
            if (!InCell)
            {
                return;
            }

            if (!m_GameRules)
            {
                return;
            }

            GridPawnUnit gridUnit = InCell.GetUnitOnCell();
            if (gridUnit)
            {
                BattleTeam CurrentTurnTeam = m_GameRules.GetCurrentTeam();
                BattleTeam UnitsTeam = gridUnit.GetTeam();

                if (UnitsTeam == CurrentTurnTeam)
                {
                    m_GameRules.HandlePlayerSelected(gridUnit);
                }
                else
                {
                    if (UnitsTeam == BattleTeam.Hostile)
                    {
                        m_GameRules.HandleEnemySelected(gridUnit);
                    }
                }
            }
            m_GameRules.HandleCellSelected(InCell);
        }

        void HandleGameComplete()
        {
            m_CurrentHoverCell = null;
            CleanupHoverCells();
            m_bIsPlaying = false;
        }

        private void HandleInteractionFocused(LevelCellBase InCell, CellInteractionState InInteractionState)
        {
            GridObject ObjOnCell = InCell.GetObjectOnCell();

            switch (InInteractionState)
            {
                case CellInteractionState.eBeginFocused:
                    BeginHover(InCell);
                    if (ObjOnCell)
                    {
                        ObjOnCell.HandleBegingFocused();
                    }
                    break;
                case CellInteractionState.eEndFocused:
                    EndHover(InCell);
                    if (ObjOnCell)
                    {
                        ObjOnCell.HandleEndFocused();
                    }
                    break;
            }
        }

        #endregion
    }
} 