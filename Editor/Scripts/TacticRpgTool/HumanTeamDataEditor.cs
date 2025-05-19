using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.PlayerData;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    using Editor = UnityEditor.Editor;
    
    [CustomEditor(typeof(HumanTeamData))]
    [CanEditMultipleObjects]
    public class HumanTeamDataEditor : Editor
    {
        void DrawArrElem(PropertyReplaceInfo ReplaceInfo, SerializedProperty InArrayProp)
        {
            SerializedProperty m_SpawnAtCellId = InArrayProp.FindPropertyRelative("m_SpawnAtCellId");
            SerializedProperty UnitDataProp = InArrayProp.FindPropertyRelative("m_UnitData");
            SerializedProperty bIsATarget = InArrayProp.FindPropertyRelative("m_bIsATarget");
            SerializedProperty m_StartDirection = InArrayProp.FindPropertyRelative("m_StartDirection");

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(m_SpawnAtCellId, new GUIContent("(Optional)" + m_SpawnAtCellId.displayName));
            EditorGUILayout.PropertyField(UnitDataProp, new GUIContent(UnitDataProp.displayName));
            EditorGUILayout.PropertyField(m_StartDirection, new GUIContent(m_StartDirection.displayName));
            EditorGUILayout.PropertyField(bIsATarget, new GUIContent(bIsATarget.displayName));

            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            List<PropertyReplaceInfo> ReplaceInfoList = new List<PropertyReplaceInfo>()
            {
                new PropertyReplaceInfo("m_UnitRoster", EditorUtils.MakeCustomArrayWidget, DrawArrElem)
            };

            EditorUtils.DrawAllProperties(serializedObject, ReplaceInfoList);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
