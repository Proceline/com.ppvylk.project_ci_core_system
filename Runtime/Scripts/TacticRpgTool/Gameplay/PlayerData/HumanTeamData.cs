using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.PlayerData
{
    [System.Serializable]
    public struct HumanUnitSpawnInfo
    {
        public string m_SpawnAtCellId;
        public UnitData m_UnitData;
        public CompassDir m_StartDirection;
        public bool m_bIsATarget;
    }

    [CreateAssetMenu(fileName = "NewHumanTeamData", menuName = "TurnBasedTools/PlayerTeamData/Create HumanTeamData", order = 1)]
    public class HumanTeamData : TeamData
    {
        public List<HumanUnitSpawnInfo> m_UnitRoster;
    }
}
