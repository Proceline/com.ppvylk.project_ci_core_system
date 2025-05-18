using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Levels.Data
{
    /// <summary>
    /// Manages the mapping between grid positions and their corresponding battle cells.
    /// Used for tracking all cells in the grid system.
    /// </summary>
    [System.Serializable]
    public class BattleGridCellMap
    {
        [SerializeField]
        public List<BattleCellPositionPair> Pairs;

        public BattleGridCellMap()
        {
            Pairs = new List<BattleCellPositionPair>();
        }

        public BattleCellPositionPair Add(Vector2 InPosition, BattleCell InCell)
        {
            foreach (BattleCellPositionPair pair in Pairs)
            {
                if (pair.Position == InPosition)
                {
                    return pair;
                }
            }

            Pairs.Add(new BattleCellPositionPair(InPosition, InCell));
            return Pairs[Pairs.Count - 1];
        }

        public void Remove(Vector2 InPosition)
        {
            foreach (var pair in Pairs)
            {
                if (pair.Position == InPosition)
                {
                    Pairs.Remove(pair);
                    break;
                }
            }
        }

        public bool ContainsKey(Vector2 InPosition)
        {
            foreach (var pair in Pairs)
            {
                if (pair.Position == InPosition)
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            Pairs.Clear();
        }

        public BattleCell this[Vector2 InPosition]
        {
            get
            {
                foreach (var pair in Pairs)
                {
                    if (pair.Position == InPosition)
                    {
                        return pair.Cell;
                    }
                }

                return null;
            }
        }
    }
} 