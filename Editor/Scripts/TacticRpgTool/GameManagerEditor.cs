using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using UnityEditor;

namespace ProjectCI.CoreSystem.IEditor.TacticRpgTool
{   
    [CustomEditor(typeof(TacticBattleManager))]
    [CanEditMultipleObjects]
    public class GameManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            List<PropertyReplaceInfo> ReplaceInfoList = new List<PropertyReplaceInfo>()
            {
                new PropertyReplaceInfo("m_WinConditions", EditorUtils.MakeCustomArrayWidget),
                new PropertyReplaceInfo("m_SpawnOnStart", EditorUtils.MakeCustomArrayWidget),
                new PropertyReplaceInfo("m_AddToSpawnedUnits", EditorUtils.MakeCustomArrayWidget),
                new PropertyReplaceInfo("m_DeathParticles", EditorUtils.MakeCustomArrayWidget),
            };

            EditorUtils.DrawAllProperties(serializedObject, ReplaceInfoList);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
