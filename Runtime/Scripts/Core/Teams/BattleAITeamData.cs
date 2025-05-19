using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Core.Teams
{
    [System.Serializable]
    public struct BattleAIUnitSpawnInfo
    {
        public string SpawnAtCellId;
        public IUnitData UnitData;
        public UnitAI AssociatedAI;
        public CompassDir StartDirection;
        public bool IsTarget;
    }

    [CreateAssetMenu(fileName = "NewBattleAITeamData", menuName = "ProjectCI/BattleTeams/Create BattleAITeamData", order = 1)]
    public class BattleAITeamData : BattleTeamData
    {
        public List<BattleAIUnitSpawnInfo> AISpawnUnits;
    }
} 