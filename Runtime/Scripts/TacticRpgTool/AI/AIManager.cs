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
        public ILevelCell StartCell;
        public ILevelCell TargetCell;
        public bool bIgnoreUnits;
        public bool bAllowBlocked;
        public bool bTakeWeightIntoAccount;
        public List<ILevelCell> AllowedCells;

        public AIPathInfo(ILevelCell InStart, ILevelCell InTarget)
        {
            StartCell = InStart;
            TargetCell = InTarget;
            bIgnoreUnits = true;
            AllowedCells = null;
            bAllowBlocked = false;
            bTakeWeightIntoAccount = false;
        }
    }


    public struct AIRadiusInfo
    {
        public ILevelCell StartCell;
        public int Radius;
        public GridObject Caster;
        public bool bAllowBlocked;
        public bool bStopAtBlockedCell;
        public GameTeam EffectedTeam;

        public AIRadiusInfo(ILevelCell InStart, int InRadius)
        {
            StartCell = InStart;
            Radius = InRadius;
            Caster = null;
            bAllowBlocked = true;
            bStopAtBlockedCell = true;
            EffectedTeam = GameTeam.All;
        }
    }

    public class AIManager : MonoBehaviour
    {
        [SerializeField] private List<UnitAIComponent> m_AIUnits;

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

        public static List<ILevelCell> GetPath(AIPathInfo InPathInfo)
        {
            List<ILevelCell> outPath = new List<ILevelCell>();

            if (InPathInfo.StartCell == null || InPathInfo.TargetCell == null)
            {
                Debug.Log("([TurnBasedTools]::AIManager::GetPath) Invalid Start, or Target");
                return outPath;
            }

            List<PathFindingNode> OpenSet = new List<PathFindingNode>();
            List<PathFindingNode> ClosedSet = new List<PathFindingNode>();
            PathFindingNode ParentNode = null;

            OpenSet.Add(AStarCalculatePathNode(ParentNode, InPathInfo.StartCell, InPathInfo.StartCell, InPathInfo.TargetCell, InPathInfo));

            while (OpenSet.Count > 0)
            {
                PathFindingNode CurrNode = AStarGetLowestFScore(OpenSet);

                if (CurrNode.Cell == InPathInfo.TargetCell)
                {
                    //Found node
                    PathFindingNode reverseNode = CurrNode;
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

                List<ILevelCell> AdjCells = CurrNode.Cell.GetAllAdjacentCells();
                foreach (var cell in AdjCells)
                {
                    PathFindingNode NewPathFindNode = AStarCalculatePathNode(CurrNode, cell, InPathInfo.StartCell, InPathInfo.TargetCell, InPathInfo);

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

        public static List<ILevelCell> GetRadius(AIRadiusInfo InRadiusInfo)
        {
            List<ILevelCell> outPath = new List<ILevelCell>();

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

                List<ILevelCell> AdjCells = CurrNode.Cell.GetAllAdjacentCells();
                foreach (var cell in AdjCells)
                {
                    PathFindingNode NewPathFindNode = new PathFindingNode(cell, CurrNode);

                    if (ClosedSet.Contains(NewPathFindNode))
                    {
                        continue;
                    }

                    if(InRadiusInfo.bStopAtBlockedCell)
                    {
                        if( !AllowCellInRadius(NewPathFindNode.Cell, InRadiusInfo) )
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
                List<ILevelCell> CellsToRemove = new List<ILevelCell>();
                foreach (ILevelCell cell in outPath)
                {
                    if( !AllowCellInRadius(cell, InRadiusInfo) )
                    {
                        CellsToRemove.Add(cell);
                    }
                }

                foreach (ILevelCell cellToRemove in CellsToRemove)
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

        static bool AllowCellInRadius(ILevelCell InCell, AIRadiusInfo InRadiusInfo)
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
            if (gridObj)
            {
                if (InRadiusInfo.EffectedTeam == GameTeam.None)
                {
                    return false;
                }

                if(InRadiusInfo.Caster != null)
                {
                    GameTeam ObjAffinity = GameManager.GetTeamAffinity(gridObj.GetTeam(), InRadiusInfo.Caster.GetTeam());
                    if (ObjAffinity == GameTeam.Friendly && InRadiusInfo.EffectedTeam == GameTeam.Hostile)
                    {
                        return false;
                    }

                    if (ObjAffinity == GameTeam.Hostile && InRadiusInfo.EffectedTeam == GameTeam.Friendly)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #region UnitAI

        public static void RunAI(List<GridUnit> InAIUnits, UnityAction OnComplete)
        {
            UnityEvent OnAIComplete = new UnityEvent();
            OnAIComplete.AddListener(OnComplete);

            GameManager.Get().StartCoroutine(InternalRunAI(InAIUnits, OnAIComplete));
        }

        static IEnumerator InternalRunAI(List<GridUnit> InAIUnits, UnityEvent OnComplete)
        {
            foreach (GridUnit AIUnit in InAIUnits)
            {
                if (AIUnit && !AIUnit.IsDead())
                {
                    UnitAIComponent AIComponent = AIUnit.GetComponent<UnitAIComponent>();
                    if(AIComponent)
                    {
                        IEnumerator RunOnUnitEnum = AIComponent.GetAIData().RunOnUnit(AIUnit);
                        yield return GameManager.Get().StartCoroutine(RunOnUnitEnum);
                    }
                }
            }

            OnComplete.Invoke();
        }
        
        public static IEnumerator ExecuteAbility(GridUnit InCaster, ILevelCell InTarget, UnitAbility InAbility)
        {
            if(InCaster)
            {
                InCaster.ExecuteAbility(InAbility, InTarget);
                yield return new WaitForSeconds(InAbility.CalculateAbilityTime(InCaster));
            }
        }

        #endregion

        #region AStarComponents

        static PathFindingNode AStarCalculatePathNode(PathFindingNode InParent, ILevelCell InCurrent, ILevelCell InStart, ILevelCell InTarget, AIPathInfo InPathInfo)
        {
            int gCost = AStarCalculateG(InCurrent, InParent, InPathInfo);
            int hCost = AStarDistance(InCurrent, InTarget);

            return new PathFindingNode(InCurrent, InParent, gCost, hCost);
        }
        static int AStarCalculateG(ILevelCell InCurrent, PathFindingNode InParent, AIPathInfo InPathInfo)
        {
            int weight = 0;
            if(InPathInfo.bTakeWeightIntoAccount)
            {
                weight = InCurrent.GetWeightInfo().Weight;
            }

            return 1 + (InParent != null ? InParent.G : 0) + weight;
        }
        static int AStarDistance(ILevelCell InStart, ILevelCell InDest)
        {
            return (int)(InStart.GetIndex() - InDest.GetIndex()).SqrMagnitude();
        }
        static PathFindingNode AStarGetLowestFScore(List<PathFindingNode> InSet)
        {
            PathFindingNode LowestF = null;
            foreach (PathFindingNode CurrItem in InSet)
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
        static List<PathFindingNode> AStarUpdateList(List<PathFindingNode> InSet, PathFindingNode InReplaceNode)
        {
            List<PathFindingNode> UpdatedSet = InSet;

            int Index = UpdatedSet.IndexOf(InReplaceNode);
            if(Index != -1)
            {
                UpdatedSet[Index] = InReplaceNode;
            }

            return UpdatedSet;
        }

        #endregion

        #region DijkstraRadiusComponents

        static int DijCalculateG(ILevelCell InCurrent, PathFindingNode InParent)
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

        public void AddAIUnit(UnitAIComponent InAIUnit)
        {
            if(InAIUnit != null)
            {
                m_AIUnits.Add(InAIUnit);
            }
        }

        public void RemoveAIUnit(UnitAIComponent InAIUnit)
        {
            if(InAIUnit != null)
            {
                m_AIUnits.Remove(InAIUnit);
            }
        }

        public List<UnitAIComponent> GetAIUnits()
        {
            return m_AIUnits;
        }
    }
}
