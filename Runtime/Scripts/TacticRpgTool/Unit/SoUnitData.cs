using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.Attributes;
using System;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit
{
    [Serializable]
    public struct AttributeValuePair
    {
        public AttributeType m_AttributeType;
        public int m_Value;
        public int m_GainValue;
    }

    [CreateAssetMenu(fileName = "NewUnitData", menuName = "ProjectCI Tools/Create UnitData", order = 1)]
    public class SoUnitData : ScriptableObject
    {
        public string m_UnitName;

        [SerializeField]
        public string m_UnitClass;

        [SerializeField]
        public AttributeValuePair[] originalAttributes;

        [Space(5)]

        [Header("Sounds")]
        public AudioClip m_TravelSound;
        public AudioClip m_DamagedSound;
        public AudioClip m_HealSound;
        public AudioClip m_DeathSound;

        [Space(5)]
        [Header("Misc")]

        public bool m_bLookAtTargets;

        [Space(5)]

        public bool m_bIsFlying;
        public float m_HeightOffset;

        [Space(5)]

        public AbilityShape m_MovementShape;

        [Space(5)]

        public AbilityParticle[] m_SpawnOnHeal;

        [Space(5)]

        [Header("Points")]
        public int m_AbilityPoints;

        public virtual void InitializeUnitDataToGridUnit(GridPawnUnit pawnUnit)
        {
            // Empty
        }
        
        void Reset()
        {
            m_bLookAtTargets = true;
        }
    }
}
