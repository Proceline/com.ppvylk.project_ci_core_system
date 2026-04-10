using UnityEditor;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.MapBuilder;

namespace ProjectCI.CoreSystem.IEditor.MapBuilder
{
    /// <summary>
    /// Custom Inspector for MapBuilderRoot.
    ///
    /// Responsibilities:
    ///  - Draws a prominent Edit Mode toggle with clear visual feedback.
    ///  - Detects changes to isEditModeEnabled and config via BeginChangeCheck.
    ///  - Responds to Undo/Redo via Undo.undoRedoPerformed.
    ///  - Calls MapBuilderOperations to clear / regenerate scene objects accordingly.
    ///
    /// Zero editor code lives in the Runtime assembly — all logic is here.
    /// </summary>
    [CustomEditor(typeof(MapBuilderRoot))]
    public class MapBuilderRootEditor : Editor
    {
        // ── Tracked previous state ───────────────────────────────────

        private MapBuilderConfig _previousConfig;
        private bool _previousEditMode;

        // ── Lifecycle ────────────────────────────────────────────────

        private void OnEnable()
        {
            var root = (MapBuilderRoot)target;
            _previousConfig   = root.Config;
            _previousEditMode = root.IsEditModeEnabled;

            // Handle Undo / Redo — Unity restores serialized values silently,
            // so we re-evaluate the state after any undo/redo operation.
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        // ── Inspector GUI ────────────────────────────────────────────

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var root = (MapBuilderRoot)target;

            // ── Edit Mode Toggle ─────────────────────────────────────
            DrawEditModeToggle(root);

            EditorGUILayout.Space(6);
            DrawHorizontalLine();
            EditorGUILayout.Space(4);

            // ── Rest of fields (config, hierarchy roots) ─────────────
            EditorGUI.BeginChangeCheck();

            // Config field
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("config"),
                new GUIContent("Map Config"));

            // Hierarchy roots
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("cellsRoot"),
                new GUIContent("Cells Root"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("decoRoot"),
                new GUIContent("Deco Root"));

            bool changed = EditorGUI.EndChangeCheck();

            if (serializedObject.ApplyModifiedProperties() || changed)
            {
                EvaluateAndApplyChanges(root);
            }

            // ── Runtime Settings ─────────────────────────────────────
            // Drawn outside the BeginChangeCheck block — this field does not
            // affect Editor-time scene generation, so no need to trigger EvaluateAndApplyChanges.
            EditorGUILayout.Space(4);
            DrawHorizontalLine();
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Runtime Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("stopAwakeGeneration"),
                new GUIContent("Stop Awake Generation",
                    "Prevent Awake from auto-generating the map at Play Mode start."));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(4);

            // ── Manual refresh button ─────────────────────────────────
            if (root.IsEditModeEnabled)
            {
                if (GUILayout.Button("🔄  Refresh from Config", GUILayout.Height(26)))
                {
                    ForceRefresh(root);
                }
            }
        }

        // ── Edit Mode Toggle UI ───────────────────────────────────────

        private void DrawEditModeToggle(MapBuilderRoot root)
        {
            SerializedProperty editModeProp = serializedObject.FindProperty("isEditModeEnabled");

            bool currentValue = editModeProp.boolValue;

            // Colored background to make state obvious
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = currentValue
                ? new Color(0.3f, 0.85f, 0.4f)   // green = editing
                : new Color(0.55f, 0.55f, 0.55f); // grey  = off

            string label = currentValue
                ? "✏  Edit Mode  ON"
                : "⬜  Edit Mode  OFF";

            EditorGUI.BeginChangeCheck();
            bool newValue = GUILayout.Toggle(currentValue, label, "Button", GUILayout.Height(32));
            bool toggled = EditorGUI.EndChangeCheck();

            GUI.backgroundColor = prevBg;

            if (toggled)
            {
                Undo.RecordObject(target, "Toggle MapBuilder Edit Mode");
                editModeProp.boolValue = newValue;
                serializedObject.ApplyModifiedProperties();
                EvaluateAndApplyChanges((MapBuilderRoot)target);
            }
        }

        // ── Change Evaluation ─────────────────────────────────────────

        private void EvaluateAndApplyChanges(MapBuilderRoot root)
        {
            bool editModeChanged = root.IsEditModeEnabled != _previousEditMode;
            bool configChanged   = root.Config != _previousConfig;

            if (root.IsEditModeEnabled)
            {
                // Edit mode is ON — regenerate whenever something relevant changed
                if (editModeChanged || configChanged)
                {
                    ClearAndRegenerate(root);
                }
            }
            else
            {
                // Edit mode is OFF — clear scene objects if we just turned it off
                if (editModeChanged)
                {
                    SafeClearAll(root);
                }
                // If it was already OFF and config changed, do nothing.
            }

            _previousConfig   = root.Config;
            _previousEditMode = root.IsEditModeEnabled;
        }

        // ── Operations ────────────────────────────────────────────────

        private void ClearAndRegenerate(MapBuilderRoot root)
        {
            // Only clear scene objects - preserve the previous config's cellDataMap data
            MapBuilderOperations.ClearSceneObjects(root);

            if (root.Config != null)
            {
                MapBuilderOperations.RestoreFromConfig(root, root.Config);
            }
        }

        private static void SafeClearAll(MapBuilderRoot root,
            MapBuilderConfig configOverride = null)
        {
            // Clear scene objects only — never touch cellDataMap here.
            // cellDataMap is persistent save data and must not be wiped during normal edit flow.
            MapBuilderOperations.ClearSceneObjects(root);
        }

        private static void DestroyChildrenOf(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
            }
        }

        private void ForceRefresh(MapBuilderRoot root)
        {
            MapBuilderOperations.ClearSceneObjects(root);
            if (root.Config != null)
            {
                MapBuilderOperations.RestoreFromConfig(root, root.Config);
            }
            _previousConfig   = root.Config;
            _previousEditMode = root.IsEditModeEnabled;
        }

        // ── Undo / Redo ───────────────────────────────────────────────

        private void OnUndoRedo()
        {
            // After an undo/redo, Unity has already restored serialized values.
            // Re-read the current state and apply if needed.
            if (target == null) return;
            var root = (MapBuilderRoot)target;

            // Force re-evaluate regardless of what looks changed,
            // because Undo could have reverted to any past state.
            bool wasEditMode = _previousEditMode;

            _previousConfig   = null; // force "config changed" detection
            _previousEditMode = !root.IsEditModeEnabled; // force "toggle changed" detection

            EvaluateAndApplyChanges(root);
        }

        // ── UI Helpers ────────────────────────────────────────────────

        private static void DrawHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        }
    }
}
