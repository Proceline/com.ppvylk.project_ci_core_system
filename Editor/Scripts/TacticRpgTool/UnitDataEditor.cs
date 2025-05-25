using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    using Editor = UnityEditor.Editor;
    
    [CustomEditor(typeof(SoUnitData))]
    [CanEditMultipleObjects]
    public class UnitDataEditor : Editor
    {
        void DrawUnitClassPopup()
        {
            SoUnitData unitData = target as SoUnitData;
            if(unitData)
            {
                EditorUtils.DrawClassPopup<GridPawnUnit>(ref unitData.m_UnitClass);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawUnitClassPopup();

            List<PropertyReplaceInfo> ReplaceInfoList = new List<PropertyReplaceInfo>()
            {
                new PropertyReplaceInfo( "m_SpawnOnHeal", EditorUtils.MakeCustomArrayWidget ),
                new PropertyReplaceInfo( "m_UnitClass", EditorUtils.MakeBlankWidget ),
            };

            EditorUtils.DrawAllProperties(serializedObject, ReplaceInfoList);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
