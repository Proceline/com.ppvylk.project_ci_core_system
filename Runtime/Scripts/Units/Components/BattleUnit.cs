using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.Enums;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Levels;
using ProjectCI.CoreSystem.Runtime.Core;

namespace ProjectCI.CoreSystem.Runtime.Units.Components
{
    /// <summary>
    /// Represents a unit in a battle system that can perform actions, move, and interact with other units
    /// </summary>
    public class BattleUnit : BattleObject, IUnit
    {
        // IIdentifier implementation
        private Guid id;
        public Guid ID => id;

        public IUnitData UnitData {get; private set;}
        
        // State variables
        private UnitState currentState;
        private bool isMoving;
        private bool isUsingAbility;
        private bool isTarget;
        private bool isActivated;
        private bool isDead;
        
        // Resource variables
        private IUnitResource<int> currentMovementPoints;
        private IUnitResource<int> currentAbilityPoints;

        // Events
        private UnityEvent OnMovementComplete = new UnityEvent();
        private List<BattleCell> editedCells = new List<BattleCell>();

        // IUnit properties
        public UnitState CurrentState => currentState;
        public bool IsMoving => isMoving;
        public bool IsUsingAbility => isUsingAbility;
        public bool IsTarget => isTarget;
        public bool IsActivated => isActivated;
        public bool IsDead => isDead;
        public int CurrentMovementPoints => currentMovementPoints.CurrentValue;
        public int CurrentAbilityPoints => currentAbilityPoints.CurrentValue;

        // TODO: Add resource system
        // private Dictionary<ResourceType, IUnitResource> resources;

        // TODO: Add ability system
        // private List<IUnitAbility> abilities;

        // TODO: Add ailment system
        // private IUnitAilmentContainer ailmentContainer;

        protected override void Awake()
        {
            base.Awake();
            GenerateNewID();
        }

        public override void Initialize()
        {
            base.Initialize();
            // Base initialization logic
        }

        public void SetUnitData(IUnitData data)
        {
            UnitData = data;
            // TODO: Initialize resources
            // TODO: Initialize abilities
            // TODO: Initialize ailment container
            
            if (UnitData != null)
            {
                currentMovementPoints.SetValue(UnitData.MaxMovementPoints, UnitData.MaxMovementPoints);
                currentAbilityPoints.SetValue(UnitData.MaxAbilityPoints, UnitData.MaxAbilityPoints);
            }
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
            // TODO: Setup any post-initialization logic
            // TODO: Setup visibility system
            GetCell().HandleVisibilityChanged();
        }

        public virtual void SelectUnit()
        {
            SetupMovement();
        }

        public void CleanUp()
        {
            currentState = UnitState.Idle;
            isMoving = false;
            isUsingAbility = false;
            isTarget = false;
            
            // TODO: Clean up resources
            // TODO: Clean up abilities
            // TODO: Clean up ailments

            foreach (BattleCell cell in editedCells)
            {
                if (cell)
                {
                    BattleManager.ResetCellState(cell);
                }
            }

            editedCells.Clear();
        }

        public void GenerateNewID()
        {
            id = Guid.NewGuid();
        }

        #region Movement System
        public void SetupMovement()
        {
            if (IsMoving || BattleManager.IsActionBeingPerformed())
            {
                return;
            }

            CleanUp();

            if (UnitData.MovementShape == null)
            {
                return;
            }

            currentState = UnitState.Moving;
            List<BattleCell> abilityCells = GetAllowedMovementCells();

            foreach (BattleCell cell in abilityCells)
            {
                if (cell && cell.IsVisible())
                {
                    BattleManager.SetCellState(cell, BattleCellState.Movement);
                }
            }

            editedCells.AddRange(abilityCells);
            BattleManager.Get().UpdateHoverCells();
        }

