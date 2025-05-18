using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Levels.Data
{
    /// <summary>
    /// Information about a cell in the battle grid
    /// </summary>
    [System.Serializable]
    public struct BattleCellInfo
    {
        // Used for referencing, primarily used for placing enemies.
        [SerializeField]
        public string CellId;

        [SerializeField]
        public bool IsFriendlySpawnPoint;

        [SerializeField]
        [Tooltip("Only used if team2 is human.")]
        public bool IsHostileSpawnPoint;

        [SerializeField]
        public bool IsVisible;

        public static BattleCellInfo Default()
        {
            return new BattleCellInfo()
            {
                IsVisible = true,
                IsFriendlySpawnPoint = false,
                IsHostileSpawnPoint = false
            };
        }
    }
} 