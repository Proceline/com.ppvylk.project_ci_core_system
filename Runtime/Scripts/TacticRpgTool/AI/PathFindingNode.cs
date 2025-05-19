using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI
{
    public class PathFindingNode
    {
        public ILevelCell Cell;
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
            if(InPathNode.Parent != null)
            {
                count += InternalGetDistance(InPathNode.Parent);
            }
            count++;

            return count;
        }

        public PathFindingNode(ILevelCell InCell)
        {
            Cell = InCell;
        }

        public PathFindingNode(ILevelCell InCell, PathFindingNode InParent, int GCost, int HCost)
        {
            Cell = InCell;
            Parent = InParent;
            G = GCost;
            H = HCost;
        }

        public PathFindingNode(ILevelCell InCell, PathFindingNode InParent)
        {
            Cell = InCell;
            Parent = InParent;
            G = GetDistanceToStart();
        }
    }
}
