using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Levels.Pathfinding
{
    /// <summary>
    /// Represents a node in the battle pathfinding algorithm
    /// </summary>
    public class BattlePathFindingNode
    {
        public BattleCell Cell;
        public BattlePathFindingNode Parent;
        public int G;
        public int H;

        public override bool Equals(object obj)
        {
            BattlePathFindingNode other = (obj as BattlePathFindingNode);

            if (Cell == null || other.Cell == null)
            {
                return false;
            }

            return Cell.GetIndex() == other.Cell.GetIndex();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public float GetFScore()
        {
            return G + H;
        }

        public int GetDistanceToStart()
        {
            return InternalGetDistance(this);
        }

        int InternalGetDistance(BattlePathFindingNode InPathNode)
        {
            int count = 0;
            if(InPathNode.Parent != null)
            {
                count += InternalGetDistance(InPathNode.Parent);
            }
            count++;

            return count;
        }

        public BattlePathFindingNode(BattleCell InCell)
        {
            Cell = InCell;
        }

        public BattlePathFindingNode(BattleCell InCell, BattlePathFindingNode InParent, int GCost, int HCost)
        {
            Cell = InCell;
            Parent = InParent;
            G = GCost;
            H = HCost;
        }

        public BattlePathFindingNode(BattleCell InCell, BattlePathFindingNode InParent)
        {
            Cell = InCell;
            Parent = InParent;
            G = GetDistanceToStart();
        }
    }
} 