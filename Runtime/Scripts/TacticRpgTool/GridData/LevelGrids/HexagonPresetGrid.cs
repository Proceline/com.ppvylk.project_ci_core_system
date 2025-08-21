using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids
{
    public class HexagonPresetGrid : LevelGridBase
    {
        public override Dictionary<Vector2Int, RaycastHit> ScanGridRayHits(
            Vector3 center,
            Vector3 cellSize,
            Vector2Int gridSize,
            float maxDistance = 100f,
            LayerMask layerMask = default)
        {
            Dictionary<Vector2Int, RaycastHit> hitResults = new Dictionary<Vector2Int, RaycastHit>();

            for (int y = 0; y < gridSize.y; y++)
            {
                float offset = 0;
                if (y % 2 == 0)
                {
                    offset = cellSize.x * 0.5f;
                }

                float finalY = y * cellSize.z * 0.75f;

                for (int x = 0; x < gridSize.x; x++)
                {
                    float finalX = x * cellSize.x + offset;
                    Vector3 worldPos = center + new Vector3(finalX, 0.0f, -finalY);

                    Ray ray = new Ray(worldPos, Vector3.down);
                    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
                    {
                        hitResults[new Vector2Int(x, y)] = hit;
                    }
                }
            }

            return hitResults;
        }
        
        protected override void SetupAllNeighbors(LevelCellBase InCell)
        {
            if (InCell)
            {
                Vector2 cellIndex = InCell.GetIndex();
                bool bIsOdd = cellIndex.y % 2 != 0;

                var adjacentVects = GetRelativeIndicesMap(InCell);
                foreach (var currAdjVect in adjacentVects)
                {
                    Vector2Int val = currAdjVect.Value;
                    if (bIsOdd && currAdjVect.Key != CompassDir.E && currAdjVect.Key != CompassDir.W)
                    {
                        val -= new Vector2Int(1, 0);
                    }

                    LevelCellBase adjContender = this[val];
                    if (adjContender)
                    {
                        InCell.AddAdjacentCell(currAdjVect.Key, adjContender);
                    }
                }
            }
        }

        protected override Vector2Int GetIndex(Vector2Int InOriginalIndex, CompassDir InDirection)
        {
            Vector2Int index = InOriginalIndex + GetOffsetFromDirection(InDirection);
            if (index.y % 2 == 0 && InDirection != CompassDir.E && InDirection != CompassDir.W)
            {
                index.x -= 1;
            }
            return index;
        }

        protected override Vector2 GetPosition(Vector2Int OriginalIndex, CompassDir dir)
        {
            LevelCellBase OriginalCell = this[OriginalIndex];

            Vector3 bounds = CellObjCursor.GetComponent<Renderer>().bounds.size;

            float finalY = 0.0f;
            float finalX = 0.0f;

            if (OriginalCell)
            {
                switch (dir)
                {
                    case CompassDir.NE:
                        finalX = OriginalCell.transform.position.x + bounds.x * 0.5f;
                        finalY = OriginalCell.transform.position.z + bounds.z * 0.75f;
                        break;
                    case CompassDir.E:
                        finalX = OriginalCell.transform.position.x + bounds.x;
                        finalY = OriginalCell.transform.position.z;
                        break;
                    case CompassDir.SE:
                        finalX = OriginalCell.transform.position.x + bounds.x * 0.5f;
                        finalY = OriginalCell.transform.position.z - bounds.z * 0.75f;
                        break;
                    case CompassDir.SW:
                        finalX = OriginalCell.transform.position.x - bounds.x * 0.5f;
                        finalY = OriginalCell.transform.position.z - bounds.z * 0.75f;
                        break;
                    case CompassDir.W:
                        finalX = OriginalCell.transform.position.x - bounds.x;
                        finalY = OriginalCell.transform.position.z;
                        break;
                    case CompassDir.NW:
                        finalX = OriginalCell.transform.position.x - bounds.x * 0.5f;
                        finalY = OriginalCell.transform.position.z + bounds.z * 0.75f;
                        break;
                    default:
                        break;
                }
            }

            return new Vector2(finalX, finalY);
        }

        protected override Vector2Int GetOffsetFromDirection(CompassDir dir)
        {
            Vector2Int Offset = new Vector2Int();

            switch (dir)
            {
                case CompassDir.NE:
                    Offset = new Vector2Int(1, -1);
                    break;
                case CompassDir.E:
                    Offset = new Vector2Int(1, 0);
                    break;
                case CompassDir.SE:
                    Offset = new Vector2Int(1, 1);
                    break;
                case CompassDir.SW:
                    Offset = new Vector2Int(0, 1);
                    break;
                case CompassDir.W:
                    Offset = new Vector2Int(-1, 0);
                    break;
                case CompassDir.NW:
                    Offset = new Vector2Int(0, -1);
                    break;
            }

            return Offset;
        }

        protected override Dictionary<CompassDir, Vector2Int> GetRelativeIndicesMap(LevelCellBase InCell)
        {
            Dictionary<CompassDir, Vector2Int> AdjacentVects = new Dictionary<CompassDir, Vector2Int>();

            if (InCell)
            {
                Vector2Int cellIndex = InCell.GetIndex();

                AdjacentVects.Add(CompassDir.NE, cellIndex + new Vector2Int(1, -1));
                AdjacentVects.Add(CompassDir.E, cellIndex + new Vector2Int(1, 0));
                AdjacentVects.Add(CompassDir.SE, cellIndex + new Vector2Int(1, 1));
                AdjacentVects.Add(CompassDir.SW, cellIndex + new Vector2Int(0, 1));
                AdjacentVects.Add(CompassDir.W, cellIndex + new Vector2Int(-1, 0));
                AdjacentVects.Add(CompassDir.NW, cellIndex + new Vector2Int(0, -1));
            }

            return AdjacentVects;
        }

    }
}
