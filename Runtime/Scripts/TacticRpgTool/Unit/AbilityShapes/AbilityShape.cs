using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    public class AbilityShape : ScriptableObject
    {
        public virtual List<ILevelCell> GetCellList(GridUnit InCaster, ILevelCell InCell, int InRange, bool bAllowBlocked = true, GameTeam m_EffectedTeam = GameTeam.None)
        {
            return new List<ILevelCell>();
        }
    }
}
