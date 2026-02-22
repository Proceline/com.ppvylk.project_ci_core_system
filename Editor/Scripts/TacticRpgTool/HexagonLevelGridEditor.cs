using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;

namespace ProjectCI.CoreSystem.IEditor.TacticRpgTool
{
    [CustomEditor(typeof(HexagonPresetGrid))]
    [CanEditMultipleObjects]
    public class HexagonLevelGridEditor : Editor
    {
        SerializedProperty m_CellPalette;

        private void OnEnable()
        {
            m_CellPalette = serializedObject.FindProperty("m_CellPalette");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_CellPalette, new GUIContent(m_CellPalette.displayName));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
