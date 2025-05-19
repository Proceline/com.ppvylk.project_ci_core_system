using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    [CreateAssetMenu(fileName = "NewSelfAbilityShape", menuName = "TurnBasedTools/Ability/Shapes/Create SelfAbilityShape", order = 1)]
    public class SelfAbilityShape : AbilityShape
    {
        public override List<ILevelCell> GetCellList(GridUnit InCaster, ILevelCell InCell, int InRange, bool bAllowBlocked = true, GameTeam m_EffectedTeam = GameTeam.None)
        {
            return new List<ILevelCell> { InCaster.GetCell() };
        }
    }
}
