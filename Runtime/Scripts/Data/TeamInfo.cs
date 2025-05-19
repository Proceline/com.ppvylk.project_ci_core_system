using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Data
{
    /// <summary>
    /// Represents information about a team in the battle system
    /// </summary>
    [System.Serializable]
    public struct TeamInfo
    {
        public int TeamId;

        public TeamInfo(int teamId)
        {
            TeamId = teamId;
        }

        public static TeamInfo InvalidTeam()
        {
            return new TeamInfo(-1);
        }

        public bool IsValid()
        {
            return TeamId != -1;
        }

        public override bool Equals(object obj)
        {
            TeamInfo otherTeamInfo = (TeamInfo)obj;
            return otherTeamInfo.TeamId == TeamId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
} 