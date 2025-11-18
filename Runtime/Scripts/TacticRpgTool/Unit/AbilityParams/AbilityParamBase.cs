using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    public abstract class AbilityParamBase : ScriptableObject
    {
        public abstract void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, Queue<CommandResult> results);
        
        public abstract void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            List<LevelCellBase> targetCells, Queue<CommandResult> results);

        public virtual string GetAbilityInfo()
        {
            return name;
        }
    }
}
