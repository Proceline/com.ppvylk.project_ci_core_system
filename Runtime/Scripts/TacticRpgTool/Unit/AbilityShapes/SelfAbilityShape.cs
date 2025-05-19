using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    [CreateAssetMenu(fileName = "NewSelfAbilityShape", menuName = "ProjectCI Tools/Ability/Shapes/Create SelfAbilityShape", order = 1)]
    public class SelfAbilityShape : AbilityShape
    {
        public override List<LevelCellBase> GetCellList(GridUnit InCaster, LevelCellBase InCell, int InRange, bool bAllowBlocked = true, GameTeam m_EffectedTeam = GameTeam.None)
        {
            return new List<LevelCellBase> { InCaster.GetCell() };
        }
    }
}
