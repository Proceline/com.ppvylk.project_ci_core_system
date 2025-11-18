using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    public abstract class AbilityParamBase : ScriptableObject
    {
        /// <summary>
        /// Execute on each target
        /// </summary>
        /// <param name="resultId">identical if in same ability</param>
        /// <param name="ability">target ability</param>
        /// <param name="fromUnit">caster</param>
        /// <param name="mainTarget">main target, might not be current target cell</param>
        /// <param name="targetCell">target cell</param>
        /// <param name="results">calculated result</param>
        public abstract void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit mainTarget, LevelCellBase targetCell, Queue<CommandResult> results);

        public virtual string GetAbilityInfo()
        {
            return name;
        }
    }
}
