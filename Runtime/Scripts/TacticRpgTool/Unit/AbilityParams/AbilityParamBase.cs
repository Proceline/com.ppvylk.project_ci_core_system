using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Commands;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    public abstract class AbilityParamBase : ScriptableObject
    {
        public abstract void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, Queue<CommandResult> results);

        public virtual string GetAbilityInfo()
        {
            return name;
        }
    }
}
