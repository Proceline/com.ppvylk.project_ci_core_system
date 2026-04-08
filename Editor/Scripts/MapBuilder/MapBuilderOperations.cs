using UnityEditor;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.MapBuilder;

namespace ProjectCI.CoreSystem.IEditor.MapBuilder
{
    /// <summary>
    /// Static utility class that performs all scene-level GameObject operations for the Map Builder.
    /// All methods are editor-only and fully Undo-aware.
    /// </summary>
    public static class MapBuilderOperations
    {
        // ── Grid Generation ─────────────────────────────────────────

        /// <summary>
        /// Generates the full ground tile grid based on the config.
        /// Each cell already recorded in cellDataMap is placed; missing cells get default tile index 0.
        /// </summary>
        public static void GenerateGrid(MapBuilderRoot root, MapBuilderConfig config)
        {
            if (!ValidateInputs(root, config)) return;
            if (config.Palette == null || config.Palette.GroundTiles.Count == 0)
            {
                Debug.LogWarning("[MapBuilder] No ground tiles in palette. Aborting grid generation.");
                return;
            }

            Undo.SetCurrentGroupName("Generate Map Grid");
            int group = Undo.GetCurrentGroup();

            for (int y = 0; y < config.GridSize.y; y++)
            {
                for (int x = 0; x < config.GridSize.x; x++)
                {
                    var index = new Vector2Int(x, y);
                    MapCellEntry? existing = config.GetEntry(index);
                    int tileIdx = existing.HasValue ? existing.Value.groundTileIndex : 0;
                    int decoIdx = existing.HasValue ? existing.Value.decoObjectIndex : -1;

                    PlaceCellObject(root, config, index, tileIdx);

                    if (decoIdx >= 0)
                    {
                        PlaceDecoObject(root, config, index, decoIdx);
                    }
                }
            }

            Undo.CollapseUndoOperations(group);
        }

        /// <summary>
        /// Clears all generated GameObjects under CellsRoot and DecoRoot, and resets cell data.
        /// </summary>
        public static void ClearAll(MapBuilderRoot root, MapBuilderConfig config)
        {
            if (!ValidateInputs(root, config)) return;

            Undo.SetCurrentGroupName("Clear Map");
            int group = Undo.GetCurrentGroup();

            ClearChildren(root.CellsRoot);
            ClearChildren(root.DecoRoot);

            Undo.RecordObject(config, "Clear Cell Data");
            config.ClearAllEntries();
            EditorUtility.SetDirty(config);

            Undo.CollapseUndoOperations(group);
        }

        // ── Brush Operations ────────────────────────────────────────

        /// <summary>
        /// Paints a ground tile on the given cell, replacing any existing one.
        /// </summary>
        public static void PaintGround(MapBuilderRoot root, MapBuilderConfig config,
            Vector2Int index, int groundTileIndex)
        {
            if (!ValidateInputs(root, config)) return;
            if (!config.IsIndexInBounds(index)) return;

            Undo.SetCurrentGroupName("Paint Ground");
            int group = Undo.GetCurrentGroup();

            // Remove old cell object if present
            RemoveCellObject(root, index);

            // Place new cell object
            PlaceCellObject(root, config, index, groundTileIndex);

            // Update config data
            Undo.RecordObject(config, "Paint Ground Data");
            MapCellEntry? existing = config.GetEntry(index);
            var entry = new MapCellEntry
            {
                index = index,
                groundTileIndex = groundTileIndex,
                decoObjectIndex = existing.HasValue ? existing.Value.decoObjectIndex : -1
            };
            config.SetEntry(entry);
            EditorUtility.SetDirty(config);

            Undo.CollapseUndoOperations(group);
        }

