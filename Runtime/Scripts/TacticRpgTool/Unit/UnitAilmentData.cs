using UnityEngine.Serialization;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem
{
    [System.Serializable]
    public struct UnitAilmentData
    {
        [FormerlySerializedAs("m_ailment")] public StatusEffect mStatusEffect;
        public int m_NumTurns;

        public UnitAilmentData(StatusEffect inStatusEffect, int InNumTurns = 0)
        {
            mStatusEffect = inStatusEffect;
            m_NumTurns = InNumTurns;
        }

        public bool IsEqual(UnitAilmentData other)
        {
            return mStatusEffect == other.mStatusEffect;
        }

        public bool IsEqual(StatusEffect other)
        {
            return mStatusEffect == other;
        }
    }
} 