using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Enums;

namespace ProjectCI.CoreSystem.Runtime.Units.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that defines the data configuration for a battle unit
    /// </summary>
    [CreateAssetMenu(fileName = "New Battle Unit Data", menuName = "ProjectCI/Units/Battle Unit Data")]
    public class BattleUnitData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string unitName;
        [SerializeField] private GameObject model;
        [SerializeField] private string unitClass;

        [Header("Misc")]
        [SerializeField] private bool shouldLookAtTargets = true;
        [SerializeField] private bool isFlying;
        [SerializeField] private float heightOffset;

        [Header("Points")]
        [SerializeField] private int maxMovementPoints;
        [SerializeField] private int maxAbilityPoints;

        [Header("Health")]
        [SerializeField] private float maxHealth;
        [SerializeField] private int armor;
        [SerializeField] private int magicalArmor;

        // IUnitData Implementation
        public string UnitName => unitName;
        public GameObject Model => model;
        public bool IsFlying => isFlying;
        public bool ShouldLookAtTargets => shouldLookAtTargets;
        public float HeightOffset => heightOffset;
        public int MaxMovementPoints => maxMovementPoints;
        public int MaxAbilityPoints => maxAbilityPoints;
        public float MaxHealth => maxHealth;

        // Additional Properties
        public string UnitClass => unitClass;
        public int Armor => armor;
        public int MagicalArmor => magicalArmor;
    }
} 