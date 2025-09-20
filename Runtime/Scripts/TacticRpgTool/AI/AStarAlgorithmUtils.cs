using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using Object = UnityEngine.Object;

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

            openSet.Add(AStarCalculatePathNode(null, InPathInfo.StartCell, InPathInfo.TargetCell, InPathInfo));

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
                    PathFindingNode newPathFindNode = AStarCalculatePathNode(currNode, cell, InPathInfo.TargetCell, InPathInfo);

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
                        int gScore = AStarCalculateG(cell, currNode, InPathInfo);

                        if (gScore >= newPathFindNode.G)
                        {
                            continue;
                        }

                        newPathFindNode.Parent = currNode;
                        newPathFindNode.G = gScore;
                        openSet = AStarUpdateList(openSet, newPathFindNode);
                    }
                }
            }

            return outPath;
        }

        public static List<LevelCellBase> GetRadius(AIRadiusInfo inRadiusInfo)
        {
            List<LevelCellBase> outPath = new List<LevelCellBase>();

            if (!inRadiusInfo.StartCell)
            {
                throw new NullReferenceException(
                    $"([{nameof(AStarAlgorithmUtils)}]::AIManager::GetRadius) Invalid Start, or Target");
            }

            List<PathFindingNode> openSet = new();
            List<PathFindingNode> closedSet = new();

            var newNode = new PathFindingNode(inRadiusInfo.StartCell, null);

            openSet.Add(newNode);

            while (openSet.Count > 0)
            {
                PathFindingNode currNode = DijGetLowestGScore(openSet);

                if (currNode.G == inRadiusInfo.Radius + 1)
                {
                    foreach (var open in openSet)
                    {
                        if (open.Cell != inRadiusInfo.StartCell)
                        {
                            outPath.Add(open.Cell);
                        }
                    }

                    foreach (var closed in closedSet)
                    {
                        if (closed.Cell != inRadiusInfo.StartCell)
                        {
                            outPath.Add(closed.Cell);
                        }
                    }

                    break;
                }

                openSet.Remove(currNode);
                closedSet.Add(currNode);

                List<LevelCellBase> adjCells = currNode.Cell.GetAllAdjacentCells();
                foreach (var cell in adjCells)
                {
                    PathFindingNode newPathFindNode = new PathFindingNode(cell, currNode);

                    if (closedSet.Contains(newPathFindNode))
                    {
                        continue;
                    }

                    if (inRadiusInfo.bStopAtBlockedCell)
                    {
                        if (!AllowCellInRadius(newPathFindNode.Cell, inRadiusInfo))
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
                        int tenativeGScore = DijCalculateG(cell, currNode);

                        if (tenativeGScore < newPathFindNode.G)
                        {
                            newPathFindNode.Parent = currNode;
                            newPathFindNode.G = tenativeGScore;
                            openSet = DijUpdateList(openSet, newPathFindNode);
                        }
                    }
                }
            }

            if (!inRadiusInfo.bStopAtBlockedCell)
            {
                for (var delIndex = outPath.Count - 1; delIndex >= 0; delIndex--)
                {
                    if (!AllowCellInRadius(outPath[delIndex], inRadiusInfo))
                    {
                        outPath.RemoveAt(delIndex);
                    }
                }
            }
            else
            {
                foreach (var closed in closedSet)
                {
                    if (closed.Cell != inRadiusInfo.StartCell)
                    {
                        outPath.Add(closed.Cell);
                    }
                }
            }

            return outPath;
        }

        internal static bool AllowCellInRadius(LevelCellBase inCell, AIRadiusInfo inRadiusInfo)
        {
            if (!inCell)
            {
                return false;
            }

            if (inCell.IsBlocked() && !inRadiusInfo.bAllowBlocked)
            {
                return false;
            }

            GridObject gridObj = inCell.GetObjectOnCell();
            if (!gridObj)
            {
                return true;
            }
            
            if (inRadiusInfo.EffectedTeam == BattleTeam.None)
            {
                return false;
            }

            if (inRadiusInfo.Caster != null)
            {
                BattleTeam objAffinity = TacticBattleManager.GetTeamAffinity(gridObj.GetTeam(), inRadiusInfo.Caster.GetTeam());
                if (objAffinity == BattleTeam.Friendly && inRadiusInfo.EffectedTeam == BattleTeam.Hostile)
                {
                    return false;
                }

                if (objAffinity == BattleTeam.Hostile && inRadiusInfo.EffectedTeam == BattleTeam.Friendly)
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

        private static PathFindingNode AStarCalculatePathNode(PathFindingNode parent, LevelCellBase current,
            LevelCellBase target, AIPathInfo pathInfo)
        {
            var gCost = AStarCalculateG(current, parent, pathInfo);
            var hCost = AStarDistance(current, target);

            return new PathFindingNode(current, parent, gCost, hCost);
        }

        private static int AStarCalculateG(LevelCellBase current, PathFindingNode parent, AIPathInfo pathInfo)
        {
            var weight = 0;
            if (pathInfo.bTakeWeightIntoAccount)
            {
                weight = current.GetWeightInfo().weight;
            }

            return 1 + (parent?.G ?? 0) + weight;
        }

        private static int AStarDistance(LevelCellBase InStart, LevelCellBase InDest) =>
            (InStart.GetIndex() - InDest.GetIndex()).sqrMagnitude;

        private static PathFindingNode AStarGetLowestFScore(List<PathFindingNode> InSet)
        {
            PathFindingNode lowestF = null;
            foreach (PathFindingNode currentNode in InSet)
            {
                if (lowestF == null)
                {
                    lowestF = currentNode;
                    continue;
                }

                if (currentNode.GetFScore() < lowestF.GetFScore())
                {
                    lowestF = currentNode;
                }
            }

            return lowestF;
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
