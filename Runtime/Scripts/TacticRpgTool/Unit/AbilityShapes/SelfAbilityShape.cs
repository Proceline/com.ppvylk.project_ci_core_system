using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    [CreateAssetMenu(fileName = "NewSelfAbilityShape", menuName = "ProjectCI Tools/Ability/Shapes/Create SelfAbilityShape", order = 1)]
    public class SelfAbilityShape : AbilityShape
    {
        public override List<LevelCellBase> GetCellList(GridPawnUnit inCaster, LevelCellBase inCell, int inRange, bool bAllowBlocked = true, BattleTeam effectTeam = BattleTeam.None)
        {
            return new List<LevelCellBase> { inCaster.GetCell() };
        }
    }
}
