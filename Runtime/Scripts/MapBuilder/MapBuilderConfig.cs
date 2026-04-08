using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.MapBuilder
{
    public enum MapGridType
    {
        Square,
        Hexagon
    }

    [Serializable]
    public struct MapCellEntry
    {
        public Vector2Int index;
        public int groundTileIndex;  // -1 = no tile placed
        public int decoObjectIndex;  // -1 = no deco placed
    }

    [CreateAssetMenu(
        fileName = "MapBuilderConfig",
        menuName = "ProjectCI/MapBuilder/Create MapBuilderConfig")]
    public class MapBuilderConfig : ScriptableObject
    {
        [Header("Grid Settings")]
        [SerializeField]
        private MapGridType gridType = MapGridType.Square;

        [SerializeField]
        private Vector2Int gridSize = new Vector2Int(10, 10);

        [SerializeField]
        [Min(0.1f)]
        private float cellSize = 1.0f;

        [Header("Palette")]
        [SerializeField]
        private MapPalette palette;

        [Header("Cell Data (auto-managed)")]
        [SerializeField]
        private List<MapCellEntry> cellDataMap = new List<MapCellEntry>();

        // ── Accessors ──────────────────────────────────────────────

        public MapGridType GridType => gridType;
        public Vector2Int GridSize => gridSize;
        public float CellSize => cellSize;
        public MapPalette Palette => palette;
        public List<MapCellEntry> CellDataMap => cellDataMap;

        // ── Cell Data Helpers ───────────────────────────────────────

        /// <summary>Returns the entry for the given cell index, or null if not found.</summary>
        public MapCellEntry? GetEntry(Vector2Int index)
        {
            for (int i = 0; i < cellDataMap.Count; i++)
            {
                if (cellDataMap[i].index == index)
                    return cellDataMap[i];
            }
            return null;
        }

        /// <summary>Upserts a cell entry by index.</summary>
        public void SetEntry(MapCellEntry entry)
        {
            for (int i = 0; i < cellDataMap.Count; i++)
            {
                if (cellDataMap[i].index == entry.index)
                {
                    cellDataMap[i] = entry;
                    return;
                }
            }
            cellDataMap.Add(entry);
        }

        /// <summary>Removes the cell entry for the given index if it exists.</summary>
        public void RemoveEntry(Vector2Int index)
        {
            cellDataMap.RemoveAll(e => e.index == index);
        }

        /// <summary>Clears all cell data.</summary>
        public void ClearAllEntries()
        {
            cellDataMap.Clear();
        }

        /// <summary>Calculates the world-space position for a given grid index.</summary>
        public Vector3 IndexToWorldPosition(Vector2Int index)
        {
            float x = index.x * cellSize;
            float z = -index.y * cellSize;
            return new Vector3(x, 0f, z);
        }

        /// <summary>Converts a world-space position back to the nearest grid index.</summary>
        public Vector2Int WorldPositionToIndex(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / cellSize);
            int y = Mathf.RoundToInt(-worldPos.z / cellSize);
            return new Vector2Int(x, y);
        }

        /// <summary>Returns true if the given index is within the configured grid bounds.</summary>
        public bool IsIndexInBounds(Vector2Int index)
        {
            return index.x >= 0 && index.x < gridSize.x &&
                   index.y >= 0 && index.y < gridSize.y;
        }
    }
}
