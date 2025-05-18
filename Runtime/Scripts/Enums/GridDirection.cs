using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Enums
{
    /// <summary>
    /// Represents the eight cardinal and intercardinal directions in a grid system.
    /// Used for navigation and adjacency calculations in grid-based games.
    /// </summary>
    public enum GridDirection
    {
        /// <summary>
        /// North direction (0 degrees)
        /// </summary>
        North,
        
        /// <summary>
        /// Northeast direction (45 degrees)
        /// </summary>
        NorthEast,
        
        /// <summary>
        /// East direction (90 degrees)
        /// </summary>
        East,
        
        /// <summary>
        /// Southeast direction (135 degrees)
        /// </summary>
        SouthEast,
        
        /// <summary>
        /// South direction (180 degrees)
        /// </summary>
        South,
        
        /// <summary>
        /// Southwest direction (225 degrees)
        /// </summary>
        SouthWest,
        
        /// <summary>
        /// West direction (270 degrees)
        /// </summary>
        West,
        
        /// <summary>
        /// Northwest direction (315 degrees)
        /// </summary>
        NorthWest
    }
} 