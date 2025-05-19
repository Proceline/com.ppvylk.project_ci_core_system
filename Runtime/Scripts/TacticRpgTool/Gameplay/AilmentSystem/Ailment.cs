using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem
{
    [System.Serializable]
    public struct AilmentExecutionInfo
    {
        public AbilityParticle[] m_SpawnOnReciever;
        public AbilityParam[] m_Params;
        public AudioClip m_AudioClip;
    }

    [CreateAssetMenu(fileName = "NewAilment", menuName = "TurnBasedTools/Create New Ailment", order = 1)]
    public class Ailment : ScriptableObject
    {
        public string m_AilmentName;
        public string m_Description;

        public int m_NumEffectedTurns;

        public AilmentExecutionInfo m_ExecuteOnStartOfTurn;
        public AilmentExecutionInfo m_ExecuteOnEndOfTurn;
    }
}
