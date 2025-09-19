using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio;
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
        private UnitAttributeContainer _simulatedAttributes;

        protected int CurrentMovementPoints;
        
        private bool _bIsMoving = false;

        protected UnityEvent OnMovementPostComplete = new();

        public event Action OnPreStandIdleAnimRequired;
        public event Action OnPreMovementAnimRequired;

        protected SoUnitData UnitData => _unitData;

        public UnitAttributeContainer RuntimeAttributes 
        { 
            get => _runtimeAttributes; 
            protected set => _runtimeAttributes = value; 
        }

        public UnitAttributeContainer SimulatedAttributes 
        { 
            get => _simulatedAttributes; 
            protected set => _simulatedAttributes = value; 
        }

        public virtual void PostInitialize()
        {
            GetCell().HandleVisibilityChanged();
        }

        public abstract void LookAtCell(LevelCellBase InCell);
        
        public void AddAI(UnitAI InAIData)
        {
            UnitAIComponent AIComponent = gameObject.GetComponent<UnitAIComponent>();
            if( !AIComponent )
            {
                AIComponent = gameObject.AddComponent<UnitAIComponent>();
            }

            AIComponent.SetAIData( InAIData );
        }
        
        public void CheckCellVisibility(LevelCellBase InCell)
        {
            if(InCell)
            {
                bool bDead = IsDead();
                bool bCellIsVisible = InCell.IsVisible();

                SetVisible(bCellIsVisible && !bDead);
            }
        }

        public void CheckCellVisibility()
        {
            CheckCellVisibility(GetCell());
        }

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

        #endregion

        #region Getters

        public SoUnitData GetUnitData()
        {
            return _unitData;
        }

        public StatusEffectContainer GetAilmentContainer()
        {
            StatusEffectContainer statusEffectHandler = GetComponent<StatusEffectContainer>();
            if (!statusEffectHandler)
            {
                statusEffectHandler = gameObject.AddComponent<StatusEffectContainer>();
            }

            return statusEffectHandler;
        }

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

        public virtual int GetCurrentMovementPoints()
        {
            return CurrentMovementPoints;
        }

        public Vector3 GetCellAllignPos(LevelCellBase inCell)
        {
            if(inCell)
            {
                GridObject obj = inCell.GetObjectOnCell();
                if (obj)
                {
                    return inCell.GetAllignPos(obj);
                }
                else
                {
                    Vector3 AllignPos = inCell.gameObject.transform.position;
                    AllignPos.y = gameObject.transform.position.y;
                    return AllignPos;
                }
            }

            return Vector3.zero;
        }
        
        #endregion

        #region MovementStuff

        public virtual List<LevelCellBase> GetAllowedMovementCells()
        {
            return _unitData.m_MovementShape.GetCellList(this, GetCell(), CurrentMovementPoints, _unitData.m_bIsFlying);
        }

        public bool ExecuteMovement(LevelCellBase targetCell)
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

            TraverseTo(targetCell, OnMovementPostComplete, allowedMovementCells);
            return true;
        }

        private async void TraverseTo(LevelCellBase InTargetCell, 
            UnityEvent onMovementComplete, List<LevelCellBase> InAllowedCells)
        {
            await OnGridTraverseTo(InTargetCell, onMovementComplete, InAllowedCells);
        }

        public virtual async void ForceMoveTo(LevelCellBase targetCell)
        {
            await ForceMoveToInternally(targetCell);
        }

        protected internal async Awaitable OnGridTraverseTo(LevelCellBase InTargetCell, UnityEvent onMovementComplete, List<LevelCellBase> InAllowedCells)
        {
            if (InTargetCell)
            {
                _bIsMoving = true;

                OnPreMovementAnimRequired?.Invoke();

                List<LevelCellBase> cellPath = GetPathTo(InTargetCell, InAllowedCells);
                Vector3 StartPos = GetCell().GetAllignPos(this);

                int movementCount = 0;

                LevelCellBase finalCell = InTargetCell;

                LevelCellBase startingCell = GetCell();

                foreach (LevelCellBase cell in cellPath)
                {
                    FogOfWar fogOfWar = TacticBattleManager.GetFogOfWar();
                    if (fogOfWar)
                    {
                        if (GetTeam() == BattleTeam.Friendly)
                        {
                            fogOfWar.CheckPoint(cell);
                        }
                        else
                        {
                            CheckCellVisibility(cell);
                        }
                    }

                    float timeSpent = 0;
                    Vector3 endPos = cell.GetAllignPos(this);

                    LookAtCell(cell);

                    while (timeSpent < 1f && StartPos != endPos)
                    {
                        await Awaitable.NextFrameAsync();
                        timeSpent += Time.deltaTime * AStarAlgorithmUtils.GetMovementSpeed();
                        gameObject.transform.position = Vector3.Lerp(StartPos, endPos, timeSpent);
                    }

                    gameObject.transform.position = endPos;
                    StartPos = cell.GetAllignPos(this);

                    await Awaitable.WaitForSecondsAsync(AStarAlgorithmUtils.GetWaitTime());

                    if ( cell != startingCell )
                    {
                        StatusEffectUtils.HandleUnitOnCell(this, cell);
                    }

                    if (IsDead())
                    {
                        break;
                    }

                    if(movementCount++ >= CurrentMovementPoints)
                    {
                        finalCell = cell;
                        break;
                    }
                }

                if ( !IsDead() )
                {
                    SetCurrentCell(finalCell);
                    RemoveMovementPoints(cellPath.Count - 1);

                    OnPreStandIdleAnimRequired?.Invoke();
                }

                _bIsMoving = false;

                if (onMovementComplete != null)
                {
                    onMovementComplete.Invoke();
                }
            }
        }

        protected async Awaitable ForceMoveToInternally(LevelCellBase targetCell)
        {
            if (targetCell)
            {
                AIPathInfo pathInfo = new AIPathInfo
                {
                    StartCell = GetCell(),
                    TargetCell = targetCell,
                    bNoDestinationUnits = true,
                    bIgnoreUnitsOnPath = true,
                    bTakeWeightIntoAccount = false
                };

                List<LevelCellBase> cellPath = AStarAlgorithmUtils.GetPath(pathInfo);

                LevelCellBase StartingCell = GetCell();

                Vector3 StartPos = GetCell().GetAllignPos(this);

                SetCurrentCell(targetCell);

                foreach (LevelCellBase cell in cellPath)
                {
                    FogOfWar fogOfWar = TacticBattleManager.GetFogOfWar();
                    if (fogOfWar)
                    {
                        if (GetTeam() == BattleTeam.Friendly)
                        {
                            fogOfWar.CheckPoint(cell);
                        }
                        else
                        {
                            CheckCellVisibility( cell );
                        }
                    }

                    float timeTo = 0;
                    Vector3 endPos = cell.GetAllignPos(this);
                    while (timeTo < 1.5f)
                    {
                        timeTo += Time.deltaTime * AStarAlgorithmUtils.GetMovementSpeed();
                        gameObject.transform.position = Vector3.MoveTowards(StartPos, endPos, timeTo);

                        await Awaitable.NextFrameAsync();
                    }

                    gameObject.transform.position = endPos;
                    StartPos = cell.GetAllignPos(this);

                    await Awaitable.WaitForSecondsAsync(AStarAlgorithmUtils.GetWaitTime());

                    if ( cell != StartingCell )
                    {
                        StatusEffectUtils.HandleUnitOnCell(this, cell);
                    }
                }
            }
        }

        /// <summary>
        /// Used for Pathfinding Moving on Grid, no Teleport
        /// </summary>
        /// <param name="InTargetCell"></param>
        /// <param name="InAllowedCells"></param>
        /// <returns></returns>
        public List<LevelCellBase> GetPathTo(LevelCellBase InTargetCell, List<LevelCellBase> InAllowedCells)
        {
            AIPathInfo pathInfo = new AIPathInfo
            {
                StartCell = GetCell(),
                TargetCell = InTargetCell,
                bNoDestinationUnits = true,
                bIgnoreUnitsOnPath = true,
                bTakeWeightIntoAccount = TacticBattleManager.IsTeamAI(GetTeam()),
                AllowedCells = InAllowedCells,
                bAllowBlocked = _unitData.m_bIsFlying
            };

            List<LevelCellBase> cellPath = AStarAlgorithmUtils.GetPath(pathInfo);

            return cellPath;
        }

        private void RemoveMovementPoints(int moveCount)
        {
            CurrentMovementPoints -= moveCount;
            if (CurrentMovementPoints < 0)
            {
                CurrentMovementPoints = 0;
            }
        }

        #endregion

        #region EventListeners

        public virtual void HandleTurnStarted()
        {
            CurrentMovementPoints = 5;
        }

        public abstract void BroadcastActionTriggerByTag(string actionTagName);
        public abstract float GrabActionValueDataByIndexTag(int additionalIndex, params string[] tags);

        #endregion

    }
}
