using ProjectCI.CoreSystem.DependencyInjection;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Editor.TacticRpgTool;

namespace ProjectCI.CoreSystem.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(ObjectServiceRegistration), true)]
    public class ObjectDependencyInjectPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 不使用PrefixLabel，直接绘制字段，让左侧label为空

            // Get the value properties
            SerializedProperty serviceTypeProperty = property.FindPropertyRelative("serviceTypeName");
            SerializedProperty scriptableObjectProperty = property.FindPropertyRelative("objectInstance");

            if (scriptableObjectProperty != null)
            {
                var targetObject = scriptableObjectProperty.objectReferenceValue;

                // Calculate positions for each field
                float fieldHeight = EditorGUIUtility.singleLineHeight;
                float spacing = EditorGUIUtility.standardVerticalSpacing;

                Rect serviceTypeRect = new Rect(position.x, position.y, position.width, fieldHeight);
                Rect scriptableObjectRect = new Rect(position.x, position.y + fieldHeight + spacing, position.width,
                    fieldHeight);

                if (targetObject != null)
                {
                    EditorUtils.DrawInterfacesPopup(serviceTypeRect, targetObject, ref serviceTypeProperty);

                }
                // Draw Service Type Name field
                // EditorGUI.PropertyField(serviceTypeRect, serviceTypeProperty, new GUIContent("Service Type Name"));

                // Draw ScriptableObject Instance field
                EditorGUI.PropertyField(scriptableObjectRect, scriptableObjectProperty,
                    new GUIContent("UnityObject Instance"));
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Calculate height for all three fields plus spacing
            float fieldHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            return fieldHeight * 2 + spacing;
        }
    }
} 