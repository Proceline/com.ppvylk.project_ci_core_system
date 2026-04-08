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
        [Header("Config")]
        [SerializeField]
        private MapBuilderConfig config;

        [Header("Hierarchy Roots (auto-created if missing)")]
        [SerializeField]
        private Transform cellsRoot;

        [SerializeField]
        private Transform decoRoot;

        // ── Public Accessors ────────────────────────────────────────

        public MapBuilderConfig Config => config;
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

        // ── Helpers ─────────────────────────────────────────────────

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
