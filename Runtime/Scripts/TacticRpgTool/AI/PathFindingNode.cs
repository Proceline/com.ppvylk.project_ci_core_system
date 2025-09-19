﻿using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI
{
    public class PathFindingNode
    {
        public LevelCellBase Cell;
        public PathFindingNode Parent;
        public int G;
        public int H;

        public override bool Equals(object obj)
        {
            PathFindingNode other = (obj as PathFindingNode);

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

        int InternalGetDistance(PathFindingNode InPathNode)
        {
            int count = 0;
            if (InPathNode.Parent != null)
            {
                count += InternalGetDistance(InPathNode.Parent);
            }
            count++;

            return count;
        }

        public PathFindingNode(LevelCellBase InCell)
        {
            Cell = InCell;
        }

        public PathFindingNode(LevelCellBase InCell, PathFindingNode InParent, int gCost, int hCost)
        {
            Cell = InCell;
            Parent = InParent;
            G = gCost;
            H = hCost;
        }

        public PathFindingNode(LevelCellBase InCell, PathFindingNode InParent)
        {
            Cell = InCell;
            Parent = InParent;
            G = GetDistanceToStart();
        }
    }
}
