using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.Attributes;

namespace ProjectCI.CoreSystem.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(AttributeType))]
    public class AttributeTypePropertyDrawer : PropertyDrawer
    {
        private AttributeTypeDefinition attributeTypeDefinition;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Find the AttributeTypeDefinition asset
            if (attributeTypeDefinition == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:AttributeTypeDefinition");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    attributeTypeDefinition = AssetDatabase.LoadAssetAtPath<AttributeTypeDefinition>(path);
                }
            }

            EditorGUI.BeginProperty(position, label, property);

            // Draw the label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Get the value property
            SerializedProperty valueProperty = property.FindPropertyRelative("value");

            if (attributeTypeDefinition != null)
            {
                // Create popup with attribute type names
                string[] typeNames = new string[attributeTypeDefinition.AttributeTypeNames.Count];
                for (int i = 0; i < typeNames.Length; i++)
                {
                    typeNames[i] = attributeTypeDefinition.AttributeTypeNames[i];
                }

                int currentIndex = valueProperty.intValue;
                if (currentIndex >= 0 && currentIndex < typeNames.Length)
                {
                    int newIndex = EditorGUI.Popup(position, currentIndex, typeNames);
                    if (newIndex != currentIndex)
                    {
                        valueProperty.intValue = newIndex;
                    }
                }
                else
                {
                    EditorGUI.Popup(position, 0, typeNames);
                    valueProperty.intValue = 0;
                }
            }
            else
            {
                // Fallback to int field if no definition found
                EditorGUI.PropertyField(position, valueProperty, GUIContent.none);
            }

            EditorGUI.EndProperty();
        }
    }
} 