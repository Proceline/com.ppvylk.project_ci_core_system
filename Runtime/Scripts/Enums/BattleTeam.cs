using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Enums
{
    /// <summary>
    /// Represents the team affiliation of a battle entity
    /// </summary>
    public enum BattleTeam
    {
        /// <summary>
        /// No team affiliation
        /// </summary>
        None = 0,

        /// <summary>
        /// Player's team
        /// </summary>
        Player = 1,

        /// <summary>
        /// Enemy team
        /// </summary>
        Enemy = 2,

        /// <summary>
        /// Neutral team
        /// </summary>
        Neutral = 3
    }
} 