using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Params
{
    [CreateAssetMenu(fileName = "NewBattleSpawnUnitParam", menuName = "ProjectCI/Ability/Parameters/Create BattleSpawnUnitParam", order = 1)]
    public class BattleSpawnUnitParam : BattleAbilityParam
    {
        [SerializeField] public IUnitData m_UnitToSpawn;

        public override void ApplyTo(IUnit InCaster, IObject InTarget)
        {
            // TODO: Implement direct spawn unit logic
        }

        public override void ApplyTo(IUnit InCaster, Vector2Int InCell)
        {
            // TODO: Implement cell-based spawn unit logic
            // IUnit SpawnedUnit = SpawnUnit(m_UnitToSpawn, InCaster.GetTeam(), InCell);
            // SpawnedUnit.HandleTurnStarted();
        }

        public override string GetAbilityInfo()
        {
            return "Spawn: " + m_UnitToSpawn.UnitName;
        }
    }
} 