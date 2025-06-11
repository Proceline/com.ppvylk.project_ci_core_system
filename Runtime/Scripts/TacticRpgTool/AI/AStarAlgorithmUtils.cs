using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI
{
    public struct AIPathInfo
    {
        public LevelCellBase StartCell;
        public LevelCellBase TargetCell;
        public bool bNoDestinationUnits;
        public bool bIgnoreUnitsOnPath;
        public bool bAllowBlocked;
        public bool bTakeWeightIntoAccount;
        public List<LevelCellBase> AllowedCells;

        public AIPathInfo(LevelCellBase InStart, LevelCellBase InTarget)
        {
            StartCell = InStart;
            TargetCell = InTarget;
            bNoDestinationUnits = true;
            bIgnoreUnitsOnPath = true;
            AllowedCells = null;
            bAllowBlocked = false;
            bTakeWeightIntoAccount = false;
        }
    }

    public struct AIRadiusInfo
    {
        public LevelCellBase StartCell;
        public int Radius;
        public GridObject Caster;
        public bool bAllowBlocked;
        public bool bStopAtBlockedCell;
        public BattleTeam EffectedTeam;

        public AIRadiusInfo(LevelCellBase InStart, int InRadius)
        {
            StartCell = InStart;
            Radius = InRadius;
            Caster = null;
            bAllowBlocked = true;
            bStopAtBlockedCell = true;
            EffectedTeam = BattleTeam.All;
        }
    }

    public class AStarAlgorithmUtils : Object
    {
        static float m_CellWaitTime = 0.0f;
        public static float m_MovementSpeed = 3.0f;

        public static float GetWaitTime()
        {
            return m_CellWaitTime;
        }

        public static float GetMovementSpeed()
        {
            return m_MovementSpeed;
        }

        public static List<LevelCellBase> GetPath(AIPathInfo InPathInfo)
        {
            List<LevelCellBase> outPath = new List<LevelCellBase>();

            if (InPathInfo.StartCell == null || InPathInfo.TargetCell == null)
            {
                Debug.Log("([TurnBasedTools]::AIManager::GetPath) Invalid Start, or Target");
                return outPath;
            }

            List<PathFindingNode> openSet = new List<PathFindingNode>();
            List<PathFindingNode> closedSet = new List<PathFindingNode>();

            openSet.Add(AStarCalculatePathNode(null, InPathInfo.StartCell, InPathInfo.StartCell, InPathInfo.TargetCell, InPathInfo));

            while (openSet.Count > 0)
            {
                PathFindingNode currNode = AStarGetLowestFScore(openSet);

                if (currNode.Cell == InPathInfo.TargetCell)
                {
                    // Found node
                    PathFindingNode reverseNode = currNode;
                    while (reverseNode != null)
                    {
                        outPath.Add(reverseNode.Cell);
                        reverseNode = reverseNode.Parent;
                    }

                    outPath.Reverse();
                    return outPath;
                }

                openSet.Remove(currNode);
                closedSet.Add(currNode);

                List<LevelCellBase> adjCells = currNode.Cell.GetAllAdjacentCells();
                foreach (var cell in adjCells)
                {
                    PathFindingNode newPathFindNode = AStarCalculatePathNode(currNode, cell, InPathInfo.StartCell, InPathInfo.TargetCell, InPathInfo);

                    if (closedSet.Contains(newPathFindNode))
                    {
                        continue;
                    }

                    if (newPathFindNode.Cell.IsBlocked() && !InPathInfo.bAllowBlocked)
                    {
                        continue;
                    }

                    if (newPathFindNode.Cell.IsObjectOnCell())
                    {
                        if (InPathInfo.bNoDestinationUnits && newPathFindNode.Cell == InPathInfo.TargetCell)
                        {
                            continue;
                        }
                        if (!InPathInfo.bIgnoreUnitsOnPath && newPathFindNode.Cell != InPathInfo.TargetCell)
                        {
                            continue;
                        }
                    }

                    if (InPathInfo.AllowedCells != null)
                    {
                        if (!InPathInfo.AllowedCells.Contains(newPathFindNode.Cell))
                        {
                            continue;
                        }
                    }

                    if (!openSet.Contains(newPathFindNode))
                    {
                        openSet.Add(newPathFindNode);
                    }
                    else
                    {
                        int tenativeGScore = AStarCalculateG(cell, currNode, InPathInfo);

                        if (tenativeGScore < newPathFindNode.G)
                        {
                            newPathFindNode.Parent = currNode;
                            newPathFindNode.G = tenativeGScore;
                            openSet = AStarUpdateList(openSet, newPathFindNode);
                        }
                    }
                }
            }

            return outPath;
        }

        public static List<LevelCellBase> GetRadius(AIRadiusInfo InRadiusInfo)
        {
            List<LevelCellBase> outPath = new List<LevelCellBase>();

            if (InRadiusInfo.StartCell == null)
            {
                Debug.Log("([TurnBasedTools]::AIManager::GetRadius) Invalid Start, or Target");
                return outPath;
            }

            List<PathFindingNode> OpenSet = new List<PathFindingNode>();
            List<PathFindingNode> ClosedSet = new List<PathFindingNode>();

            PathFindingNode NewNode = new PathFindingNode(InRadiusInfo.StartCell, null);

            OpenSet.Add(NewNode);

            while (OpenSet.Count > 0)
            {
                PathFindingNode CurrNode = DijGetLowestGScore(OpenSet);

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

                List<LevelCellBase> adjCells = CurrNode.Cell.GetAllAdjacentCells();
                foreach (var cell in adjCells)
                {
                    PathFindingNode NewPathFindNode = new PathFindingNode(cell, CurrNode);

                    if (ClosedSet.Contains(NewPathFindNode))
                    {
                        continue;
                    }

                    if (InRadiusInfo.bStopAtBlockedCell)
                    {
                        if (!AllowCellInRadius(NewPathFindNode.Cell, InRadiusInfo))
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

            if (!InRadiusInfo.bStopAtBlockedCell)
            {
                List<LevelCellBase> CellsToRemove = new List<LevelCellBase>();
                foreach (LevelCellBase cell in outPath)
                {
                    if (!AllowCellInRadius(cell, InRadiusInfo))
                    {
                        CellsToRemove.Add(cell);
                    }
                }

                foreach (LevelCellBase cellToRemove in CellsToRemove)
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

        static bool AllowCellInRadius(LevelCellBase InCell, AIRadiusInfo InRadiusInfo)
        {
            if (!InCell)
            {
                return false;
            }

            if (InCell.IsBlocked() && !InRadiusInfo.bAllowBlocked)
            {
                return false;
            }

            GridObject gridObj = InCell.GetObjectOnCell();
            if (!gridObj)
            {
                return true;
            }
            
            if (InRadiusInfo.EffectedTeam == BattleTeam.None)
            {
                return false;
            }

            if (InRadiusInfo.Caster != null)
            {
                BattleTeam objAffinity = TacticBattleManager.GetTeamAffinity(gridObj.GetTeam(), InRadiusInfo.Caster.GetTeam());
                if (objAffinity == BattleTeam.Friendly && InRadiusInfo.EffectedTeam == BattleTeam.Hostile)
                {
                    return false;
                }

                if (objAffinity == BattleTeam.Hostile && InRadiusInfo.EffectedTeam == BattleTeam.Friendly)
                {
                    return false;
                }
            }

            return true;
        }

        #region UnitAI

        public static void RunAI(List<GridPawnUnit> InAIUnits, UnityAction OnComplete)
        {
            UnityEvent OnAIComplete = new UnityEvent();
            OnAIComplete.AddListener(OnComplete);

            TacticBattleManager.Get().StartCoroutine(InternalRunAI(InAIUnits, OnAIComplete));
        }

        static IEnumerator InternalRunAI(List<GridPawnUnit> InAIUnits, UnityEvent OnComplete)
        {
            foreach (GridPawnUnit AIUnit in InAIUnits)
            {
                if (AIUnit && !AIUnit.IsDead())
                {
                    UnitAIComponent AIComponent = AIUnit.GetComponent<UnitAIComponent>();
                    if (AIComponent)
                    {
                        IEnumerator RunOnUnitEnum = AIComponent.GetAIData().RunOnUnit(AIUnit);
                        yield return TacticBattleManager.Get().StartCoroutine(RunOnUnitEnum);
                    }
                }
            }

            OnComplete.Invoke();
        }

        #endregion

        #region AStarComponents

        private static PathFindingNode AStarCalculatePathNode(PathFindingNode InParent, LevelCellBase InCurrent,
            LevelCellBase InStart, LevelCellBase InTarget, AIPathInfo InPathInfo)
        {
            int gCost = AStarCalculateG(InCurrent, InParent, InPathInfo);
            int hCost = AStarDistance(InCurrent, InTarget);

            return new PathFindingNode(InCurrent, InParent, gCost, hCost);
        }

        static int AStarCalculateG(LevelCellBase InCurrent, PathFindingNode InParent, AIPathInfo InPathInfo)
        {
            int weight = 0;
            if (InPathInfo.bTakeWeightIntoAccount)
            {
                weight = InCurrent.GetWeightInfo().Weight;
            }

            return 1 + (InParent != null ? InParent.G : 0) + weight;
        }

        static int AStarDistance(LevelCellBase InStart, LevelCellBase InDest)
        {
            return (int)(InStart.GetIndex() - InDest.GetIndex()).SqrMagnitude();
        }

        static PathFindingNode AStarGetLowestFScore(List<PathFindingNode> InSet)
        {
            PathFindingNode LowestF = null;
            foreach (PathFindingNode CurrItem in InSet)
            {
                if (LowestF == null)
                {
                    LowestF = CurrItem;
                    continue;
                }

                if (CurrItem.GetFScore() < LowestF.GetFScore())
                {
                    LowestF = CurrItem;
                }
            }

            return LowestF;
        }

        static List<PathFindingNode> AStarUpdateList(List<PathFindingNode> InSet, PathFindingNode InReplaceNode)
        {
            List<PathFindingNode> UpdatedSet = InSet;

            int Index = UpdatedSet.IndexOf(InReplaceNode);
            if (Index != -1)
            {
                UpdatedSet[Index] = InReplaceNode;
            }

            return UpdatedSet;
        }

        #endregion

        #region DijkstraRadiusComponents

        static int DijCalculateG(LevelCellBase InCurrent, PathFindingNode InParent)
        {
            return 1 + (InParent != null ? InParent.G : 0);
        }

        static List<PathFindingNode> DijUpdateList(List<PathFindingNode> InSet, PathFindingNode InReplaceNode)
        {
            List<PathFindingNode> UpdatedSet = InSet;

            int Index = UpdatedSet.IndexOf(InReplaceNode);
            if (Index != -1)
            {
                UpdatedSet[Index] = InReplaceNode;
            }

            return UpdatedSet;
        }

        static PathFindingNode DijGetLowestGScore(List<PathFindingNode> InSet)
        {
            PathFindingNode LowestG = null;
            foreach (PathFindingNode CurrItem in InSet)
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
