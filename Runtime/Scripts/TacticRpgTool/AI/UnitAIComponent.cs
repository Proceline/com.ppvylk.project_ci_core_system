using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI
{
    public class UnitAIComponent : MonoBehaviour
    {
        [SerializeField]
        UnitAI m_AIData;

        public UnitAI GetAIData()
        {
            return m_AIData;
        }

        public void SetAIData(UnitAI InAIData)
        {
            m_AIData = InAIData;
        }
    }
}
