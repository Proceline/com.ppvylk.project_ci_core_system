using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Params
{
    [CreateAssetMenu(fileName = "NewBattleSpawnObjectParam", menuName = "ProjectCI/Ability/Parameters/Create BattleSpawnObjectParam", order = 1)]
    public class BattleSpawnObjectParam : BattleAbilityParam
    {
        public GameObject m_Object;
        public Vector3 m_Offset;

        public override void ApplyTo(IUnit InCaster, IObject InTarget)
        {
            // TODO: Implement direct spawn logic
        }

        public override void ApplyTo(IUnit InCaster, Vector2Int InCell)
        {
            // TODO: Implement cell-based spawn logic
            // if(!IsObjectOnCell(InCell))
            // {
            //     SpawnObjectOnCell(m_Object, InCell, m_Offset);
            // }
        }

        public override string GetAbilityInfo()
        {
            return "Spawn: " + m_Object.name;
        }
    }
} 