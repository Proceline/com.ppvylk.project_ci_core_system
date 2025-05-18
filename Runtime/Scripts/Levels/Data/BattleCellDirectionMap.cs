using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Enums;

namespace ProjectCI.CoreSystem.Runtime.Levels.Data
{
    /// <summary>
    /// Manages the mapping between grid directions and their corresponding battle cells.
    /// Used for tracking adjacent cell relationships in the grid system.
    /// </summary>
    [System.Serializable]
    public class BattleCellDirectionMap
    {
        [SerializeField]
        public List<BattleCellDirectionPair> Pairs;

        public BattleCellDirectionMap()
        {
            Pairs = new List<BattleCellDirectionPair>();
        }

        public BattleCellDirectionPair Add(GridDirection InDirection, BattleCell InCell)
        {
            foreach (BattleCellDirectionPair pair in Pairs)
            {
                if (pair.Direction == InDirection)
                {
                    return pair;
                }
            }

            Pairs.Add(new BattleCellDirectionPair(InDirection, InCell));
            return Pairs[Pairs.Count - 1];
        }

        public bool ContainsKey(GridDirection InDirection)
        {
            foreach (var pair in Pairs)
            {
                if (pair.Direction == InDirection)
                {
                    return true;
                }
            }

            return false;
        }

        public BattleCell this[GridDirection InDirection]
        {
            get
            {
                foreach (var pair in Pairs)
                {
                    if (pair.Direction == InDirection)
                    {
                        return pair.Cell;
                    }
                }

                return null;
            }
        }
    }
} 