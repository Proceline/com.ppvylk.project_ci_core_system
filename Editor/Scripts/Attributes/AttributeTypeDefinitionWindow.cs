using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using ProjectCI.CoreSystem.Runtime.Attributes;

namespace ProjectCI.CoreSystem.IEditor.Attributes
{
    public class AttributeTypeDefinitionWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private string newTypeName = "";
        private AttributeTypeDefinition currentDefinition;
        private List<AttributeTypeDefinition> allDefinitions = new List<AttributeTypeDefinition>();
        private string newAssetPath = "Assets/Resources/AttributeTypeDefinition.asset";

        [MenuItem("ProjectCI/Attributes/Attribute Type Definition Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<AttributeTypeDefinitionWindow>("Attribute Type Definition");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            RefreshDefinitions();
        }

        private void RefreshDefinitions()
        {
            allDefinitions.Clear();
            string[] guids = AssetDatabase.FindAssets("t:AttributeTypeDefinition");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var definition = AssetDatabase.LoadAssetAtPath<AttributeTypeDefinition>(path);
                if (definition != null)
                {
                    allDefinitions.Add(definition);
                }
            }

            if (allDefinitions.Count == 1)
            {
                currentDefinition = allDefinitions[0];
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            if (allDefinitions.Count == 0)
            {
                DrawCreateNewDefinition();
            }
            else if (allDefinitions.Count == 1)
            {
                DrawSingleDefinition();
            }
            else
            {
                DrawMultipleDefinitions();
            }
        }

        private void DrawCreateNewDefinition()
        {
            EditorGUILayout.LabelField("No AttributeTypeDefinition found. Create a new one:", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            newAssetPath = EditorGUILayout.TextField("Save Path", newAssetPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save AttributeTypeDefinition",
                    "AttributeTypeDefinition",
                    "asset",
                    "Choose where to save the AttributeTypeDefinition"
                );
                if (!string.IsNullOrEmpty(path))
                {
                    newAssetPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Create New Definition"))
            {
                CreateNewDefinition();
            }
        }

        private void DrawSingleDefinition()
        {
            EditorGUILayout.LabelField("Current Definition:", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField(currentDefinition, typeof(AttributeTypeDefinition), false);
            EditorGUILayout.Space(10);

            // Draw the definition editor
            var serializedObject = new SerializedObject(currentDefinition);
            var attributeTypeNamesProperty = serializedObject.FindProperty("attributeTypeNames");

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

        private void DrawMultipleDefinitions()
        {
            EditorGUILayout.LabelField("Multiple AttributeTypeDefinitions found:", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var definition in allDefinitions)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(definition, typeof(AttributeTypeDefinition), false);
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Delete Definition",
                        $"Are you sure you want to delete {definition.name}?",
                        "Yes", "No"))
                    {
                        string path = AssetDatabase.GetAssetPath(definition);
                        AssetDatabase.DeleteAsset(path);
                        RefreshDefinitions();
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Create New Definition"))
            {
                DrawCreateNewDefinition();
            }
        }

        private void CreateNewDefinition()
        {
            // Ensure the directory exists
            string directory = Path.GetDirectoryName(newAssetPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var definition = CreateInstance<AttributeTypeDefinition>();
            AssetDatabase.CreateAsset(definition, newAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RefreshDefinitions();
        }
    }
} 