using UnityEditor;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.MapBuilder;

namespace ProjectCI.CoreSystem.IEditor.MapBuilder
{
    public enum MapBuilderTool
    {
        PaintGround,
        PaintDeco,
        Erase,
        FillAll
    }

    /// <summary>
    /// Main EditorWindow for the Tactic Map Builder.
    /// Open via: Tools > ProjectCI > Map Builder
    /// </summary>
    public class MapBuilderEditorWindow : EditorWindow
    {
        // ── State ────────────────────────────────────────────────────

        private MapBuilderRoot _root;
        private MapBuilderConfig _config;

        private MapBuilderTool _activeTool = MapBuilderTool.PaintGround;
        private int _selectedGroundIndex = 0;
        private int _selectedDecoIndex   = 0;

        private Vector2 _groundScrollPos;
        private Vector2 _decoScrollPos;

        private bool _sceneHandlerActive = false;

        // Foldout states
        private bool _foldConfig  = true;
        private bool _foldPalette = true;
        private bool _foldTools   = true;

        // Thumbnail cache
        private const float ThumbSize = 64f;

        // ── Public Accessors (used by MapBuilderSceneHandler) ────────

        public MapBuilderTool ActiveTool => _activeTool;
        public int SelectedGroundIndex   => _selectedGroundIndex;
        public int SelectedDecoIndex     => _selectedDecoIndex;

        // ── Menu Entry ───────────────────────────────────────────────

        [MenuItem("Tools/ProjectCI/Map Builder")]
        public static void OpenWindow()
        {
            var window = GetWindow<MapBuilderEditorWindow>("Map Builder");
            window.minSize = new Vector2(320f, 500f);
            window.Show();
        }

        // ── Lifecycle ────────────────────────────────────────────────

        private void OnEnable()
        {
            // Try to find a MapBuilderRoot in the current scene automatically
            if (_root == null)
                _root = FindFirstObjectByType<MapBuilderRoot>();

            if (_root != null)
                _config = _root.Config;

            ActivateSceneHandler();
        }

        private void OnDisable()
        {
            DeactivateSceneHandler();
        }

        // ── GUI ──────────────────────────────────────────────────────

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(4);

            DrawConfigSection();
            EditorGUILayout.Space(4);

            if (_config != null && _config.Palette != null)
            {
                DrawPaletteSection();
                EditorGUILayout.Space(4);
                DrawToolSection();
                EditorGUILayout.Space(8);
                DrawActionButtons();
            }
            else
            {
                if (_config == null)
                    EditorGUILayout.HelpBox("Assign a MapBuilderConfig to continue.", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("Assign a MapPalette inside the Config to continue.", MessageType.Info);
            }
        }

        // ── Sections ─────────────────────────────────────────────────

