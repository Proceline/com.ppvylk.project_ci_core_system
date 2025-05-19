using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    using Editor = UnityEditor.Editor;
    
    [CustomEditor(typeof(CellAilment))]
    [CanEditMultipleObjects]
    public class CellAilmentEditor : Editor
    {
        void DrawArrElem(PropertyReplaceInfo ReplaceInfo, SerializedProperty InArrayProp)
        {
            SerializedProperty NameProp = InArrayProp.FindPropertyRelative("m_Name");
            SerializedProperty TilesProp = InArrayProp.FindPropertyRelative("m_Tiles");

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(NameProp, new GUIContent(""));

            PropertyReplaceInfo TileReplaceInfo = new PropertyReplaceInfo("m_Tiles", EditorUtils.MakeCustomArrayWidget);

            EditorUtils.MakeCustomArrayWidget(TileReplaceInfo, TilesProp);

            EditorGUILayout.EndVertical();
        }

        public void MakeStructWidget(PropertyReplaceInfo ReplaceInfo, SerializedProperty InProp)
        {
            SerializedProperty SpawnObjList = InProp.FindPropertyRelative("m_SpawnOnReciever");
            SerializedProperty ParamList = InProp.FindPropertyRelative("m_Params");
            SerializedProperty m_AudioClip = InProp.FindPropertyRelative("m_AudioClip");

            GUIStyle BorderStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(5, 5, 5, 5)
            };

            GUIStyle LabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };

            GUI.backgroundColor = Color.gray;

            EditorGUILayout.BeginVertical(BorderStyle);

            GUI.backgroundColor = Color.white;

            EditorGUILayout.LabelField(InProp.displayName, LabelStyle);

            PropertyReplaceInfo SpawnObjListReplaceInfo = new PropertyReplaceInfo("m_SpawnOnReciever", EditorUtils.MakeCustomArrayWidget);
            PropertyReplaceInfo ParamListReplaceInfo = new PropertyReplaceInfo("m_Params", EditorUtils.MakeCustomArrayWidget);

            EditorUtils.MakeCustomArrayWidget(SpawnObjListReplaceInfo, SpawnObjList);
            EditorUtils.MakeCustomArrayWidget(ParamListReplaceInfo, ParamList);

            EditorGUILayout.BeginVertical(BorderStyle);

            EditorGUILayout.LabelField("Audio", LabelStyle);
            EditorGUILayout.PropertyField(m_AudioClip, new GUIContent("On Execute"));

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        void DrawWeightInfo(PropertyReplaceInfo ReplaceInfo, SerializedProperty InProp)
        {
            SerializedProperty bBlocked = InProp.FindPropertyRelative("bBlocked");
            SerializedProperty Weight = InProp.FindPropertyRelative("Weight");

            GUIStyle BorderStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(5, 5, 5, 5)
            };

            GUIStyle LabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };

            EditorGUILayout.BeginVertical(BorderStyle);
            EditorGUILayout.LabelField(InProp.displayName, LabelStyle);

            EditorGUILayout.PropertyField(bBlocked, new GUIContent(bBlocked.displayName));
            EditorGUILayout.PropertyField(Weight, new GUIContent(Weight.displayName));

            EditorGUILayout.EndVertical();

        }

        public override void OnInspectorGUI()
        {
            List<PropertyReplaceInfo> ReplaceInfoList = new List<PropertyReplaceInfo>()
            {
                new PropertyReplaceInfo("m_ExecuteOnUnitOver", MakeStructWidget),
                new PropertyReplaceInfo("m_ExecuteOnStartOfTurn", MakeStructWidget),
                new PropertyReplaceInfo("m_ExecuteOnEndOfTurn", MakeStructWidget),
                new PropertyReplaceInfo("m_WeightInfo", DrawWeightInfo),
            };

            EditorUtils.DrawAllProperties(serializedObject, ReplaceInfoList);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
