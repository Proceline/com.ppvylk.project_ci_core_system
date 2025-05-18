using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Enums
{
    /// <summary>
    /// Represents the different visual states a battle cell can be in.
    /// Used for visual feedback and interaction states in the grid system.
    /// </summary>
    public enum BattleCellState
    {
        /// <summary>
        /// Default state of the cell
        /// </summary>
        Normal,

        /// <summary>
        /// Cell is being hovered over by the mouse
        /// </summary>
        Hover,

        /// <summary>
        /// Cell is in a positive state (e.g., valid target, friendly area)
        /// </summary>
        Positive,

        /// <summary>
        /// Cell is in a negative state (e.g., invalid target, hostile area)
        /// </summary>
        Negative,

        /// <summary>
        /// Cell is in a movement state (e.g., valid movement target)
        /// </summary>
        Movement
    }
} 