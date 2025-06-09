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
        AbilityConfirming
    }

    public abstract class GridPawnUnit : GridObject, IStateOwner<UnitBattleState>
    {
        SoUnitData m_UnitData;
        private UnitAttributeContainer _runtimeAttributes;
        private UnitAttributeContainer _simulatedAttributes;

        UnitAbilityCore m_CurrentAbility;

        protected int m_CurrentMovementPoints;
        protected int m_CurrentAbilityPoints;

        bool m_bIsTarget = false;
        bool m_bIsMoving = false;
        bool m_bIsAttacking = false;
        bool m_bActivated = false;

        protected bool m_bIsDead = false;

        protected UnityEvent OnMovementPostComplete = new UnityEvent();

        List<LevelCellBase> m_EditedCells = new List<LevelCellBase>();

        private List<UnitAbilityCore> _loadedAbilities = new List<UnitAbilityCore>();

        public event Action OnPreStandIdleAnimRequired;
        public event Action OnPreMovementAnimRequired;
        public event Action OnPreHitAnimRequired;
        public event Action OnPreHealAnimRequired;

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

        public virtual UnitAbilityCore GetEquippedAbility()
        {
            if (m_CurrentAbility)
            {
                return m_CurrentAbility;
            }

            return _loadedAbilities.Count > 0 ? _loadedAbilities[0] : null;
        }

        public override void PostInitalize()
        {
            GetCell().HandleVisibilityChanged();
        }

        public virtual void SelectUnit()
        {
            SetupMovement();
        }
        
        public void CleanUp()
        {
            ClearStates();

            foreach (LevelCellBase cell in m_EditedCells)
            {
                if (cell)
                {
                    TacticBattleManager.ResetCellState(cell);
                }
            }

            m_EditedCells.Clear();
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

        public void SetAbilities(List<UnitAbilityCore> InAbilities)
        {
            _loadedAbilities = InAbilities;
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

        public AilmentContainer GetAilmentContainer()
        {
            AilmentContainer ailmentHandler = GetComponent<AilmentContainer>();
            if (!ailmentHandler)
            {
                ailmentHandler = gameObject.AddComponent<AilmentContainer>();
            }

            return ailmentHandler;
        }

        public abstract UnitBattleState GetCurrentState();

        public abstract void AddState(UnitBattleState state);

        public abstract void RemoveLastState();

        public abstract void ClearStates();

        public bool IsMoving()
        {
            return m_bIsMoving;
        }

        public bool IsAttacking()
        {
            return m_bIsAttacking;
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

        public int GetCurrentAbilityPoints()
        {
            return m_CurrentAbilityPoints;
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

        #region AbilityStuff

        public List<UnitAbilityCore> GetAbilities() => _loadedAbilities;

        public List<LevelCellBase> GetAbilityHoverCells(LevelCellBase InCell)
        {
            List<LevelCellBase> outCells = new List<LevelCellBase>();
            var currentState = GetCurrentState();

            if (currentState == UnitBattleState.AbilityTargeting 
                || currentState == UnitBattleState.AbilityConfirming 
                || currentState == UnitBattleState.UsingAbility)
            {
                UnitAbilityCore ability = GetCurrentAbility();
                if (ability)
                {
                    List<LevelCellBase> abilityCells = ability.GetAbilityCells(this);
                    List<LevelCellBase> effectedCells = ability.GetEffectedCells(this, InCell);

                    if (abilityCells.Contains(InCell))
                    {
                        foreach (LevelCellBase currCell in effectedCells)
                        {
                            if (currCell)
                            {
                                BattleTeam EffectedTeam = (currCell == InCell) ? ability.GetEffectedTeam() : BattleTeam.All;

                                if (TacticBattleManager.CanCasterEffectTarget(GetCell(), currCell, EffectedTeam, ability.DoesAllowBlocked()))
                                {
                                    outCells.Add(currCell);
                                }
                            }
                        }
                    }
                }
            }

            return outCells;
        }

        public UnitAbilityCore GetCurrentAbility()
        {
            return m_CurrentAbility;
        }

        public void SetupAbility(int abilityIndex)
        {
            if(abilityIndex < _loadedAbilities.Count)
            {
                SetupAbility(_loadedAbilities[abilityIndex]);
            }
        }

        public virtual void SetupAbility(UnitAbilityCore InAbility)
        {
            if (IsMoving() || TacticBattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if (InAbility)
            {
                if (InAbility.GetActionPointCost() <= m_CurrentAbilityPoints)
                {
                    CleanUp();

                    m_CurrentAbility = InAbility;
                    AddState(UnitBattleState.UsingAbility);

                    List<LevelCellBase> EditedAbilityCells = m_CurrentAbility.Setup(this);
                    m_EditedCells.AddRange(EditedAbilityCells);

                    TacticBattleManager.Get().UpdateHoverCells();
                }
            }
        }

        public async Awaitable ShowResult(UnitAbilityCore ability, LevelCellBase target,
            List<Action<GridPawnUnit, LevelCellBase>> reactions)
        {
            m_bIsAttacking = true;
            UnityEvent OnAbilityComplete = new UnityEvent();
            OnAbilityComplete.AddListener(HandleAbilityFinished);

            await ability.ApplyResult(this, target, reactions, OnAbilityComplete);
        }

        public void RemoveAbilityPoints(int InAbilityPoints)
        {
            m_CurrentAbilityPoints -= InAbilityPoints;
            if (m_CurrentAbilityPoints < 0)
            {
                m_CurrentAbilityPoints = 0;
            }
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

            CleanUp();

            if (!m_UnitData.m_MovementShape)
            {
                return;
            }

            AddState(UnitBattleState.Moving);
            List<LevelCellBase> abilityCells = GetAllowedMovementCells();

            foreach (LevelCellBase cell in abilityCells)
            {
                if (cell && cell.IsVisible())
                {
                    TacticBattleManager.SetCellState(cell, CellState.eMovement);
                }
            }

            m_EditedCells.AddRange(abilityCells);

            TacticBattleManager.Get().UpdateHoverCells();
        }

        public bool ExecuteMovement(LevelCellBase TargetCell)
        {
            if (IsMoving())
            {
                return false;
            }
            
            UnityEvent onMovementPreCompleted = new UnityEvent();

            if (!m_UnitData.m_MovementShape || !TargetCell)
            {
                return false;
            }

            if(!TargetCell.IsVisible())
            {
                return false;
            }

            List<LevelCellBase> abilityCells = GetAllowedMovementCells();
            if (!abilityCells.Contains(TargetCell))
            {
                return false;
            }

            onMovementPreCompleted.AddListener(HandleTraversePreFinished);
            TraverseTo(TargetCell, onMovementPreCompleted, abilityCells);

            TacticBattleManager.CheckWinConditions();

            CleanUp();
            return true;
        }

        public async void TraverseTo(LevelCellBase InTargetCell, 
            UnityEvent OnMovementComplete = null, List<LevelCellBase> InAllowedCells = null)
        {
            await OnGridTraverseTo(InTargetCell, OnMovementComplete, InAllowedCells);
        }

        public async void ForceMoveTo(LevelCellBase InTargetCell)
        {
            await ForceMoveToInternally(InTargetCell);
        }

        public virtual async Awaitable OnGridTraverseTo(LevelCellBase InTargetCell, UnityEvent OnMovementComplete = null, List<LevelCellBase> InAllowedCells = null)
        {
            if (InTargetCell)
            {
                m_bIsMoving = true;

                OnPreMovementAnimRequired?.Invoke();

                TacticBattleManager.AddActionBeingPerformed();
                List<LevelCellBase> cellPath = GetPathTo(InTargetCell, InAllowedCells);
                Vector3 StartPos = GetCell().GetAllignPos(this);

                int MovementCount = 0;

                LevelCellBase FinalCell = InTargetCell;

                LevelCellBase StartingCell = GetCell();

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
                    Vector3 EndPos = cell.GetAllignPos(this);

                    LookAtCell(cell);

                    while (timeSpent < 1f && StartPos != EndPos)
                    {
                        await Awaitable.NextFrameAsync();
                        timeSpent += Time.deltaTime * AStarAlgorithmUtils.GetMovementSpeed();
                        gameObject.transform.position = Vector3.Lerp(StartPos, EndPos, timeSpent);
                    }

                    gameObject.transform.position = EndPos;
                    StartPos = cell.GetAllignPos(this);

                    await Awaitable.WaitForSecondsAsync(AStarAlgorithmUtils.GetWaitTime());

                    if ( cell != StartingCell )
                    {
                        AilmentHandlerUtils.HandleUnitOnCell(this, cell);
                        PlayTravelAudio();
                    }

                    if (IsDead())
                    {
                        break;
                    }

                    if(MovementCount++ >= m_CurrentMovementPoints)
                    {
                        FinalCell = cell;
                        break;
                    }
                }

                if ( !IsDead() )
                {
                    SetCurrentCell(FinalCell);
                    RemoveMovementPoints(cellPath.Count - 1);

                    OnPreStandIdleAnimRequired?.Invoke();
                }

                TacticBattleManager.RemoveActionBeingPerformed();

                m_bIsMoving = false;

                if (OnMovementComplete != null)
                {
                    OnMovementComplete.Invoke();
                }
            }
        }

        protected virtual async Awaitable ForceMoveToInternally(LevelCellBase InTargetCell)
        {
            if (InTargetCell)
            {
                TacticBattleManager.AddActionBeingPerformed();

                AIPathInfo pathInfo = new AIPathInfo();
                pathInfo.StartCell = GetCell();
                pathInfo.TargetCell = InTargetCell;
                pathInfo.bIgnoreUnits = true;
                pathInfo.bTakeWeightIntoAccount = false;

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
                        AilmentHandlerUtils.HandleUnitOnCell(this, cell);
                    }
                }

                TacticBattleManager.RemoveActionBeingPerformed();
            }
        }

        public List<LevelCellBase> GetPathTo(LevelCellBase InTargetCell, List<LevelCellBase> InAllowedCells = null)
        {
            AIPathInfo pathInfo = new AIPathInfo();
            pathInfo.StartCell = GetCell();
            pathInfo.TargetCell = InTargetCell;
            pathInfo.bIgnoreUnits = true;
            pathInfo.bTakeWeightIntoAccount = TacticBattleManager.IsTeamAI(GetTeam());
            pathInfo.AllowedCells = InAllowedCells;
            pathInfo.bAllowBlocked = m_UnitData.m_bIsFlying;

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
            m_CurrentAbilityPoints = m_UnitData.m_AbilityPoints;
        }

        protected virtual void HandleAbilityFinished()
        {
            m_bIsAttacking = false;

            BattleTeam team = TacticBattleManager.GetUnitTeam(this);
            if(TacticBattleManager.IsTeamHuman(team) && TacticBattleManager.IsPlaying() && !IsDead() )
            {
                SetupMovement();
            }
        }

        protected virtual void HandleTraversePreFinished()
        {
            if (TacticBattleManager.IsPlaying() && !IsDead())
            {
                SetupMovement();
            }

            OnMovementPostComplete.Invoke();
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
