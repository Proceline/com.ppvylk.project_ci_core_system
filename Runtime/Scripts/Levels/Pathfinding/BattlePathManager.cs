using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Enums;

namespace ProjectCI.CoreSystem.Runtime.Levels.Pathfinding
{
    /// <summary>
    /// Manages pathfinding calculations for the battle system
    /// </summary>
    public static class BattlePathManager
    {
        static float m_CellWaitTime = 0.0f;
        static float m_MovementSpeed = 9.0f;

        public static float GetWaitTime()
        {
            return m_CellWaitTime;
        }

        public static float GetMovementSpeed()
        {
            return m_MovementSpeed;
        }

        public static List<BattleCell> GetPath(BattlePathInfo InPathInfo)
        {
            List<BattleCell> outPath = new List<BattleCell>();

            if (InPathInfo.StartCell == null || InPathInfo.TargetCell == null)
            {
                Debug.Log("[ProjectCI]::BattlePathManager::GetPath) Invalid Start, or Target");
                return outPath;
            }

            List<BattlePathFindingNode> OpenSet = new List<BattlePathFindingNode>();
            List<BattlePathFindingNode> ClosedSet = new List<BattlePathFindingNode>();
            BattlePathFindingNode ParentNode = null;

            OpenSet.Add(AStarCalculatePathNode(ParentNode, InPathInfo.StartCell, InPathInfo.StartCell, InPathInfo.TargetCell, InPathInfo));

            while (OpenSet.Count > 0)
            {
                BattlePathFindingNode CurrNode = AStarGetLowestFScore(OpenSet);

                if (CurrNode.Cell == InPathInfo.TargetCell)
                {
                    //Found node
                    BattlePathFindingNode reverseNode = CurrNode;
                    while (reverseNode != null)
                    {
                        outPath.Add(reverseNode.Cell);
                        reverseNode = reverseNode.Parent;
                    }

                    outPath.Reverse();
                    return outPath;
                }

                OpenSet.Remove(CurrNode);
                ClosedSet.Add(CurrNode);

                List<BattleCell> AdjCells = CurrNode.Cell.GetAllAdjacentCells();
                foreach (var cell in AdjCells)
                {
                    BattlePathFindingNode NewPathFindNode = AStarCalculatePathNode(CurrNode, cell, InPathInfo.StartCell, InPathInfo.TargetCell, InPathInfo);

                    if (ClosedSet.Contains(NewPathFindNode))
                    {
                        continue;
                    }

                    if(NewPathFindNode.Cell.IsBlocked() && !InPathInfo.bAllowBlocked)
                    {
                        continue;
                    }

                    if (NewPathFindNode.Cell.IsObjectOnCell() && InPathInfo.bIgnoreUnits && NewPathFindNode.Cell != InPathInfo.TargetCell)
                    {
                        continue;
                    }

                    if(InPathInfo.AllowedCells != null)
                    {
                        if(!InPathInfo.AllowedCells.Contains(NewPathFindNode.Cell))
                        {
                            continue;
                        }
                    }

                    if (!OpenSet.Contains(NewPathFindNode))
                    {
                        OpenSet.Add(NewPathFindNode);
                    }
                    else
                    {
                        int tenativeGScore = AStarCalculateG(cell, CurrNode, InPathInfo);

                        if (tenativeGScore < NewPathFindNode.G)
                        {
                            NewPathFindNode.Parent = CurrNode;
                            NewPathFindNode.G = tenativeGScore;
                            OpenSet = AStarUpdateList(OpenSet, NewPathFindNode);
                        }
                    }
                }

                ParentNode = CurrNode;
            }

            return outPath;
        }

        public static List<BattleCell> GetRadius(BattleRadiusInfo InRadiusInfo)
        {
            List<BattleCell> outPath = new List<BattleCell>();

            if (InRadiusInfo.StartCell == null)
            {
                Debug.Log("[ProjectCI]::BattlePathManager::GetRadius) Invalid Start");
                return outPath;
            }

            List<BattlePathFindingNode> OpenSet = new List<BattlePathFindingNode>();
            List<BattlePathFindingNode> ClosedSet = new List<BattlePathFindingNode>();

            BattlePathFindingNode NewNode = new BattlePathFindingNode(InRadiusInfo.StartCell, null);

            OpenSet.Add(NewNode);

            while (OpenSet.Count > 0)
            {
                BattlePathFindingNode CurrNode = DijGetLowestGScore(OpenSet);

                if (CurrNode.G == InRadiusInfo.Radius + 1)
                {
                    foreach (var open in OpenSet)
                    {
                        if (open.Cell != InRadiusInfo.StartCell)
                        {
                            outPath.Add(open.Cell);
                        }
                    }

                    foreach (var closed in ClosedSet)
                    {
                        if (closed.Cell != InRadiusInfo.StartCell)
                        {
                            outPath.Add(closed.Cell);
                        }
                    }

                    break;
                }

                OpenSet.Remove(CurrNode);
                ClosedSet.Add(CurrNode);

                List<BattleCell> AdjCells = CurrNode.Cell.GetAllAdjacentCells();
                foreach (var cell in AdjCells)
                {
                    BattlePathFindingNode NewPathFindNode = new BattlePathFindingNode(cell, CurrNode);

                    if (ClosedSet.Contains(NewPathFindNode))
                    {
                        continue;
                    }

                    if(InRadiusInfo.bStopAtBlockedCell)
                    {
                        if(!AllowCellInRadius(NewPathFindNode.Cell, InRadiusInfo))
                        {
                            continue;
                        }
                    }

                    if (!OpenSet.Contains(NewPathFindNode))
                    {
                        OpenSet.Add(NewPathFindNode);
                    }
                    else
                    {
                        int tenativeGScore = DijCalculateG(cell, CurrNode);

                        if (tenativeGScore < NewPathFindNode.G)
                        {
                            NewPathFindNode.Parent = CurrNode;
                            NewPathFindNode.G = tenativeGScore;
                            OpenSet = DijUpdateList(OpenSet, NewPathFindNode);
                        }
                    }
                }
            }

            if(!InRadiusInfo.bStopAtBlockedCell)
            {
                List<BattleCell> CellsToRemove = new List<BattleCell>();
                foreach (BattleCell cell in outPath)
                {
                    if(!AllowCellInRadius(cell, InRadiusInfo))
                    {
                        CellsToRemove.Add(cell);
                    }
                }

                foreach (BattleCell cellToRemove in CellsToRemove)
                {
                    outPath.Remove(cellToRemove);
                }
            }
            else
            {
                foreach (var closed in ClosedSet)
                {
                    if (closed.Cell != InRadiusInfo.StartCell)
                    {
                        outPath.Add(closed.Cell);
                    }
                }
            }

            return outPath;
        }

