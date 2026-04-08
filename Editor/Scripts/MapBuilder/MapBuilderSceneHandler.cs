using UnityEditor;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.MapBuilder;

namespace ProjectCI.CoreSystem.IEditor.MapBuilder
{
    /// <summary>
    /// Handles Scene View interaction for the Map Builder:
    /// - Draws the grid preview wireframe via Handles
    /// - Detects the hovered cell via XZ plane intersection (no Physics required)
    /// - Forwards paint/erase events to MapBuilderOperations
    /// </summary>
    public static class MapBuilderSceneHandler
    {
        // ── State ────────────────────────────────────────────────────

        private static MapBuilderRoot _root;
        private static MapBuilderConfig _config;
        private static MapBuilderEditorWindow _window;

        private static Vector2Int _hoveredIndex = new Vector2Int(-1, -1);
        private static bool _isActive = false;
        private static bool _isDragging = false;

        // ── Activation ───────────────────────────────────────────────

        public static void Activate(MapBuilderEditorWindow window, MapBuilderRoot root, MapBuilderConfig config)
        {
            _window = window;
            _root = root;
            _config = config;
            _isActive = true;
            SceneView.duringSceneGui -= OnSceneGUI;   // prevent double subscription
            SceneView.duringSceneGui += OnSceneGUI;
            SceneView.RepaintAll();
        }

        public static void Deactivate()
        {
            _isActive = false;
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
        }

        public static void RefreshReferences(MapBuilderRoot root, MapBuilderConfig config)
        {
            _root = root;
            _config = config;
        }

        // ── Scene GUI ────────────────────────────────────────────────

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!_isActive || _config == null) return;

            DrawGridPreview();
            HandleMouseInput();

            // Keep scene repainting while mouse moves over it
            if (Event.current.type == EventType.MouseMove)
            {
                sceneView.Repaint();
            }
        }

        // ── Grid Preview ─────────────────────────────────────────────

        private static void DrawGridPreview()
        {
            float cs = _config.CellSize;
            int w = _config.GridSize.x;
            int h = _config.GridSize.y;

            // Grid is offset by half a cell so that each cell's wireframe
            // is centered on the tile's world position (center-based convention).
            float half = cs * 0.5f;
            float originX = -half;
            float originZ =  half;

            // Horizontal lines (along X axis)
            Handles.color = new Color(0.35f, 0.85f, 0.45f, 0.45f);
            for (int row = 0; row <= h; row++)
            {
                float z = originZ - row * cs;
                Vector3 start = new Vector3(originX,          0f, z);
                Vector3 end   = new Vector3(originX + w * cs, 0f, z);
                Handles.DrawLine(start, end);
            }

            // Vertical lines (along Z axis)
            for (int col = 0; col <= w; col++)
            {
                float x = originX + col * cs;
                Vector3 start = new Vector3(x, 0f, originZ);
                Vector3 end   = new Vector3(x, 0f, originZ - h * cs);
                Handles.DrawLine(start, end);
            }

            // Highlight border
            Handles.color = new Color(0.9f, 0.7f, 0.1f, 0.8f);
            Vector3 bl = new Vector3(originX,          0f, originZ);
            Vector3 br = new Vector3(originX + w * cs, 0f, originZ);
            Vector3 tr = new Vector3(originX + w * cs, 0f, originZ - h * cs);
            Vector3 tl = new Vector3(originX,          0f, originZ - h * cs);
            Handles.DrawLine(bl, br);
            Handles.DrawLine(br, tr);
            Handles.DrawLine(tr, tl);
            Handles.DrawLine(tl, bl);

            // Hover highlight
            if (_config.IsIndexInBounds(_hoveredIndex))
            {
                DrawCellHighlight(_hoveredIndex, new Color(1f, 1f, 0.3f, 0.35f));
            }
        }

        private static void DrawCellHighlight(Vector2Int index, Color color)
        {
            float cs   = _config.CellSize;
            float half = cs * 0.5f;

            // Corners are offset by -half so the highlight is centered on the tile's world position.
            float x0 = index.x * cs - half;
            float z0 = -index.y * cs + half;

            Vector3 a = new Vector3(x0,      0.01f, z0);
            Vector3 b = new Vector3(x0 + cs, 0.01f, z0);
            Vector3 c = new Vector3(x0 + cs, 0.01f, z0 - cs);
            Vector3 d = new Vector3(x0,      0.01f, z0 - cs);

            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(
                new Vector3[] { a, b, c, d },
                color,
                new Color(color.r, color.g, color.b, 1f));
        }

        // ── Mouse Input ──────────────────────────────────────────────

        private static void HandleMouseInput()
        {
            Event e = Event.current;

            // Resolve hovered cell via XZ plane intersection (no Physics / Colliders needed)
            Ray worldRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(worldRay, out float distance))
            {
                Vector3 hit = worldRay.GetPoint(distance);
                _hoveredIndex = _config.WorldPositionToIndex(hit);
            }

            // Only process clicks when a paint/erase tool is active and left mouse button used
            if (_window == null) return;

            bool isLeftMouse = e.button == 0;

            if (isLeftMouse && (e.type == EventType.MouseDown || (e.type == EventType.MouseDrag && _isDragging)))
            {
                if (e.type == EventType.MouseDown) _isDragging = true;

                if (_config.IsIndexInBounds(_hoveredIndex))
                {
                    ApplyActiveTool();
                    e.Use(); // Consume the event so Unity doesn't de-select objects
                }
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                _isDragging = false;
            }
        }

        private static void ApplyActiveTool()
        {
            if (_root == null || _config == null || _window == null) return;

            switch (_window.ActiveTool)
            {
                case MapBuilderTool.PaintGround:
                    if (_window.SelectedGroundIndex >= 0)
                        MapBuilderOperations.PaintGround(_root, _config, _hoveredIndex, _window.SelectedGroundIndex);
                    break;

                case MapBuilderTool.PaintDeco:
                    if (_window.SelectedDecoIndex >= 0)
                        MapBuilderOperations.PaintDeco(_root, _config, _hoveredIndex, _window.SelectedDecoIndex);
                    break;

                case MapBuilderTool.Erase:
                    MapBuilderOperations.EraseCell(_root, _config, _hoveredIndex);
                    break;
            }
        }
    }
}
