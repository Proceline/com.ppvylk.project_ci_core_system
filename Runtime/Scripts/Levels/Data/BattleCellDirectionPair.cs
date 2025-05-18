using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Enums;

namespace ProjectCI.CoreSystem.Runtime.Levels.Data
{
    /// <summary>
    /// Represents a pair of grid direction and its corresponding battle cell.
    /// Used for managing adjacent cell relationships in the grid system.
    /// </summary>
    [System.Serializable]
    public struct BattleCellDirectionPair
    {
        [SerializeField]
        public GridDirection Direction;

        [SerializeField]
        public BattleCell Cell;

        public BattleCellDirectionPair(GridDirection InDirection, BattleCell InCell) : this()
        {
            this.Direction = InDirection;
            this.Cell = InCell;
        }
    }
} 