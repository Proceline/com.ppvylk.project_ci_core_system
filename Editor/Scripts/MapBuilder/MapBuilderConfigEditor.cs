using UnityEditor;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.MapBuilder;

namespace ProjectCI.CoreSystem.IEditor.MapBuilder
{
    /// <summary>
    /// Custom Inspector for MapBuilderConfig.
    /// Provides a shortcut button to open the Map Builder EditorWindow directly from the Inspector.
    /// </summary>
    [CustomEditor(typeof(MapBuilderConfig))]
    public class MapBuilderConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the "Open Map Builder" button at the top
            GUI.backgroundColor = new Color(0.4f, 0.85f, 0.45f);
            if (GUILayout.Button("Open Map Builder Window", GUILayout.Height(30)))
            {
                MapBuilderEditorWindow.OpenWindow();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(4);
            DrawHorizontalLine();
            EditorGUILayout.Space(4);

            // Draw the default Inspector fields below
            DrawDefaultInspector();

            EditorGUILayout.Space(6);

            // Show cell data summary
            MapBuilderConfig config = (MapBuilderConfig)target;
            EditorGUILayout.LabelField(
                $"Stored Cells: {config.CellDataMap.Count}  /  " +
                $"Total Grid: {config.GridSize.x * config.GridSize.y}",
                EditorStyles.miniLabel);
        }

        private static void DrawHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }
    }
}
