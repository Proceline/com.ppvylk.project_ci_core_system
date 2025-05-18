using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Enums;
using ProjectCI.CoreSystem.Runtime.Levels.Data;

namespace ProjectCI.CoreSystem.Runtime.Levels.Components
{
    /// <summary>
    /// Standard implementation of BattleGrid with hexagonal grid layout
    /// </summary>
    public class BattleStandardGrid : BattleGrid
    {
        public override BattleCell AddLevelCellToObject(GameObject InObj)
        {
            if (!InObj)
            {
                return null;
            }

            return InObj.AddComponent<BattleLevelCell>();
        }

        protected override void SetupAdjacencies(BattleCell InCell)
        {
            if (InCell)
            {
                Vector2 cellIndex = InCell.GetIndex();
                bool bIsOdd = cellIndex.y % 2 != 0;

                var adjacentVects = GetRelativeIndicesMap(InCell);
                foreach (var currAdjVect in adjacentVects)
                {
                    Vector2 val = currAdjVect.Value;
                    if (bIsOdd && currAdjVect.Key != GridDirection.East && currAdjVect.Key != GridDirection.West)
                    {
                        val -= new Vector2(1, 0);
                    }

                    BattleCell adjContender = this[val];
                    if (adjContender)
                    {
                        InCell.AddAdjacentCell(currAdjVect.Key, adjContender);
                    }
                }
            }
        }

        protected override Vector2 GetIndex(Vector2 InOriginalIndex, GridDirection InDirection)
        {
            Vector2 index = InOriginalIndex + GetOffsetFromDirection(InDirection);
            if (index.y % 2 == 0 && InDirection != GridDirection.East && InDirection != GridDirection.West)
            {
                index.x -= 1;
            }
            return index;
        }

        protected override Vector2 GetPosition(Vector2 OriginalIndex, GridDirection dir)
        {
            BattleCell OriginalCell = this[OriginalIndex];

            Vector3 bounds = m_CellObjCursor.GetComponent<Renderer>().bounds.size;

            float finalY = 0.0f;
            float finalX = 0.0f;

            if (OriginalCell)
            {
                switch (dir)
                {
                    case GridDirection.NorthEast:
                        finalX = OriginalCell.transform.position.x + bounds.x * 0.5f;
                        finalY = OriginalCell.transform.position.z + bounds.z * 0.75f;
                        break;
                    case GridDirection.East:
                        finalX = OriginalCell.transform.position.x + bounds.x;
                        finalY = OriginalCell.transform.position.z;
                        break;
                    case GridDirection.SouthEast:
                        finalX = OriginalCell.transform.position.x + bounds.x * 0.5f;
                        finalY = OriginalCell.transform.position.z - bounds.z * 0.75f;
                        break;
                    case GridDirection.SouthWest:
                        finalX = OriginalCell.transform.position.x - bounds.x * 0.5f;
                        finalY = OriginalCell.transform.position.z - bounds.z * 0.75f;
                        break;
                    case GridDirection.West:
                        finalX = OriginalCell.transform.position.x - bounds.x;
                        finalY = OriginalCell.transform.position.z;
                        break;
                    case GridDirection.NorthWest:
                        finalX = OriginalCell.transform.position.x - bounds.x * 0.5f;
                        finalY = OriginalCell.transform.position.z + bounds.z * 0.75f;
                        break;
                    default:
                        break;
                }
            }

            return new Vector2(finalX, finalY);
        }

        protected override Vector2 GetOffsetFromDirection(GridDirection dir)
        {
            Vector2 Offset = new Vector2();

            switch (dir)
            {
                case GridDirection.NorthEast:
                    Offset = new Vector2(1, -1);
                    break;
                case GridDirection.East:
                    Offset = new Vector2(1, 0);
                    break;
                case GridDirection.SouthEast:
                    Offset = new Vector2(1, 1);
                    break;
                case GridDirection.SouthWest:
                    Offset = new Vector2(0, 1);
                    break;
                case GridDirection.West:
                    Offset = new Vector2(-1, 0);
                    break;
                case GridDirection.NorthWest:
                    Offset = new Vector2(0, -1);
                    break;
                default:
                    break;
            }

            return Offset;
        }

        protected override Dictionary<GridDirection, Vector2> GetRelativeIndicesMap(BattleCell InCell)
        {
            Dictionary<GridDirection, Vector2> AdjacentVects = new Dictionary<GridDirection, Vector2>();

            if (InCell)
            {
                Vector2 cellIndex = InCell.GetIndex();

                AdjacentVects.Add(GridDirection.NorthEast, cellIndex + new Vector2(1, -1));
                AdjacentVects.Add(GridDirection.East, cellIndex + new Vector2(1, 0));
                AdjacentVects.Add(GridDirection.SouthEast, cellIndex + new Vector2(1, 1));
                AdjacentVects.Add(GridDirection.SouthWest, cellIndex + new Vector2(0, 1));
                AdjacentVects.Add(GridDirection.West, cellIndex + new Vector2(-1, 0));
                AdjacentVects.Add(GridDirection.NorthWest, cellIndex + new Vector2(0, -1));
            }

            return AdjacentVects;
        }
    }
} 