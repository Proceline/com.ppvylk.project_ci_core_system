using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit
{
    [System.Serializable]
    public struct UnitAbilityPlayerData
    {
        public UnitAbility unitAbility;

        public AnimationClip AssociatedAnimation;

        public float ExecuteAfterTime;

        public AudioClip AudioOnStart;

        public AudioClip AudioOnExecute;

        public AnimationClip GetAnimation()
        {
            return AssociatedAnimation;
        }
    }

    [CreateAssetMenu(fileName = "NewUnitData", menuName = "ProjectCI Tools/Create UnitData", order = 1)]
    public class UnitData : ScriptableObject
    {
        public string m_UnitName;
        public GameObject m_Model;

        [SerializeField]
        public string m_UnitClass;

        [Space(5)]
        
        [Header("Animations")]
        public AnimationClip m_IdleAnimation;
        public AnimationClip m_MovementAnimation;
        public AnimationClip m_DamagedAnimation;
        public AnimationClip m_HealAnimation;

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

        [Header("Ability")]
        public UnitAbilityPlayerData[] m_Abilities;

        [Space(5)]

        public AbilityParticle[] m_SpawnOnHeal;

        [Space(5)]

        [Header("Points")]
        public int m_MovementPoints;
        public int m_AbilityPoints;

        [Space(5)]

        [Header("Health")]
        public int m_Health;
        public int m_Armor;
        public int m_MagicalArmor;

        void Reset()
        {
            m_bLookAtTargets = true;
        }
    }
}