        /// <summary>
        /// Paints a decoration object on the given cell, replacing any existing one.
        /// </summary>
        public static void PaintDeco(MapBuilderRoot root, MapBuilderConfig config,
            Vector2Int index, int decoIndex)
        {
            if (!ValidateInputs(root, config)) return;
            if (!config.IsIndexInBounds(index)) return;

            Undo.SetCurrentGroupName("Paint Decoration");
            int group = Undo.GetCurrentGroup();

            // Remove old deco object if present
            RemoveDecoObject(root, index);

            // Place new deco object
            PlaceDecoObject(root, config, index, decoIndex);

            // Update config data
            Undo.RecordObject(config, "Paint Deco Data");
            MapCellEntry? existing = config.GetEntry(index);
            var entry = new MapCellEntry
            {
                index = index,
                groundTileIndex = existing.HasValue ? existing.Value.groundTileIndex : 0,
                decoObjectIndex = decoIndex
            };
            config.SetEntry(entry);
            EditorUtility.SetDirty(config);

            Undo.CollapseUndoOperations(group);
        }

        /// <summary>
        /// Erases both the ground tile and decoration from a cell.
        /// </summary>
        public static void EraseCell(MapBuilderRoot root, MapBuilderConfig config, Vector2Int index)
        {
            if (!ValidateInputs(root, config)) return;

            Undo.SetCurrentGroupName("Erase Cell");
            int group = Undo.GetCurrentGroup();

            RemoveCellObject(root, index);
            RemoveDecoObject(root, index);

            Undo.RecordObject(config, "Erase Cell Data");
            config.RemoveEntry(index);
            EditorUtility.SetDirty(config);

            Undo.CollapseUndoOperations(group);
        }

        /// <summary>
        /// Fills the entire grid with the given ground tile index, leaving existing decos intact.
        /// </summary>
        public static void FillAll(MapBuilderRoot root, MapBuilderConfig config, int groundTileIndex)
        {
            if (!ValidateInputs(root, config)) return;

            Undo.SetCurrentGroupName("Fill All Ground");
            int group = Undo.GetCurrentGroup();

            for (int y = 0; y < config.GridSize.y; y++)
            {
                for (int x = 0; x < config.GridSize.x; x++)
                {
                    PaintGround(root, config, new Vector2Int(x, y), groundTileIndex);
                }
            }

            Undo.CollapseUndoOperations(group);
        }

        // ── Internal Helpers ────────────────────────────────────────

        private static void PlaceCellObject(MapBuilderRoot root, MapBuilderConfig config,
            Vector2Int index, int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= config.Palette.GroundTiles.Count) return;
            GameObject prefab = config.Palette.GroundTiles[tileIndex];
            if (prefab == null) return;

            root.EnsureChildRoots();

            GameObject cellGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.CellsRoot);
            cellGO.name = $"Cell_{index.x}_{index.y}";
            cellGO.transform.position = config.IndexToWorldPosition(index);
            Undo.RegisterCreatedObjectUndo(cellGO, "Place Cell");
        }

        private static void PlaceDecoObject(MapBuilderRoot root, MapBuilderConfig config,
            Vector2Int index, int decoIndex)
        {
            if (decoIndex < 0 || decoIndex >= config.Palette.DecorationObjects.Count) return;
            GameObject prefab = config.Palette.DecorationObjects[decoIndex].prefab;
            if (prefab == null) return;

            root.EnsureChildRoots();

            // Deco sits on top of the cell tile: offset slightly upward using a flat offset
            Vector3 worldPos = config.IndexToWorldPosition(index);

            GameObject decoGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.DecoRoot);
            decoGO.name = $"Deco_{index.x}_{index.y}";
            decoGO.transform.position = worldPos;
            Undo.RegisterCreatedObjectUndo(decoGO, "Place Deco");
        }

        private static void RemoveCellObject(MapBuilderRoot root, Vector2Int index)
        {
            GameObject existing = root.FindCell(index);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }
        }

        private static void RemoveDecoObject(MapBuilderRoot root, Vector2Int index)
        {
            GameObject existing = root.FindDeco(index);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            // Iterate backwards so removal doesn't affect indices
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static bool ValidateInputs(MapBuilderRoot root, MapBuilderConfig config)
        {
            if (root == null)
            {
                Debug.LogWarning("[MapBuilder] MapBuilderRoot is null.");
                return false;
            }
            if (config == null)
            {
                Debug.LogWarning("[MapBuilder] MapBuilderConfig is null.");
                return false;
            }
            return true;
        }
    }
}
