using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.PlayerData
{
    [System.Serializable]
    public struct AIObjectSpawnInfo
    {
        public string m_SpawnAtCellId;
        public UnitData m_UnitData;
        public UnitAI m_AssociatedAI;
        public CompassDir m_StartDirection;
        public bool m_bIsATarget;
    }

    [CreateAssetMenu(fileName = "NewAITeamData", menuName = "TurnBasedTools/PlayerTeamData/Create AITeamData", order = 1)]
    public class AITeamData : TeamData
    {
        public List<AIObjectSpawnInfo> m_AISpawnUnits;
    }
}
