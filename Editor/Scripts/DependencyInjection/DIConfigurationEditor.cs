using System;
using System.Collections.Generic;
using System.Linq;
using ProjectCI.CoreSystem.DependencyInjection;
using UnityEditor;
using UnityEngine;

namespace ProjectCI.CoreSystem.Editor.Attributes
{
    using Editor = UnityEditor.Editor;
    
    [CustomEditor(typeof(DIConfiguration))]
    public class DIConfigurationEditor : Editor
    {
        private readonly List<string> _options = new();

        [NonSerialized] private bool _optionUpdated;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SerializedProperty typesProperty = serializedObject.FindProperty("staticInjectTargets");
            GUI.enabled = false;
            EditorGUILayout.PropertyField(typesProperty);
            GUI.enabled = true;
            
            if (GUILayout.Button("Refresh Static Targets"))
            {

                for (var i = typesProperty.arraySize - 1; i >= 0; i--)
                {
                    typesProperty.DeleteArrayElementAtIndex(i);
                }

                var typeTargets = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.FullName.Contains(".Editor") && !a.FullName.Contains(".Tests"))
                    .SelectMany(s => s.GetTypes())
                    .Where(p => p.GetCustomAttributes(typeof(StaticInjectableTargetAttribute), false).Length > 0);

                var index = 0;
                foreach (var typeTarget in typeTargets)
                {
                    typesProperty.InsertArrayElementAtIndex(index);
                    typesProperty.GetArrayElementAtIndex(index).stringValue = typeTarget.AssemblyQualifiedName;
                }

                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssetIfDirty(target);
            }

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
} 