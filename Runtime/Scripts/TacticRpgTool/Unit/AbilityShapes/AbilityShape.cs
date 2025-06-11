using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    public class AbilityShape : ScriptableObject
    {
        public virtual List<LevelCellBase> GetCellList(GridPawnUnit InCaster, LevelCellBase InCell, int InRange, bool bAllowBlocked = true, BattleTeam effectedTeam = BattleTeam.None)
        {
            return new List<LevelCellBase>();
        }
    }
}
