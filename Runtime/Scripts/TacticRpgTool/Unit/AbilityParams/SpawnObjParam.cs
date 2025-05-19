using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    [CreateAssetMenu(fileName = "NewSpawnObjectParam", menuName = "ProjectCI Tools/Ability/Parameters/ Create SpawnObjectAbilityParam", order = 1)]
    public class SpawnObjParam : AbilityParam
    {
        public GameObject m_Object;
        public Vector3 m_Offset;

        public override void ApplyTo(GridUnit InCaster, LevelCellBase InCell)
        {
            if(!InCell.IsObjectOnCell())
            {
                TacticBattleManager.SpawnObjectOnCell(m_Object, InCell, m_Offset);
            }
        }

        public override string GetAbilityInfo()
        {
            return "Spawn: " + m_Object.name;
        }
    }
}