        static bool AllowCellInRadius(BattleCell InCell, BattleRadiusInfo InRadiusInfo)
        {
            if (!InCell)
            {
                return false;
            }

            if (InCell.IsBlocked() && !InRadiusInfo.bAllowBlocked)
            {
                return false;
            }

            if (InCell.IsObjectOnCell())
            {
                if (InRadiusInfo.EffectedTeam == BattleTeam.None)
                {
                    return false;
                }

                BattleTeam cellTeam = InCell.GetCellTeam();
                if (cellTeam != InRadiusInfo.EffectedTeam)
                {
                    return false;
                }
            }

            return true;
        }

        #region AStarComponents

        static BattlePathFindingNode AStarCalculatePathNode(BattlePathFindingNode InParent, BattleCell InCurrent, BattleCell InStart, BattleCell InTarget, BattlePathInfo InPathInfo)
        {
            int gCost = AStarCalculateG(InCurrent, InParent, InPathInfo);
            int hCost = AStarDistance(InCurrent, InTarget);

            return new BattlePathFindingNode(InCurrent, InParent, gCost, hCost);
        }

        static int AStarCalculateG(BattleCell InCurrent, BattlePathFindingNode InParent, BattlePathInfo InPathInfo)
        {
            int weight = 0;
            if(InPathInfo.bTakeWeightIntoAccount)
            {
                weight = InCurrent.GetWeightInfo().Weight;
            }

            return 1 + (InParent != null ? InParent.G : 0) + weight;
        }

        static int AStarDistance(BattleCell InStart, BattleCell InDest)
        {
            return (int)(InStart.GetIndex() - InDest.GetIndex()).SqrMagnitude();
        }

        static BattlePathFindingNode AStarGetLowestFScore(List<BattlePathFindingNode> InSet)
        {
            BattlePathFindingNode LowestF = null;
            foreach (BattlePathFindingNode CurrItem in InSet)
            {
                if(LowestF == null)
                {
                    LowestF = CurrItem;
                    continue;
                }

                if(CurrItem.GetFScore() < LowestF.GetFScore())
                {
                    LowestF = CurrItem;
                }
            }

            return LowestF;
        }

        static List<BattlePathFindingNode> AStarUpdateList(List<BattlePathFindingNode> InSet, BattlePathFindingNode InReplaceNode)
        {
            List<BattlePathFindingNode> UpdatedSet = InSet;

            int Index = UpdatedSet.IndexOf(InReplaceNode);
            if(Index != -1)
            {
                UpdatedSet[Index] = InReplaceNode;
            }

            return UpdatedSet;
        }

        #endregion

        #region DijkstraRadiusComponents

        static int DijCalculateG(BattleCell InCurrent, BattlePathFindingNode InParent)
        {
            return 1 + (InParent != null ? InParent.G : 0);
        }

        static List<BattlePathFindingNode> DijUpdateList(List<BattlePathFindingNode> InSet, BattlePathFindingNode InReplaceNode)
        {
            List<BattlePathFindingNode> UpdatedSet = InSet;

            int Index = UpdatedSet.IndexOf(InReplaceNode);
            if (Index != -1)
            {
                UpdatedSet[Index] = InReplaceNode;
            }

            return UpdatedSet;
        }

        static BattlePathFindingNode DijGetLowestGScore(List<BattlePathFindingNode> InSet)
        {
            BattlePathFindingNode LowestG = null;
            foreach (BattlePathFindingNode CurrItem in InSet)
            {
                if (LowestG == null)
                {
                    LowestG = CurrItem;
                    continue;
                }

                if (CurrItem.G < LowestG.G)
                {
                    LowestG = CurrItem;
                }
            }

            return LowestG;
        }

        #endregion
    }
} 