using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System.Linq;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    using Editor = UnityEditor.Editor;
    
    [CustomEditor(typeof(SoUnitData))]
    [CanEditMultipleObjects]
    public class UnitDataEditor : Editor
    {
        private SerializedProperty attributesProperty;
        private bool showAttributes = true;

        private void OnEnable()
        {
            attributesProperty = serializedObject.FindProperty("originalAttributes");
        }

        void DrawUnitClassPopup()
        {
            SoUnitData unitData = target as SoUnitData;
            if(unitData)
            {
                EditorUtils.DrawClassPopup<GridPawnUnit>(ref unitData.m_UnitClass);
            }
        }

        private void DrawAttributesArray()
        {
            showAttributes = EditorGUILayout.Foldout(showAttributes, "Attributes", true);
            if (!showAttributes) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Draw current attributes
            if (attributesProperty != null)
            {
                EditorGUILayout.PropertyField(attributesProperty, true);
            }

            EditorGUILayout.Space(10);

            // Add/Remove buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Add New Attribute"))
            {
                AddNewAttribute();
            }
            
            if (GUILayout.Button("Remove Last Attribute"))
            {
                RemoveLastAttribute();
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void AddNewAttribute()
        {
            // Get all SoUnitData assets
            var allUnitData = AssetDatabase.FindAssets("t:SoUnitData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<SoUnitData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(data => data != null)
                .ToList();

            // Add new attribute to all assets
            foreach (var unitData in allUnitData)
            {
                var so = new SerializedObject(unitData);
                var prop = so.FindProperty("originalAttributes");
                
                prop.arraySize++;
                var newElement = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                
                // Initialize new attribute with default values
                var typeProp = newElement.FindPropertyRelative("type");
                var valueProp = newElement.FindPropertyRelative("value");
                
                if (typeProp != null) typeProp.enumValueIndex = 0;
                if (valueProp != null) valueProp.floatValue = 0f;
                
                so.ApplyModifiedProperties();
            }

            // Refresh the current editor
            serializedObject.Update();
        }

        private void RemoveLastAttribute()
        {
            // Get all SoUnitData assets
            var allUnitData = AssetDatabase.FindAssets("t:SoUnitData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<SoUnitData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(data => data != null)
                .ToList();

            // Remove last attribute from all assets
            foreach (var unitData in allUnitData)
            {
                var so = new SerializedObject(unitData);
                var prop = so.FindProperty("originalAttributes");
                
                if (prop.arraySize > 0)
                {
                    prop.arraySize--;
                    so.ApplyModifiedProperties();
                }
            }

            // Refresh the current editor
            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            DrawUnitClassPopup();

            List<PropertyReplaceInfo> ReplaceInfoList = new List<PropertyReplaceInfo>()
            {
                new PropertyReplaceInfo( "m_SpawnOnHeal", EditorUtils.MakeCustomArrayWidget ),
                new PropertyReplaceInfo( "m_UnitClass", EditorUtils.MakeBlankWidget ),
                new PropertyReplaceInfo( "originalAttributes", EditorUtils.MakeBlankWidget ),
            };

            EditorUtils.DrawAllProperties(serializedObject, ReplaceInfoList);

            // Draw our custom attributes array UI
            DrawAttributesArray();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
