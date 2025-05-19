using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.PlayerData
{
    [System.Serializable]
    public enum PlayerType
    {
        Human,
        AI
    }

    public class TeamData : ScriptableObject
    {
        GameTeam m_Team;

        public void SetTeam(GameTeam InTeam)
        {
            m_Team = InTeam;
        }

        public GameTeam GetTeam()
        {
            return m_Team;
        }

        public T GetAs<T>() where T : TeamData
        {
            return this as T;
        }
    }
}