        public bool ExecuteMovement(BattleCell TargetCell, UnityEvent InOnMovementComplete)
        {
            if (IsMoving)
            {
                return false;
            }

            if (UnitData.MovementShape == null || TargetCell == null)
            {
                return false;
            }

            if (!TargetCell.IsVisible())
            {
                return false;
            }

            List<BattleCell> abilityCells = GetAllowedMovementCells();
            if (!abilityCells.Contains(TargetCell))
            {
                return false;
            }

            InOnMovementComplete.AddListener(HandleMovementFinished);
            TraverseTo(TargetCell, InOnMovementComplete, abilityCells);

            BattleManager.CheckWinConditions();

            CleanUp();
            return true;
        }

        public bool ExecuteMovement(BattleCell TargetCell)
        {
            UnityEvent OnMovementComplete = new UnityEvent();
            return ExecuteMovement(TargetCell, OnMovementComplete);
        }

        public void TraverseTo(BattleCell InTargetCell, UnityEvent OnMovementComplete = null, List<BattleCell> InAllowedCells = null)
        {
            StartCoroutine(EnumeratorTraverseTo(InTargetCell, OnMovementComplete, InAllowedCells));
        }

        public void MoveTo(BattleCell InTargetCell)
        {
            StartCoroutine(InternalMoveTo(InTargetCell));
        }

        public IEnumerator EnumeratorTraverseTo(BattleCell InTargetCell, UnityEvent OnMovementComplete = null, List<BattleCell> InAllowedCells = null)
        {
            if (InTargetCell)
            {
                isMoving = true;

                PlayAnimation(UnitData.MovementAnimation);

                BattleManager.AddActionBeingPerformed();

                List<BattleCell> cellPath = GetPathTo(InTargetCell, InAllowedCells);

                Vector3 StartPos = GetCell().GetAllignPos(this);

                int MovementCount = 0;

                BattleCell FinalCell = InTargetCell;
                BattleCell StartingCell = GetCell();

                foreach (BattleCell cell in cellPath)
                {
                    float TimeTo = 0;
                    Vector3 EndPos = cell.GetAllignPos(this);
                    while (TimeTo < 1.5f)
                    {
                        TimeTo += Time.deltaTime * 2f; // TODO: Replace with proper movement speed
                        transform.position = Vector3.MoveTowards(StartPos, EndPos, TimeTo);
                        yield return new WaitForSeconds(0.00001f);
                    }

                    transform.position = EndPos;
                    StartPos = cell.GetAllignPos(this);
                    yield return new WaitForSeconds(0.1f); // TODO: Replace with proper wait time

                    if (cell != StartingCell)
                    {
                        // TODO: Handle unit on cell effects
                        PlayTravelAudio();
                    }

                    if (IsDead)
                    {
                        break;
                    }

                    if (MovementCount++ >= CurrentMovementPoints)
                    {
                        FinalCell = cell;
                        break;
                    }
                }

                if (!IsDead)
                {
                    SetCurrentCell(FinalCell);
                    RemoveMovementPoints(cellPath.Count - 1);
                    PlayAnimation(UnitData.IdleAnimation);
                }

                BattleManager.RemoveActionBeingPerformed();
                isMoving = false;

                if (OnMovementComplete != null)
                {
                    OnMovementComplete.Invoke();
                }
            }
        }

        public List<BattleCell> GetPathTo(BattleCell InTargetCell, List<BattleCell> InAllowedCells = null)
        {
            // TODO: Implement proper pathfinding
            return new List<BattleCell> { InTargetCell };
        }

        public void RemoveMovementPoints(int InMoveCount)
        {
            currentMovementPoints.SetValue(currentMovementPoints.CurrentValue - InMoveCount, currentMovementPoints.MaxValue);
        }
        #endregion

        #region Ability System
        public void SetupAbility(int abilityIndex)
        {
            if (abilityIndex < UnitData.Abilities.Length)
            {
                SetupAbility(UnitData.Abilities[abilityIndex]);
            }
        }

