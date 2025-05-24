using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    public abstract class SpawnUnitParam<T> : AbilityParamBase where T : GridPawnUnit
    {
        public SoUnitData m_UnitToSpawn;
        public GameObject m_UnitPrefab;

        public override void ApplyTo(GridPawnUnit InCaster, LevelCellBase InCell)
        {
            GridPawnUnit SpawnedUnit = TacticBattleManager.SpawnUnit<T>(m_UnitPrefab, 
                m_UnitToSpawn, InCaster.GetTeam(), InCell.GetIndex());
            SpawnedUnit.HandleTurnStarted();
        }

        public override string GetAbilityInfo()
        {
            return "Spawn: " + m_UnitToSpawn.m_UnitName;
        }
    }
}
