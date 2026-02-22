using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using System.Linq;
using ProjectCI.CoreSystem.Runtime.Attributes;

namespace ProjectCI.CoreSystem.IEditor.TacticRpgTool
{
    [CustomEditor(typeof(SoUnitData))]
    [CanEditMultipleObjects]
    public class UnitDataEditor : Editor
    {
        private SerializedProperty attributesProperty;
        private bool showAttributes = true;
        private int selectedAttributeTypeIndex = 0;
        private AttributeTypeDefinition attributeTypeDefinition;
        private string[] attributeTypeNames = new string[0];

        private void OnEnable()
        {
            attributesProperty = serializedObject.FindProperty("originalAttributes");
            LoadAttributeTypeDefinition();
        }

        private void LoadAttributeTypeDefinition()
        {
            string[] guids = AssetDatabase.FindAssets("t:AttributeTypeDefinition");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                attributeTypeDefinition = AssetDatabase.LoadAssetAtPath<AttributeTypeDefinition>(path);
                if (attributeTypeDefinition != null)
                {
                    attributeTypeNames = attributeTypeDefinition.AttributeTypeNames.ToArray();
                }
            }
            else
            {
                attributeTypeNames = new string[] { "None" };
            }
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
            
            // Draw each element in one line, fill the box
            if (attributesProperty != null)
            {
                for (int i = 0; i < attributesProperty.arraySize; i++)
                {
                    var element = attributesProperty.GetArrayElementAtIndex(i);
                    var typeProp = element.FindPropertyRelative("m_AttributeType");
                    var valueProp = element.FindPropertyRelative("m_Value");
                    var gainValueProp = element.FindPropertyRelative("m_GainValue");

                    // 获取类型索引
                    int typeIndex = -1;
                    if (typeProp != null)
                    {
                        var typeValueProp = typeProp.FindPropertyRelative("value");
                        if (typeValueProp != null && typeValueProp.propertyType == SerializedPropertyType.Integer)
                        {
                            typeIndex = typeValueProp.intValue;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    // Attribute type display (read-only, auto width)
                    string typeName = (typeIndex >= 0 && typeIndex < attributeTypeNames.Length)
                        ? attributeTypeNames[typeIndex]
                        : "Unknown";
                    EditorGUILayout.LabelField(typeName, GUILayout.ExpandWidth(true));
                    // Value field (editable, auto width)
                    if (valueProp != null)
                    {
                        valueProp.intValue = EditorGUILayout.IntField(valueProp.intValue, GUILayout.ExpandWidth(true));
                    }
                    // GainValue field (editable, auto width)
                    if (gainValueProp != null)
                    {
                        gainValueProp.intValue = EditorGUILayout.IntField(gainValueProp.intValue, GUILayout.ExpandWidth(true));
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(10);

            // Add/Remove buttons with attribute type selector
            EditorGUILayout.BeginHorizontal();
            selectedAttributeTypeIndex = EditorGUILayout.Popup(selectedAttributeTypeIndex, attributeTypeNames, GUILayout.Width(150));
            if (GUILayout.Button("Add New Attribute"))
            {
                AddNewAttribute();
            }
            if (GUILayout.Button("Remove Selected Attribute"))
            {
                RemoveSelectedAttribute();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void AddNewAttribute()
        {
            // 只操作当前对象
            var prop = attributesProperty;
            prop.arraySize++;
            var newElement = prop.GetArrayElementAtIndex(prop.arraySize - 1);
            // Initialize new attribute with selected type
            var typeProp = newElement.FindPropertyRelative("m_AttributeType");
            var valueProp = newElement.FindPropertyRelative("m_Value");
            var gainValueProp = newElement.FindPropertyRelative("m_GainValue");
            if (typeProp != null)
            {
                var typeValueProp = typeProp.FindPropertyRelative("value");
                if (typeValueProp != null && typeValueProp.propertyType == SerializedPropertyType.Integer)
                    typeValueProp.intValue = selectedAttributeTypeIndex;
            }
            if (valueProp != null) valueProp.intValue = 0;
            if (gainValueProp != null) gainValueProp.intValue = 0;
            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveSelectedAttribute()
        {
            // 只操作当前对象
            var prop = attributesProperty;
            // 查找第一个匹配的类型并删除
            for (int i = 0; i < prop.arraySize; i++)
            {
                var element = prop.GetArrayElementAtIndex(i);
                var typeProp = element.FindPropertyRelative("m_AttributeType");
                int typeIndex = -1;
                if (typeProp != null)
                {
                    var typeValueProp = typeProp.FindPropertyRelative("value");
                    if (typeValueProp != null && typeValueProp.propertyType == SerializedPropertyType.Integer)
                    {
                        typeIndex = typeValueProp.intValue;
                    }
                }
                if (typeIndex == selectedAttributeTypeIndex)
                {
                    prop.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    break; // 只删第一个
                }
            }
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
