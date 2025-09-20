using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

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

        private int GetDistanceToStart()
        {
            return InternalGetDistance(this);
        }

        private int InternalGetDistance(PathFindingNode inPathNode)
        {
            int count = 0;
            if (inPathNode.Parent != null)
            {
                count += InternalGetDistance(inPathNode.Parent);
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

        public PathFindingNode(LevelCellBase inCell, PathFindingNode inParent)
        {
            Cell = inCell;
            Parent = inParent;
            G = GetDistanceToStart();
        }
    }
}
