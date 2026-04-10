using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.MapBuilder
{
    /// <summary>
    /// Scene-level root for the Map Builder tool.
    /// Place this MonoBehaviour on an empty GameObject in the scene.
    /// It acts as the parent container for all generated cell and decoration GameObjects,
    /// and holds a reference to the MapBuilderConfig asset being edited.
    /// </summary>
    [ExecuteAlways]
    public class MapBuilderRoot : MonoBehaviour
    {
        [Header("Edit Mode")]
        [SerializeField]
        private bool isEditModeEnabled = false;

        [Header("Config")]
        [SerializeField]
        private MapBuilderConfig config;

        [Header("Hierarchy Roots (auto-created if missing)")]
        [SerializeField]
        private Transform cellsRoot;

        [SerializeField]
        private Transform decoRoot;

        [Header("Runtime Settings")]
        [Tooltip("Prevent Awake from auto-generating the map at Play Mode start. " +
                 "Useful when you want to control generation timing manually.")]
        [SerializeField]
        private bool stopAwakeGeneration = false;


        // ── Public Accessors ────────────────────────────────────────

        public bool IsEditModeEnabled => isEditModeEnabled;
        public MapBuilderConfig Config 
        { 
            get => config;
            set => config = value; 
        }

        public Transform CellsRoot => cellsRoot;
        public Transform DecoRoot => decoRoot;

        // ── Lifecycle ───────────────────────────────────────────────

        private void OnValidate()
        {
            EnsureChildRoots();
        }

        private void Reset()
        {
            EnsureChildRoots();
        }

        private void Awake()
        {
            // [ExecuteAlways] causes Awake to fire in Edit Mode too — block that here.
            if (!Application.isPlaying) return;

            // INTENT check: if Edit Mode is enabled, the Editor was responsible
            // for this map. Cells may or may not exist (config could be empty),
            // but either way, runtime should not generate anything.
            if (isEditModeEnabled) return;

            // EVIDENCE check: defensive guard against accidental double-spawn.
            if (cellsRoot && cellsRoot.childCount > 0) return;

            // Edit Mode was OFF → scene is empty → generate from config at runtime.
            if (config && !stopAwakeGeneration)
                RuntimeGenerateFromConfig();
        }

        // ── Runtime Generation ───────────────────────────────────────

        /// <summary>
        /// Generates the map at runtime (Play Mode) using Instantiate.
        /// Called only when isEditModeEnabled is false and the scene has no pre-spawned cells.
        /// This is intentionally separate from the Editor-side generation in MapBuilderOperations
        /// which uses PrefabUtility.InstantiatePrefab. Zero Editor API usage here.
        /// </summary>
        public void RuntimeGenerateFromConfig()
        {
            EnsureChildRoots();
            ClearRuntimeGeneratedObjects();

            if (config == null || config.Palette == null)
            {
                Debug.LogWarning("[MapBuilder] Cannot generate at runtime: config or palette is null.");
                return;
            }

            var groundTiles = config.Palette.GroundTiles;
            var decoObjects = config.Palette.DecorationObjects;

            foreach (var entry in config.CellDataMap)
            {
                if (!config.IsIndexInBounds(entry.index)) continue;

                Vector3 worldPos = config.IndexToWorldPosition(entry.index);

                // Ground tile
                if (entry.groundTileIndex >= 0 && entry.groundTileIndex < groundTiles.Count)
                {
                    var prefab = groundTiles[entry.groundTileIndex];
                    if (prefab != null)
                    {
                        var go = Instantiate(prefab, cellsRoot);
                        go.name = $"Cell_{entry.index.x}_{entry.index.y}";
                        go.transform.position = worldPos;
                    }
                }

                // Decoration
                if (entry.decoObjectIndex >= 0 && entry.decoObjectIndex < decoObjects.Count)
                {
                    var decoEntry = decoObjects[entry.decoObjectIndex];
                    if (decoEntry.prefab != null)
                    {
                        var go = Instantiate(decoEntry.prefab, decoRoot);
                        go.name = $"Deco_{entry.index.x}_{entry.index.y}";
                        go.transform.position = worldPos;
                    }
                }
            }
        }

        private void ClearRuntimeGeneratedObjects()
        {
            if (!Application.isPlaying) return;

            if (cellsRoot != null)
            {
                for (int i = cellsRoot.childCount - 1; i >= 0; i--)
                {
                    Destroy(cellsRoot.GetChild(i).gameObject);
                }
            }

            if (decoRoot != null)
            {
                for (int i = decoRoot.childCount - 1; i >= 0; i--)
                {
                    Destroy(decoRoot.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        /// Ensures the CellsRoot and DecoRoot child transforms exist.
        /// Called automatically via OnValidate and Reset.
        /// Can also be called manually by editor tools before generating objects.
        /// </summary>
        public void EnsureChildRoots()
        {
            if (cellsRoot == null)
            {
                var existing = transform.Find("Cells");
                if (existing != null)
                {
                    cellsRoot = existing;
                }
                else
                {
                    var go = new GameObject("Cells");
                    go.transform.SetParent(transform, false);
                    cellsRoot = go.transform;
                }
            }

            if (decoRoot == null)
            {
                var existing = transform.Find("Decorations");
                if (existing != null)
                {
                    decoRoot = existing;
                }
                else
                {
                    var go = new GameObject("Decorations");
                    go.transform.SetParent(transform, false);
                    decoRoot = go.transform;
                }
            }
        }

        /// <summary>
        /// Finds a cell GameObject by its grid index name tag ("Cell_X_Y").
        /// Returns null if not found.
        /// </summary>
        public GameObject FindCell(Vector2Int index)
        {
            if (cellsRoot == null) return null;
            string targetName = $"Cell_{index.x}_{index.y}";
            Transform found = cellsRoot.Find(targetName);
            return found != null ? found.gameObject : null;
        }

        /// <summary>
        /// Finds a decoration GameObject by its grid index name tag ("Deco_X_Y").
        /// Returns null if not found.
        /// </summary>
        public GameObject FindDeco(Vector2Int index)
        {
            if (decoRoot == null) return null;
            string targetName = $"Deco_{index.x}_{index.y}";
            Transform found = decoRoot.Find(targetName);
            return found != null ? found.gameObject : null;
        }
    }
}
