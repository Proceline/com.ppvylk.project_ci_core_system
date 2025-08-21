using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids
{
    public class SquarePresetGrid : LevelGridBase
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
                var finalY = y * cellSize.z;

                for (int x = 0; x < gridSize.x; x++)
                {
                    var finalX = x * cellSize.x;
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
                var adjacentVects = GetRelativeIndicesMap(InCell);
                foreach (var currAdjVect in adjacentVects)
                {
                    LevelCellBase adjContender = this[currAdjVect.Value];
                    if (adjContender)
                    {
                        InCell.AddAdjacentCell(currAdjVect.Key, adjContender);
                    }
                }
            }
        }

        protected override Vector2Int GetIndex(Vector2Int InOriginalIndex, CompassDir InDirection)
            => InOriginalIndex + GetOffsetFromDirection(InDirection);
        

        protected override Vector2 GetPosition(Vector2Int originalIndex, CompassDir dir)
        {
            LevelCellBase originalCell = this[originalIndex];

            if (!originalCell) 
                throw new NullReferenceException($"ERROR: Cell of {originalIndex.ToString()} NOT FOUND!");
            
            Vector3 bounds = CellObjCursor.GetComponent<Renderer>().bounds.size;
            float finalY;
            float finalX;
            
            switch (dir)
            {
                case CompassDir.N:
                    finalX = originalCell.transform.position.x;
                    finalY = originalCell.transform.position.z + bounds.z;
                    break;
                case CompassDir.NE:
                    finalX = originalCell.transform.position.x + bounds.x;
                    finalY = originalCell.transform.position.z + bounds.z;
                    break;
                case CompassDir.E:
                    finalX = originalCell.transform.position.x + bounds.x;
                    finalY = originalCell.transform.position.z;
                    break;
                case CompassDir.SE:
                    finalX = originalCell.transform.position.x + bounds.x;
                    finalY = originalCell.transform.position.z - bounds.z;
                    break;
                case CompassDir.S:
                    finalX = originalCell.transform.position.x;
                    finalY = originalCell.transform.position.z - bounds.z;
                    break;
                case CompassDir.SW:
                    finalX = originalCell.transform.position.x - bounds.x;
                    finalY = originalCell.transform.position.z - bounds.z;
                    break;
                case CompassDir.W:
                    finalX = originalCell.transform.position.x - bounds.x;
                    finalY = originalCell.transform.position.z;
                    break;
                case CompassDir.NW:
                    finalX = originalCell.transform.position.x - bounds.x;
                    finalY = originalCell.transform.position.z + bounds.z;
                    break;
                default:
                    throw new NullReferenceException("ERROR: No such Direction in SquareGrid!");
            }

            return new Vector2(finalX, finalY);
        }

        protected override Vector2Int GetOffsetFromDirection(CompassDir dir)
        {
            var offset = dir switch
            {
                CompassDir.N => new Vector2Int(0, -1),
                CompassDir.NE => new Vector2Int(1, -1),
                CompassDir.E => new Vector2Int(1, 0),
                CompassDir.SE => new Vector2Int(1, 1),
                CompassDir.S => new Vector2Int(0, 1),
                CompassDir.SW => new Vector2Int(-1, 1),
                CompassDir.W => new Vector2Int(-1, 0),
                CompassDir.NW => new Vector2Int(-1, -1),
                _ => throw new NullReferenceException("ERROR: No such Direction in SquareGrid!")
            };

            return offset;
        }

        protected override Dictionary<CompassDir, Vector2Int> GetRelativeIndicesMap(LevelCellBase InCell)
        {
            Dictionary<CompassDir, Vector2Int> adjacentIndices = new();

            if (!InCell) return adjacentIndices;
            Vector2Int cellIndex = InCell.GetIndex();

            adjacentIndices.Add(CompassDir.N, cellIndex + new Vector2Int(0, -1));
            adjacentIndices.Add(CompassDir.S, cellIndex + new Vector2Int(0, 1));
            adjacentIndices.Add(CompassDir.W, cellIndex + new Vector2Int(-1, 0));
            adjacentIndices.Add(CompassDir.E, cellIndex + new Vector2Int(1, 0));

            return adjacentIndices;
        }

    }
}
