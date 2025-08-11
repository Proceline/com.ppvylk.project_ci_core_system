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
        SoUnitData m_UnitData;
        private UnitAttributeContainer _runtimeAttributes;
        private UnitAttributeContainer _simulatedAttributes;

        protected int m_CurrentMovementPoints;

        bool m_bIsTarget = false;
        bool m_bIsMoving = false;
        bool m_bActivated = false;

        protected bool m_bIsDead = false;

        protected UnityEvent OnMovementPostComplete = new();

        protected readonly List<LevelCellBase> EditedCells = new();

        public event Action OnPreStandIdleAnimRequired;
        public event Action OnPreMovementAnimRequired;
        public event Action OnPreHitAnimRequired;
        public event Action OnPreHealAnimRequired;

        protected SoUnitData UnitData => m_UnitData;

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

        public override void PostInitialize()
        {
            GetCell().HandleVisibilityChanged();
        }
        
        protected void ResetCells()
        {
            foreach (LevelCellBase cell in EditedCells)
            {
                if (cell)
                {
                    TacticBattleManager.ResetCellState(cell);
                }
            }

            EditedCells.Clear();
        }
        
        public void LookAtCell(LevelCellBase InCell)
        {
            if(InCell && ShouldLookAtTargets())
            {
                gameObject.transform.LookAt(GetCellLookAtPos(InCell));
            }
        }
        
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
            m_UnitData = InUnitData;
        }

        public void SetActivated(bool bInNewActivateState)
        {
            if(m_bActivated != bInNewActivateState)
            {
                m_bActivated = bInNewActivateState;
                if(m_bActivated)
                {
                    HandleActivation();
                }
            }
        }

        public void SetAsTarget(bool bInIsTarget)
        {
            m_bIsTarget = bInIsTarget;
        }

        #endregion

        #region Getters

        public SoUnitData GetUnitData()
        {
            return m_UnitData;
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
            return m_bIsMoving;
        }

        public bool IsTarget()
        {
            return m_bIsTarget;
        }

        public bool IsActivated()
        {
            return m_bActivated;
        }

        public bool IsFlying()
        {
            return GetUnitData().m_bIsFlying;
        }

        public bool ShouldLookAtTargets()
        {
            return GetUnitData().m_bLookAtTargets;
        }

        public bool IsDead()
        {
            return m_bIsDead;
        }

        public virtual int GetCurrentMovementPoints()
        {
            return m_CurrentMovementPoints;
        }

        public Vector3 GetCellAllignPos(LevelCellBase InCell)
        {
            if(InCell)
            {
                GridObject Obj = InCell.GetObjectOnCell();
                if (Obj)
                {
                    return InCell.GetAllignPos(Obj);
                }
                else
                {
                    Vector3 AllignPos = InCell.gameObject.transform.position;
                    AllignPos.y = gameObject.transform.position.y;
                    return AllignPos;
                }
            }

            return Vector3.zero;
        }

        Vector3 GetCellLookAtPos(LevelCellBase InCell)
        {
            if(InCell)
            {
                Vector3 allignPos = InCell.GetAllignPos(this);
                allignPos.y = gameObject.transform.position.y;

                return allignPos;
            }

            return Vector3.zero;
        }
        
        #endregion

        #region MovementStuff

        public virtual List<LevelCellBase> GetAllowedMovementCells()
        {
            return m_UnitData.m_MovementShape.GetCellList(this, GetCell(), m_CurrentMovementPoints, m_UnitData.m_bIsFlying);
        }

        public virtual void SetupMovement()
        {
            if (IsMoving() || TacticBattleManager.IsActionBeingPerformed())
            {
                return;
            }

            ResetCells();

            if (!m_UnitData.m_MovementShape)
            {
                return;
            }

            AddState(UnitBattleState.Moving);
            List<LevelCellBase> allowedMovementCells = GetAllowedMovementCells();

            foreach (LevelCellBase cell in allowedMovementCells)
            {
                if (cell && cell.IsVisible())
                {
                    TacticBattleManager.SetCellState(cell, CellState.eMovement);
                }
            }

            EditedCells.AddRange(allowedMovementCells);

            TacticBattleManager.Get().UpdateHoverCells();
        }

        public bool ExecuteMovement(LevelCellBase targetCell)
        {
            if (IsMoving())
            {
                return false;
            }

            if (!m_UnitData.m_MovementShape || !targetCell)
            {
                return false;
            }

            if(!targetCell.IsVisible())
            {
                return false;
            }

            List<LevelCellBase> allowedMovementCells = GetAllowedMovementCells();
            if (!allowedMovementCells.Contains(targetCell))
            {
                return false;
            }

            TraverseTo(targetCell, OnMovementPostComplete, allowedMovementCells);

            TacticBattleManager.CheckWinConditions();

            ResetCells();
            return true;
        }

        private async void TraverseTo(LevelCellBase InTargetCell, 
            UnityEvent onMovementComplete, List<LevelCellBase> InAllowedCells)
        {
            await OnGridTraverseTo(InTargetCell, onMovementComplete, InAllowedCells);
        }

        public async void ForceMoveTo(LevelCellBase InTargetCell)
        {
            await ForceMoveToInternally(InTargetCell);
        }

        internal async Awaitable OnGridTraverseTo(LevelCellBase InTargetCell, UnityEvent onMovementComplete, List<LevelCellBase> InAllowedCells)
        {
            if (InTargetCell)
            {
                m_bIsMoving = true;

                OnPreMovementAnimRequired?.Invoke();

                TacticBattleManager.AddActionBeingPerformed();
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
                        PlayTravelAudio();
                    }

                    if (IsDead())
                    {
                        break;
                    }

                    if(movementCount++ >= m_CurrentMovementPoints)
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

                TacticBattleManager.RemoveActionBeingPerformed();

                m_bIsMoving = false;

                if (onMovementComplete != null)
                {
                    onMovementComplete.Invoke();
                }
            }
        }

        protected virtual async Awaitable ForceMoveToInternally(LevelCellBase InTargetCell)
        {
            if (InTargetCell)
            {
                TacticBattleManager.AddActionBeingPerformed();

                AIPathInfo pathInfo = new AIPathInfo
                {
                    StartCell = GetCell(),
                    TargetCell = InTargetCell,
                    bNoDestinationUnits = true,
                    bIgnoreUnitsOnPath = true,
                    bTakeWeightIntoAccount = false
                };

                List<LevelCellBase> cellPath = AStarAlgorithmUtils.GetPath(pathInfo);

                LevelCellBase StartingCell = GetCell();

                Vector3 StartPos = GetCell().GetAllignPos(this);

                SetCurrentCell(InTargetCell);

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

                    float TimeTo = 0;
                    Vector3 EndPos = cell.GetAllignPos(this);
                    while (TimeTo < 1.5f)
                    {
                        TimeTo += Time.deltaTime * AStarAlgorithmUtils.GetMovementSpeed();
                        gameObject.transform.position = Vector3.MoveTowards(StartPos, EndPos, TimeTo);

                        await Awaitable.WaitForSecondsAsync(0.00001f);
                    }

                    gameObject.transform.position = EndPos;
                    StartPos = cell.GetAllignPos(this);

                    await Awaitable.WaitForSecondsAsync(AStarAlgorithmUtils.GetWaitTime());

                    if ( cell != StartingCell )
                    {
                        StatusEffectUtils.HandleUnitOnCell(this, cell);
                    }
                }

                TacticBattleManager.RemoveActionBeingPerformed();
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
                bAllowBlocked = m_UnitData.m_bIsFlying
            };

            List<LevelCellBase> cellPath = AStarAlgorithmUtils.GetPath(pathInfo);

            return cellPath;
        }

        public void RemoveMovementPoints(int InMoveCount)
        {
            m_CurrentMovementPoints -= InMoveCount;
            if(m_CurrentMovementPoints < 0)
            {
                m_CurrentMovementPoints = 0;
            }
        }

        #endregion

        #region EventListeners

        public virtual void HandleTurnStarted()
        {
            m_CurrentMovementPoints = 5;
        }

        protected virtual void HandleDeath()
        {
            TacticBattleManager.HandleUnitDeath(this);
        }

        protected virtual void DestroyObj()
        {
            TacticBattleManager.UnBindFromOnFinishedPerformedActions(DestroyObj);
            Destroy(gameObject);
        }

        protected void PlayHitVisualResult()
        {
            bool bShowHitAnimationOnMove = TacticBattleManager.GetRules().GetGameplayData().bShowHitAnimOnMove;
            if (!IsMoving() || bShowHitAnimationOnMove)
            {
                OnPreHitAnimRequired?.Invoke();
            }

            PlayDamagedAudio();
        }

        protected void PlayHealVisualResult()
        {
            OnPreHealAnimRequired?.Invoke();

            PlayHealAudio();

            AbilityParticle[] particles = GetUnitData().m_SpawnOnHeal;
            foreach (AbilityParticle particle in particles)
            {
                if(particle)
                {
                    Vector3 pos = GetCell().gameObject.transform.position;

                    pos = GetCell().GetAllignPos(this);

                    AbilityParticle CreatedAbilityParticle = Instantiate(particle.gameObject, pos, GetCell().transform.rotation).GetComponent<AbilityParticle>();
                    CreatedAbilityParticle.Setup(null, this, GetCell());
                }
            }
        }

        void PlayDamagedAudio()
        {
            AudioClip clip = GetUnitData().m_DamagedSound;
            if(clip)
            {
                AudioPlayData audioData = new AudioPlayData(clip);
                AudioHandler.PlayAudio(audioData, gameObject.transform.position);
            }
        }

        void PlayHealAudio()
        {
            AudioClip clip = GetUnitData().m_HealSound;
            if (clip)
            {
                AudioPlayData audioData = new AudioPlayData(clip);
                AudioHandler.PlayAudio(audioData, gameObject.transform.position);
            }
        }

        void PlayTravelAudio()
        {
            AudioClip clip = GetUnitData().m_TravelSound;
            if (clip)
            {
                AudioPlayData audioData = new AudioPlayData(clip);
                AudioHandler.PlayAudio(audioData, gameObject.transform.position);
            }
        }

        void HandleActivation()
        {
            TacticBattleManager.HandleUnitActivated(this);
        }

        #endregion

    }
}