        private void DrawHeader()
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };
            EditorGUILayout.LabelField("🗺  Tactic Map Builder", headerStyle, GUILayout.Height(26));
            DrawHorizontalLine();
        }

        private void DrawConfigSection()
        {
            _foldConfig = EditorGUILayout.BeginFoldoutHeaderGroup(_foldConfig, "Config");
            if (_foldConfig)
            {
                EditorGUI.indentLevel++;

                // Root reference
                EditorGUI.BeginChangeCheck();
                _root = (MapBuilderRoot)EditorGUILayout.ObjectField(
                    "Scene Root", _root, typeof(MapBuilderRoot), true);
                if (EditorGUI.EndChangeCheck())
                {
                    _config = _root != null ? _root.Config : null;
                    RefreshSceneHandler();
                }

                // Config reference (also editable directly)
                EditorGUI.BeginChangeCheck();
                _config = (MapBuilderConfig)EditorGUILayout.ObjectField(
                    "Map Config", _config, typeof(MapBuilderConfig), false);
                if (EditorGUI.EndChangeCheck())
                {
                    RefreshSceneHandler();
                }

                if (_config != null)
                {
                    EditorGUILayout.Space(2);

                    // Grid type & size (read-only preview — edit in Inspector)
                    using (new EditorGUI.DisabledGroupScope(false))
                    {
                        SerializedObject so = new SerializedObject(_config);
                        so.Update();
                        EditorGUILayout.PropertyField(so.FindProperty("gridType"),   new GUIContent("Grid Type"));
                        EditorGUILayout.PropertyField(so.FindProperty("gridSize"),   new GUIContent("Grid Size"));
                        EditorGUILayout.PropertyField(so.FindProperty("cellSize"),   new GUIContent("Cell Size"));
                        EditorGUILayout.PropertyField(so.FindProperty("palette"),    new GUIContent("Palette"));
                        if (so.ApplyModifiedProperties())
                        {
                            _config = _root != null ? _root.Config : _config;
                            RefreshSceneHandler();
                        }
                    }
                }

                EditorGUI.indentLevel--;

                // Helper button to create a root in the scene
                if (_root == null)
                {
                    EditorGUILayout.Space(4);
                    if (GUILayout.Button("➕  Create MapBuilderRoot in Scene"))
                    {
                        CreateRootInScene();
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawPaletteSection()
        {
            _foldPalette = EditorGUILayout.BeginFoldoutHeaderGroup(_foldPalette, "Palette");
            if (_foldPalette)
            {
                EditorGUI.indentLevel++;

                // ── Ground Tiles ──
                EditorGUILayout.LabelField("Ground Tiles", EditorStyles.boldLabel);
                var groundTiles = _config.Palette.GroundTiles;

                if (groundTiles.Count == 0)
                {
                    EditorGUILayout.HelpBox("No ground tiles in palette.", MessageType.Warning);
                }
                else
                {
                    _groundScrollPos = EditorGUILayout.BeginScrollView(
                        _groundScrollPos, GUILayout.Height(ThumbSize + 24));

                    EditorGUILayout.BeginHorizontal();
                    for (int i = 0; i < groundTiles.Count; i++)
                    {
                        bool selected = _selectedGroundIndex == i;
                        DrawThumbnailButton(groundTiles[i], selected, () =>
                        {
                            _selectedGroundIndex = i;
                            if (_activeTool == MapBuilderTool.PaintDeco || _activeTool == MapBuilderTool.Erase)
                                _activeTool = MapBuilderTool.PaintGround;
                        });
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.Space(6);

                // ── Decoration Objects ──
                EditorGUILayout.LabelField("Decoration Objects", EditorStyles.boldLabel);
                var decos = _config.Palette.DecorationObjects;

                if (decos.Count == 0)
                {
                    EditorGUILayout.HelpBox("No decorations in palette.", MessageType.None);
                }
                else
                {
                    _decoScrollPos = EditorGUILayout.BeginScrollView(
                        _decoScrollPos, GUILayout.Height(ThumbSize + 24));

                    EditorGUILayout.BeginHorizontal();
                    for (int i = 0; i < decos.Count; i++)
                    {
                        bool selected = _selectedDecoIndex == i;
                        DrawThumbnailButton(decos[i].prefab, selected, () =>
                        {
                            _selectedDecoIndex = i;
                            _activeTool = MapBuilderTool.PaintDeco;
                        });
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndScrollView();
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawToolSection()
        {
            _foldTools = EditorGUILayout.BeginFoldoutHeaderGroup(_foldTools, "Brush Tool");
            if (_foldTools)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();

                DrawToolButton("🖌 Ground",  MapBuilderTool.PaintGround);
                DrawToolButton("🌲 Deco",    MapBuilderTool.PaintDeco);
                DrawToolButton("🗑 Erase",   MapBuilderTool.Erase);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox(GetToolHint(), MessageType.None);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawActionButtons()
        {
            DrawHorizontalLine();

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.4f, 0.85f, 0.45f);
            if (GUILayout.Button("⚡ Generate Full Grid", GUILayout.Height(32)))
            {
                if (_root != null && _config != null)
                {
                    MapBuilderOperations.GenerateGrid(_root, _config);
                }
                else
                {
                    EditorUtility.DisplayDialog("Map Builder",
                        "Please assign a Scene Root and a Config first.", "OK");
                }
            }

            GUI.backgroundColor = new Color(0.9f, 0.35f, 0.35f);
            if (GUILayout.Button("🗑 Clear All", GUILayout.Height(32), GUILayout.Width(90)))
            {
                if (_root != null && _config != null &&
                    EditorUtility.DisplayDialog("Clear Map",
                        "This will destroy all generated objects and clear cell data. Are you sure?",
                        "Clear", "Cancel"))
                {
                    MapBuilderOperations.ClearAll(_root, _config);
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // Fill All shortcut
            if (GUILayout.Button("Fill All with Selected Ground Tile"))
            {
                if (_root != null && _config != null)
                    MapBuilderOperations.FillAll(_root, _config, _selectedGroundIndex);
            }

            EditorGUILayout.Space(4);

            // Scene handler toggle
            bool newActive = EditorGUILayout.ToggleLeft(
                "Scene Brush Active  (click/drag in Scene View to paint)",
                _sceneHandlerActive);

            if (newActive != _sceneHandlerActive)
            {
                _sceneHandlerActive = newActive;
                if (_sceneHandlerActive) ActivateSceneHandler();
                else DeactivateSceneHandler();
            }
        }

        // ── GUI Helpers ───────────────────────────────────────────────

        private void DrawThumbnailButton(GameObject prefab, bool selected, System.Action onClick)
        {
            Texture2D thumb = prefab != null
                ? AssetPreview.GetAssetPreview(prefab)
                : null;

            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = selected ? new Color(0.4f, 0.8f, 1f) : Color.white;

            GUIContent content = thumb != null
                ? new GUIContent(thumb, prefab.name)
                : new GUIContent(prefab != null ? prefab.name : "null");

            if (GUILayout.Button(content, GUILayout.Width(ThumbSize), GUILayout.Height(ThumbSize)))
            {
                onClick?.Invoke();
            }

            GUI.backgroundColor = prev;
        }

        private void DrawToolButton(string label, MapBuilderTool tool)
        {
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = _activeTool == tool ? new Color(0.4f, 0.8f, 1f) : Color.white;

            if (GUILayout.Button(label, GUILayout.Height(28)))
                _activeTool = tool;

            GUI.backgroundColor = prev;
        }

        private static void DrawHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        private string GetToolHint()
        {
            return _activeTool switch
            {
                MapBuilderTool.PaintGround => "Click / drag in Scene View to paint selected ground tile.",
                MapBuilderTool.PaintDeco   => "Click / drag in Scene View to place selected decoration.",
                MapBuilderTool.Erase       => "Click / drag in Scene View to erase cells.",
                MapBuilderTool.FillAll     => "Use the 'Fill All' button below.",
                _ => string.Empty
            };
        }

        // ── Scene Handler Management ──────────────────────────────────

        private void ActivateSceneHandler()
        {
            if (_root == null || _config == null) return;
            _sceneHandlerActive = true;
            MapBuilderSceneHandler.Activate(this, _root, _config);
        }

        private void DeactivateSceneHandler()
        {
            _sceneHandlerActive = false;
            MapBuilderSceneHandler.Deactivate();
        }

        private void RefreshSceneHandler()
        {
            if (_root != null && _config != null)
                MapBuilderSceneHandler.RefreshReferences(_root, _config);
        }

        // ── Scene Root Creation ──────────────────────────────────────

        private void CreateRootInScene()
        {
            var go = new GameObject("MapBuilderRoot");
            _root = go.AddComponent<MapBuilderRoot>();
            Undo.RegisterCreatedObjectUndo(go, "Create MapBuilderRoot");
            Selection.activeGameObject = go;
            _config = _root.Config;
        }
    }
}
