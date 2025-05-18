using UnityEngine;
using System;
using ProjectCI_CoreSystem.Runtime.Scripts.Enums;
using ProjectCI_CoreSystem.Runtime.Scripts.Units.Interfaces;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Units.Components
{
    /// <summary>
    /// Represents a unit in a battle system that can perform actions, move, and interact with other units
    /// </summary>
    public class BattleUnit : MonoBehaviour, IUnit
    {
        
        // IIdentifier implementation
        private Guid id;
        public Guid ID => id;

        [SerializeField] private IUnitData unitData;
        
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

        private void Awake()
        {
            GenerateNewID();
        }

        public void Initialize(IUnitData data)
        {
            unitData = data;
            // TODO: Initialize resources
            // TODO: Initialize abilities
            // TODO: Initialize ailment container
            
            if (unitData != null)
            {
                currentMovementPoints.SetValue(unitData.MaxMovementPoints, unitData.MaxMovementPoints);
                currentAbilityPoints.SetValue(unitData.MaxAbilityPoints, unitData.MaxAbilityPoints);
            }
        }

        public void PostInitialize()
        {
            // TODO: Setup any post-initialization logic
            // TODO: Setup visibility system
        }

        public void OnSelected()
        {
            // TODO: Implement unit selection logic
            // TODO: Setup movement range
            // TODO: Setup ability targets
        }

        public void OnDeselected()
        {
            // TODO: Implement unit deselection logic
            // TODO: Clear movement range
            // TODO: Clear ability targets
        }

        public void Cleanup()
        {
            currentState = UnitState.Idle;
            isMoving = false;
            isUsingAbility = false;
            isTarget = false;
            
            // TODO: Clean up resources
            // TODO: Clean up abilities
            // TODO: Clean up ailments
        }

        public void GenerateNewID()
        {
            id = Guid.NewGuid();
        }

        // TODO: Add movement system
        // public void Move(Vector3 position) { }

        // TODO: Add ability system
        // public void UseAbility(IUnitAbility ability, IUnit target) { }

        // TODO: Add resource system
        // public void ModifyResource(ResourceType type, float amount) { }

        // TODO: Add state change system
        // public void ChangeState(UnitState newState) { }
    }
} 