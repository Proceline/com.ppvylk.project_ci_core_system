using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Effects.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject that defines a battle effect
    /// </summary>
    [CreateAssetMenu(fileName = "New Battle Effect", menuName = "ProjectCI/Effects/Battle Effect")]
    public class BattleEffectSO : ScriptableObject
    {
        public string m_AilmentName;
        public string m_Description;
        public int m_NumEffectedTurns;
        public EffectInfo m_ExecuteOnStartOfTurn;
        public EffectInfo m_ExecuteOnEndOfTurn;
    }

    /// <summary>
    /// Information about an effect
    /// </summary>
    [System.Serializable]
    public struct EffectInfo
    {
    }
} 