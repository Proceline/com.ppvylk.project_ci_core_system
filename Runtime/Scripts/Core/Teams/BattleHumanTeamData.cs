using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Core.Teams
{
    [System.Serializable]
    public struct BattleHumanUnitSpawnInfo
    {
        public string SpawnAtCellId;
        public IUnitData UnitData;
        public CompassDir StartDirection;
        public bool IsTarget;
    }

    [CreateAssetMenu(fileName = "NewBattleHumanTeamData", menuName = "ProjectCI/BattleTeams/Create BattleHumanTeamData", order = 1)]
    public class BattleHumanTeamData : BattleTeamData
    {
        public List<BattleHumanUnitSpawnInfo> UnitRoster;
    }
} 