using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Core.Teams
{
    [System.Serializable]
    public enum PlayerType
    {
        Human,
        AI
    }

    public class BattleTeamData : ScriptableObject
    {
        private BattleTeam m_Team;

        public void SetTeam(BattleTeam team)
        {
            m_Team = team;
        }

        public BattleTeam GetTeam()
        {
            return m_Team;
        }

        public T GetAs<T>() where T : BattleTeamData
        {
            return this as T;
        }
    }
} 