using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    using Editor = UnityEditor.Editor;
    
    [CustomEditor(typeof(UnitData))]
    [CanEditMultipleObjects]
    public class UnitDataEditor : Editor
    {
        void DrawAbilityHeader(PropertyReplaceInfo ReplaceInfo, SerializedProperty InArrayProp)
        {
            SerializedProperty unitAbility = InArrayProp.FindPropertyRelative("unitAbility");
            SerializedProperty AssociatedAnimation = InArrayProp.FindPropertyRelative("AssociatedAnimation");
            SerializedProperty ExecuteAfterTime = InArrayProp.FindPropertyRelative("ExecuteAfterTime");
            SerializedProperty AudioOnStart = InArrayProp.FindPropertyRelative("AudioOnStart");
            SerializedProperty AudioOnExecute = InArrayProp.FindPropertyRelative("AudioOnExecute");


            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(unitAbility, new GUIContent(unitAbility.displayName));
            EditorGUILayout.PropertyField(AssociatedAnimation, new GUIContent(AssociatedAnimation.displayName));
            EditorGUILayout.PropertyField(ExecuteAfterTime, new GUIContent(ExecuteAfterTime.displayName));
            EditorGUILayout.PropertyField(AudioOnStart, new GUIContent(AudioOnStart.displayName));
            EditorGUILayout.PropertyField(AudioOnExecute, new GUIContent(AudioOnExecute.displayName));

            EditorGUILayout.EndVertical();
        }

        void DrawUnitClassPopup()
        {
            UnitData unitData = target as UnitData;
            if(unitData)
            {
                EditorUtils.DrawClassPopup<GridPawnUnit>(ref unitData.m_UnitClass);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawUnitClassPopup();

            List<PropertyReplaceInfo> ReplaceInfoList = new List<PropertyReplaceInfo>()
            {
                new PropertyReplaceInfo( "m_Abilities", EditorUtils.MakeCustomArrayWidget, DrawAbilityHeader ),
                new PropertyReplaceInfo( "m_SpawnOnHeal", EditorUtils.MakeCustomArrayWidget ),
                new PropertyReplaceInfo( "m_UnitClass", EditorUtils.MakeBlankWidget ),
            };

            EditorUtils.DrawAllProperties(serializedObject, ReplaceInfoList);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
