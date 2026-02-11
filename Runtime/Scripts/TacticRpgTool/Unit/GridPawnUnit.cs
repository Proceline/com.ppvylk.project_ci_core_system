using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status;
using System;
using ProjectCI.CoreSystem.Runtime.Attributes;
using ProjectCI.CoreSystem.Runtime.States.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit
{
    public enum UnitBattleState
    {
        Idle,
        Moving,
        MovingProgress,
        UsingAbility,
        AbilityTargeting,
        AbilityConfirming,
        Finished
    }

    public abstract class GridPawnUnit : GridObject, IStateOwner<UnitBattleState>
    {
        private SoUnitData _unitData;
        private UnitAttributeContainer _runtimeAttributes;
        
        private bool _bIsMoving = false;

        protected readonly UnityEvent OnMovementPostComplete = new();

        public event Action OnPreStandIdleAnimRequired;
        public event Action OnPreMovementAnimRequired;

        protected SoUnitData UnitData => _unitData;

        public UnitAttributeContainer RuntimeAttributes 
        { 
            get => _runtimeAttributes; 
            protected set => _runtimeAttributes = value; 
        }

        public virtual void PostInitialize()
        {
            GetCell().HandleVisibilityChanged();
        }

        public abstract void LookAtCell(LevelCellBase InCell);

        #region Events

        public void BindToOnMovementPostCompleted(UnityAction InAction)
        {
            OnMovementPostComplete.AddListener(InAction);
        }

        public void UnBindFromOnMovementPostCompleted(UnityAction InAction)
        {
            OnMovementPostComplete.RemoveListener(InAction);
        }

        #endregion

        #region Setters

        public virtual void SetUnitData(SoUnitData InUnitData)
        {
            _unitData = InUnitData;
        }

        public abstract void SetCurrentActionPoints(int actionPoint);
        
        public abstract void SetCurrentMovementPoints(int movePoint);

        #endregion

        #region Getters

        public SoUnitData GetUnitData()
        {
            return _unitData;
        }

        public abstract IStatusEffectContainer GetStatusEffectContainer();

        public abstract UnitBattleState GetCurrentState();

        public abstract void AddState(UnitBattleState state);

        public abstract void RemoveLastState();

        public abstract void ClearStates();

        public bool IsMoving()
        {
            return _bIsMoving;
        }

        public bool IsFlying()
        {
            return GetUnitData().m_bIsFlying;
        }

        public bool ShouldLookAtTargets()
        {
            return GetUnitData().m_bLookAtTargets;
        }

        public virtual bool IsDead() => false;

        public abstract int GetCurrentMovementPoints();

        public abstract int GetCurrentActionPoints();

        public Vector3 GetCellAlignPos(LevelCellBase inCell)
        {
            if(inCell)
            {
                GridObject obj = inCell.GetObjectOnCell();
                if (obj)
                {
                    return inCell.GetAlignPos(obj);
                }
                else
                {
                    var alignPos = inCell.gameObject.transform.position;
                    alignPos.y = gameObject.transform.position.y;
                    return alignPos;
                }
            }

            return Vector3.zero;
        }
        
        #endregion

        #region MovementStuff

        public abstract List<LevelCellBase> GetAllowedMovementCells();

        public bool ExecuteMovement(LevelCellBase targetCell, Action<List<LevelCellBase>> onPathCalculated,
            Action onMovementCompleted)
        {
            if (IsMoving())
            {
                return false;
            }

            if (!_unitData.m_MovementShape || !targetCell)
            {
                return false;
            }

            if (!targetCell.IsVisible())
            {
                return false;
            }

            List<LevelCellBase> allowedMovementCells = GetAllowedMovementCells();
            if (!allowedMovementCells.Contains(targetCell))
            {
                return false;
            }

            TraverseTo(targetCell, onPathCalculated, onMovementCompleted, allowedMovementCells);
            return true;
        }

        private async void TraverseTo(LevelCellBase targetCell, Action<List<LevelCellBase>> onPathCalculated,
            Action onMovementCompleted, List<LevelCellBase> allowedCells)
        {
            await OnGridTraverseTo(targetCell, onPathCalculated, onMovementCompleted, allowedCells);
            OnMovementPostComplete?.Invoke();
        }

        public virtual async void ForceMoveTo(LevelCellBase targetCell)
        {
            if (!targetCell)
            {
                return;
            }

            var startingCell = GetCell();
            var startPosition = startingCell.GetAlignPos(this);
            SetCurrentCell(targetCell);

            float time = 0;
            var endPosition = targetCell.GetAlignPos(this);
            while (time < 1f)
            {
                await Awaitable.NextFrameAsync();
                time += Time.deltaTime * AStarAlgorithmUtils.GetMovementSpeed() * 2f;
                gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            }

            gameObject.transform.position = endPosition;
        }

        protected internal async Awaitable OnGridTraverseTo(LevelCellBase inTargetCell,
            Action<List<LevelCellBase>> onPathCalculated, Action onMovementCompleted,
            List<LevelCellBase> inAllowedCells)
        {
            if (inTargetCell)
            {
                _bIsMoving = true;

                OnPreMovementAnimRequired?.Invoke();

                List<LevelCellBase> cellPath = GetPathTo(inTargetCell, inAllowedCells);
                onPathCalculated?.Invoke(cellPath);
                Vector3 startPos = GetCell().GetAlignPos(this);

                int movementCount = 0;

                LevelCellBase finalCell = inTargetCell;

                foreach (LevelCellBase cell in cellPath)
                {
                    float timeSpent = 0;
                    Vector3 endPos = cell.GetAlignPos(this);

                    LookAtCell(cell);

                    while (timeSpent < 1f && startPos != endPos)
                    {
                        await Awaitable.NextFrameAsync();
                        timeSpent += Time.deltaTime * AStarAlgorithmUtils.GetMovementSpeed();
                        gameObject.transform.position = Vector3.Lerp(startPos, endPos, timeSpent);
                    }

                    gameObject.transform.position = endPos;
                    startPos = cell.GetAlignPos(this);

                    await Awaitable.WaitForSecondsAsync(AStarAlgorithmUtils.GetWaitTime());

                    if (IsDead())
                    {
                        break;
                    }
                }

                if (!IsDead())
                {
                    SetCurrentCell(finalCell);
                    OnPreStandIdleAnimRequired?.Invoke();
                }

                _bIsMoving = false;

                onMovementCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Used for Pathfinding Moving on Grid, no Teleport
        /// </summary>
        /// <param name="targetCell"></param>
        /// <param name="allowedCells"></param>
        /// <returns></returns>
        public List<LevelCellBase> GetPathTo(LevelCellBase targetCell, List<LevelCellBase> allowedCells)
        {
            var pathInfo = new AIPathInfo
            {
                StartCell = GetCell(),
                TargetCell = targetCell,
                bNoDestinationUnits = true,
                bIgnoreUnitsOnPath = true,
                bTakeWeightIntoAccount = TacticBattleManager.IsTeamAI(GetTeam()),
                AllowedCells = allowedCells,
                bAllowBlocked = _unitData.m_bIsFlying
            };

            List<LevelCellBase> cellPath = AStarAlgorithmUtils.GetPath(pathInfo);

            return cellPath;
        }

        #endregion

        public abstract void HandleTurnStarted();

    }
}
