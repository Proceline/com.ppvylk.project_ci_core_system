using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Levels.Data
{
    /// <summary>
    /// Represents a pair of position and its corresponding battle cell.
    /// Used for managing cell positions in the grid system.
    /// </summary>
    [System.Serializable]
    public struct BattleCellPositionPair
    {
        [SerializeField]
        public Vector2 Position;

        [SerializeField]
        public BattleCell Cell;

        public BattleCellPositionPair(Vector2 InPosition, BattleCell InCell) : this()
        {
            this.Position = InPosition;
            this.Cell = InCell;
        }
    }
} 