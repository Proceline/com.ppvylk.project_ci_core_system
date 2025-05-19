using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    using Editor = UnityEditor.Editor;
    
    [CustomEditor(typeof(CellStyleInfo))]
    [CanEditMultipleObjects]
    public class CellStyleInfoEditor : Editor
    {

        void DrawArrElem(PropertyReplaceInfo ReplaceInfo, SerializedProperty InArrayProp)
        {
            SerializedProperty m_Index = InArrayProp.FindPropertyRelative("m_Index");
            SerializedProperty m_Material = InArrayProp.FindPropertyRelative("m_Material");

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(m_Index, new GUIContent(m_Index.displayName));
            EditorGUILayout.PropertyField(m_Material, new GUIContent(m_Material.displayName));

            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            List<PropertyReplaceInfo> ReplaceInfoList = new List<PropertyReplaceInfo>()
            {
                new PropertyReplaceInfo("m_HoverMatStates", EditorUtils.MakeCustomArrayWidget, DrawArrElem),
                new PropertyReplaceInfo("m_PositiveMatStates", EditorUtils.MakeCustomArrayWidget, DrawArrElem),
                new PropertyReplaceInfo("m_NegativeMatStates", EditorUtils.MakeCustomArrayWidget, DrawArrElem),
                new PropertyReplaceInfo("m_MovementMatStates", EditorUtils.MakeCustomArrayWidget, DrawArrElem),
            };

            EditorUtils.DrawAllProperties(serializedObject, ReplaceInfoList);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