        public void SetupAbility(IUnitAbility InAbility)
        {
            if (IsMoving || BattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if (InAbility != null)
            {
                if (InAbility.ActionPointCost <= CurrentAbilityPoints)
                {
                    CleanUp();
                    currentState = UnitState.UsingAbility;
                    isUsingAbility = true;

                    List<BattleCell> EditedAbilityCells = InAbility.Setup(this);
                    editedCells.AddRange(EditedAbilityCells);

                    BattleManager.Get().UpdateHoverCells();
                }
            }
        }

        public void ExecuteAbility(BattleCell InCell)
        {
            if (isUsingAbility)
            {
                // TODO: Implement ability execution
                isUsingAbility = false;
            }
            else
            {
                CleanUp();
                HandleAbilityFinished();
            }
        }

        public void RemoveAbilityPoints(int InAbilityPoints)
        {
            currentAbilityPoints.SetValue(currentAbilityPoints.CurrentValue - InAbilityPoints, currentAbilityPoints.MaxValue);
        }
        #endregion

        #region Event System
        public void BindToOnMovementComplete(UnityAction InAction)
        {
            OnMovementComplete.AddListener(InAction);
        }

        public void UnBindFromOnMovementComplete(UnityAction InAction)
        {
            OnMovementComplete.RemoveListener(InAction);
        }
        #endregion

        #region Animation System
        public void PlayAnimation(AnimationClip InClip, bool bInPlayIdleAfter = false)
        {
            if (InClip)
            {
                Animator animator = GetComponent<Animator>();
                if (animator)
                {
                    animator.Play(InClip.name);
                }

                if (bInPlayIdleAfter)
                {
                    StartCoroutine(PlayClipAfterTime(UnitData.IdleAnimation, InClip.length));
                }
            }
        }

        private IEnumerator PlayClipAfterTime(AnimationClip InClip, float InTime)
        {
            yield return new WaitForSeconds(InTime);

            if (InClip)
            {
                Animator animator = GetComponent<Animator>();
                if (animator)
                {
                    if (IsMoving && UnitData.MovementAnimation)
                    {
                        animator.Play(UnitData.MovementAnimation.name);
                    }
                    else
                    {
                        animator.Play(InClip.name);
                    }
                }
            }
        }
        #endregion

        #region Event Handlers
        public virtual void HandleTurnStarted()
        {
            currentMovementPoints.SetValue(UnitData.MaxMovementPoints, UnitData.MaxMovementPoints);
            currentAbilityPoints.SetValue(UnitData.MaxAbilityPoints, UnitData.MaxAbilityPoints);
        }

        private void HandleAbilityFinished()
        {
            isUsingAbility = false;

            BattleTeam team = BattleManager.GetUnitTeam(this);
            if (BattleManager.IsTeamHuman(team) && BattleManager.IsPlaying() && !IsDead)
            {
                SetupMovement();
            }
        }

        private void HandleMovementFinished()
        {
            if (BattleManager.IsPlaying() && !IsDead)
            {
                SetupMovement();
            }

            OnMovementComplete.Invoke();
        }

        private void HandleHit()
        {
            PlayAnimation(UnitData.DamagedAnimation, true);
            PlayDamagedAudio();
        }

        private void HandleHeal()
        {
            PlayAnimation(UnitData.HealAnimation, true);
            PlayHealAudio();
        }

        private void HandleActivation()
        {
            BattleManager.HandleUnitActivated(this);
        }
        #endregion

        #region Audio
        private void PlayDamagedAudio()
        {
            if (UnitData.DamagedSound)
            {
                // TODO: Implement audio system
            }
        }

        private void PlayHealAudio()
        {
            if (UnitData.HealSound)
            {
                // TODO: Implement audio system
            }
        }

        private void PlayTravelAudio()
        {
            if (UnitData.TravelSound)
            {
                // TODO: Implement audio system
            }
        }
        #endregion

        public void LookAtCell(BattleCell cell)
        {
            if (cell && ShouldLookAtTargets())
            {
                gameObject.transform.LookAt(cell.transform.position);
            }
        }

        protected bool ShouldLookAtTargets()
        {
            return UnitData != null && UnitData.ShouldLookAtTargets;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanUp();
        }
    }
} 