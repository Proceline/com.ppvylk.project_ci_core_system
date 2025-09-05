using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    public class AbilityShape : ScriptableObject
    {
        public virtual List<LevelCellBase> GetCellList(GridPawnUnit caster, LevelCellBase cell, int range, bool isAllowBlock = true, BattleTeam effectedTeam = BattleTeam.None)
        {
            return new List<LevelCellBase>();
        }
    }
}
