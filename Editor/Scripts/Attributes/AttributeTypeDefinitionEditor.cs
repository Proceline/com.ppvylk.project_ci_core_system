using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Attributes;

namespace ProjectCI.CoreSystem.Editor.Attributes
{
    [CustomEditor(typeof(AttributeTypeDefinition))]
    public class AttributeTypeDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty attributeTypeNamesProperty;
        private Vector2 scrollPosition;
        private string newTypeName = "";

        private void OnEnable()
        {
            attributeTypeNamesProperty = serializedObject.FindProperty("attributeTypeNames");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Attribute Type Names", EditorStyles.boldLabel);

            // Add new type name
            EditorGUILayout.BeginHorizontal();
            newTypeName = EditorGUILayout.TextField("New Type Name", newTypeName);
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                if (!string.IsNullOrEmpty(newTypeName))
                {
                    attributeTypeNamesProperty.arraySize++;
                    var newElement = attributeTypeNamesProperty.GetArrayElementAtIndex(attributeTypeNamesProperty.arraySize - 1);
                    newElement.stringValue = newTypeName;
                    newTypeName = "";
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Display existing type names
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < attributeTypeNamesProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(attributeTypeNamesProperty.GetArrayElementAtIndex(i), GUIContent.none);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    attributeTypeNamesProperty.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();
        }
    }
} 