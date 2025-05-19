using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    using Editor = UnityEditor.Editor;
    
    [CustomEditor(typeof(CellPalette))]
    [CanEditMultipleObjects]
    public class CellPaletteEditor : Editor
    {
        void DrawArrElem(PropertyReplaceInfo ReplaceInfo, SerializedProperty InArrayProp)
        {
            SerializedProperty NameProp = InArrayProp.FindPropertyRelative("m_Name");
            SerializedProperty TilesProp = InArrayProp.FindPropertyRelative("m_Cells");

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(NameProp, new GUIContent(""));

            PropertyReplaceInfo TileReplaceInfo = new PropertyReplaceInfo("m_Cells", EditorUtils.MakeCustomArrayWidget);

            EditorUtils.MakeCustomArrayWidget(TileReplaceInfo, TilesProp);

            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            List<PropertyReplaceInfo> ReplaceInfoList = new List<PropertyReplaceInfo>()
            {
                new PropertyReplaceInfo("m_CellPieces", EditorUtils.MakeCustomArrayWidget, DrawArrElem)
            };

            EditorUtils.DrawAllProperties(serializedObject, ReplaceInfoList);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
