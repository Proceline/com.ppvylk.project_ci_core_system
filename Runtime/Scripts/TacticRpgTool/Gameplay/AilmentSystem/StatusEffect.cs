using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem
{
    [System.Serializable]
    public struct AilmentExecutionInfo
    {
        public AbilityParticle[] m_SpawnOnReciever;
        public AbilityParamBase[] m_Params;
        public AudioClip m_AudioClip;
    }

    [CreateAssetMenu(fileName = "New StatusEffect", menuName = "ProjectCI Tools/Create New Status Effect", order = 1)]
    public class StatusEffect : ScriptableObject
    {
        public string effectName;
        public string m_Description;

        public int numEffectedTurns;

        public AilmentExecutionInfo m_ExecuteOnStartOfTurn;
        public AilmentExecutionInfo m_ExecuteOnEndOfTurn;
    }
}
