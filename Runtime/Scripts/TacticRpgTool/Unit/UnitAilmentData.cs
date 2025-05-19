namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem
{
    [System.Serializable]
    public struct UnitAilmentData
    {
        public Ailment m_ailment;
        public int m_NumTurns;

        public UnitAilmentData(Ailment InAilment, int InNumTurns = 0)
        {
            m_ailment = InAilment;
            m_NumTurns = InNumTurns;
        }

        public bool IsEqual(UnitAilmentData other)
        {
            return m_ailment == other.m_ailment;
        }

        public bool IsEqual(Ailment other)
        {
            return m_ailment == other;
        }
    }
} 