using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    public struct PropertyReplaceInfo
    {
        public PropertyReplaceInfo(string InPropName, Action<PropertyReplaceInfo,SerializedProperty> InFunction, Action<PropertyReplaceInfo,SerializedProperty> InSecondaryFunction = null)
        {
            PropName = InPropName;
            FunctionToCall = InFunction;
            SecondaryFunctionToCall = InSecondaryFunction;
        }

        public string PropName;
        public Action<PropertyReplaceInfo, SerializedProperty> FunctionToCall;
        public Action<PropertyReplaceInfo, SerializedProperty> SecondaryFunctionToCall;
    }

    public class EditorUtils
    {
        public struct Icons
        {
            public static Texture LoadAddIcon() { return Resources.Load<Texture>("Icons/GreenPlus"); }
            public static Texture LoadRemoveIcon() { return Resources.Load<Texture>("Icons/RedX"); }
            public static Texture LoadUpIcon() { return Resources.Load<Texture>("Icons/UpArrow"); }
            public static Texture LoadDownIcon() { return Resources.Load<Texture>("Icons/DownArrow"); }
            public static Texture LoadInfoIcon() { return Resources.Load<Texture>("Icons/Info"); }
        }

        public static void DrawLineSeparation(Color color, int height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            rect.height = 2;
            EditorGUI.DrawRect(rect, color);
        }

        public static void MakeBlankWidget(PropertyReplaceInfo ReplaceInfo, SerializedProperty InArrayProp){}

        public static void MakeCustomArrayWidget(PropertyReplaceInfo ReplaceInfo, SerializedProperty InArrayProp)
        {
            GUIStyle BorderStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(5, 5, 5, 5)
            };

            GUIStyle SmallBorderStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(1, 1, 1, 1),
                padding = new RectOffset(1, 1, 1, 1),
                fixedWidth = 28.0f
            };

            GUIStyle LabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };

            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = false,
            };

            GUILayout.Space(5);

            GUI.backgroundColor = Color.white;
            EditorGUILayout.BeginVertical(BorderStyle);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button(Icons.LoadAddIcon(), ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                InArrayProp.InsertArrayElementAtIndex(InArrayProp.arraySize);
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Label(InArrayProp.displayName, LabelStyle);

            EditorGUILayout.EndHorizontal();

            List<int> IndexesToRemove = new List<int>();

            GUI.backgroundColor = Color.grey;
            EditorGUILayout.BeginVertical(BorderStyle);

            int numAbilities = InArrayProp.arraySize;
            for (int i = 0; i < numAbilities; i++)
            {
                SerializedProperty ability = InArrayProp.GetArrayElementAtIndex(i);

                GUI.backgroundColor = Color.white;
                EditorGUILayout.BeginHorizontal(BorderStyle);

                EditorGUILayout.BeginVertical(SmallBorderStyle);

                GUI.backgroundColor = Color.white;

                if (numAbilities > 1)
                {
                    if (i > 0)
                    {
                        if (GUILayout.Button(Icons.LoadUpIcon(), ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            InArrayProp.MoveArrayElement(i, i - 1);
                        }
                    }
                }

                GUI.backgroundColor = Color.red;

                if (GUILayout.Button(Icons.LoadRemoveIcon(), ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    IndexesToRemove.Add(i);
                }

                GUI.backgroundColor = Color.white;

                if(numAbilities > 1)
                {
                    if(i < numAbilities - 1)
                    {
                        if (GUILayout.Button(Icons.LoadDownIcon(), ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            InArrayProp.MoveArrayElement(i, i + 1);
                        }
                    }
                }

                EditorGUILayout.EndVertical();

                if (ReplaceInfo.SecondaryFunctionToCall != null)
                {
                    ReplaceInfo.SecondaryFunctionToCall(ReplaceInfo, ability);
                }
                else
                {
                    EditorGUILayout.PropertyField(ability, new GUIContent(""));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();


            foreach (int index in IndexesToRemove)
            {
                SerializedProperty abilityProp = InArrayProp.GetArrayElementAtIndex(index);
                
                if (abilityProp != null && abilityProp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    abilityProp.objectReferenceValue = null;
                }

                InArrayProp.DeleteArrayElementAtIndex(index);
            }

            IndexesToRemove.Clear();

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
        }
        
        public static void DrawAllProperties(SerializedObject InSerializedObj, List<PropertyReplaceInfo> ReplacementInfo = null)
        {
            bool bCheckChildren = true;
            SerializedProperty it = InSerializedObj.GetIterator();
            while (it.NextVisible(bCheckChildren))
            {
                if (it != null)
                {
                    bool bFoundReplacement = false;

                    SerializedProperty prop = InSerializedObj.FindProperty(it.name);
                    if(ReplacementInfo != null)
                    {
                        foreach (PropertyReplaceInfo ReplaceInfo in ReplacementInfo)
                        {
                            if( ReplaceInfo.PropName == prop.name )
                            {
                                ReplaceInfo.FunctionToCall(ReplaceInfo, prop);
                                bFoundReplacement = true;
                                break;
                            }
                        }
                    }

                    if(!bFoundReplacement)
                    {
                        EditorGUILayout.PropertyField(prop, new GUIContent(prop.displayName));
                    }
                }
                bCheckChildren = false;
            }
        }

        public static List<System.Type> GetTypes<T>() where T : class
        {
            var types = new List<System.Type>();

            types.Add(typeof(T));

            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("Mono.Cecil"))
                    continue;

                if (assembly.FullName.StartsWith("UnityScript"))
                    continue;

                if (assembly.FullName.StartsWith("Boo.Lan"))
                    continue;

                if (assembly.FullName.StartsWith("System"))
                    continue;

                if (assembly.FullName.StartsWith("I18N"))
                    continue;

                if (assembly.FullName.StartsWith("UnityEngine"))
                    continue;

                if (assembly.FullName.StartsWith("UnityEditor"))
                    continue;

                if (assembly.FullName.StartsWith("mscorlib"))
                    continue;

                foreach (System.Type type in assembly.GetTypes())
                {
                    if (!type.IsClass)
                        continue;

                    if (type.IsAbstract)
                        continue;

                    if (!type.IsSubclassOf(typeof(T)))
                        continue;

                    types.Add(type);
                }
            }

            return types;
        }

        

        public static void DrawClassPopup<T>(ref string outType) where T : class
        {
            List<System.Type> types = GetTypes<T>();
            Dictionary<int, System.Type> IndexToType = new Dictionary<int, System.Type>();
            List<string> optionList = new List<string>();

            int index = 0;
            foreach (System.Type item in types)
            {
                IndexToType.Add(index++, item);
                optionList.Add(item.Name);
            }

            System.Type classType = null;

            if (outType == "" || outType == null)
            {
                classType = types[0];
            }
            else
            {
                classType = GameUtils.FindType(outType);
            }

            if(classType != null)
            {
                System.Type beforeType = classType;

                int PopupIndex = optionList.FindIndex((string inStr) => { return inStr == beforeType.Name; });

                PopupIndex = EditorGUILayout.Popup("UnitClass", PopupIndex, optionList.ToArray());

                classType = IndexToType[PopupIndex];

                outType = classType.FullName;
            }
        }
    }
}
