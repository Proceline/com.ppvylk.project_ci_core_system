using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    using Editor = UnityEditor.Editor;
    
    [CustomEditor(typeof(BasicUnitAbility))]
    [CanEditMultipleObjects]
    public class UnitAbilityEditor : Editor
    {
        void DrawAbilityHeader()
        {
            SerializedProperty m_Script = serializedObject.FindProperty("m_Script");
            SerializedProperty m_AbilityName = serializedObject.FindProperty("m_AbilityName");
            SerializedProperty m_Icon = serializedObject.FindProperty("m_Icon");
            SerializedProperty m_Radius = serializedObject.FindProperty("m_Radius");
            SerializedProperty m_ActionPointCost = serializedObject.FindProperty("m_ActionPointCost");
            SerializedProperty m_bAllowBlocked = serializedObject.FindProperty("m_bAllowBlocked");
            SerializedProperty m_EffectedType = serializedObject.FindProperty("m_EffectedType");
            SerializedProperty m_EffectedTeam = serializedObject.FindProperty("m_EffectedTeam");
            SerializedProperty m_AbilityShape = serializedObject.FindProperty("m_AbilityShape");

            SerializedProperty m_EffectShape = serializedObject.FindProperty("m_EffectShape");
            SerializedProperty m_EffectRadius = serializedObject.FindProperty("m_EffectRadius");

            Texture2D IconTexture = AssetPreview.GetAssetPreview(m_Icon.objectReferenceValue);

            GUIStyle BorderStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(5, 5, 5, 5)
            };

            EditorGUILayout.PropertyField(m_Script, new GUIContent(m_Script.displayName));

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(BorderStyle, GUILayout.Width(100), GUILayout.Height(110));
            GUI.backgroundColor = Color.white;

            GUILayout.Label(IconTexture, GUILayout.Width(100), GUILayout.Height(100));
            EditorGUILayout.PropertyField(m_Icon, new GUIContent(""));

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(m_AbilityName, new GUIContent(m_AbilityName.displayName));
            EditorGUILayout.PropertyField(m_Radius, new GUIContent(m_Radius.displayName));
            EditorGUILayout.PropertyField(m_ActionPointCost, new GUIContent(m_ActionPointCost.displayName));
            EditorGUILayout.PropertyField(m_bAllowBlocked, new GUIContent(m_bAllowBlocked.displayName));
            EditorGUILayout.PropertyField(m_EffectedType, new GUIContent(m_EffectedType.displayName));
            EditorGUILayout.PropertyField(m_EffectedTeam, new GUIContent(m_EffectedTeam.displayName));
            EditorGUILayout.PropertyField(m_AbilityShape, new GUIContent(m_AbilityShape.displayName));

            EditorGUILayout.PropertyField(m_EffectShape, new GUIContent(m_EffectShape.displayName));
            EditorGUILayout.PropertyField(m_EffectRadius, new GUIContent(m_EffectRadius.displayName));

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            DrawAbilityHeader();

            List<PropertyReplaceInfo> ReplaceInfoList = new List<PropertyReplaceInfo>()
            {
                new PropertyReplaceInfo("m_Params", EditorUtils.MakeCustomArrayWidget),
                new PropertyReplaceInfo("m_AbilityName", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_Icon", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_Radius", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_ActionPointCost", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_bAllowBlocked", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_EffectedType", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_EffectedTeam", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_AbilityTimeLength", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_AbilityShape", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_SpawnOnCaster", EditorUtils.MakeCustomArrayWidget),
                new PropertyReplaceInfo("m_SpawnOnTarget", EditorUtils.MakeCustomArrayWidget),
                new PropertyReplaceInfo("m_Ailments", EditorUtils.MakeCustomArrayWidget),
                new PropertyReplaceInfo("m_EffectShape", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_EffectRadius", EditorUtils.MakeBlankWidget),
                new PropertyReplaceInfo("m_Script", EditorUtils.MakeBlankWidget),
            };

            EditorUtils.DrawAllProperties(serializedObject, ReplaceInfoList);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
