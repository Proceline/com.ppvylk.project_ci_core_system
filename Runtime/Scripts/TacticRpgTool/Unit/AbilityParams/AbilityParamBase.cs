using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Commands;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    public abstract class AbilityParamBase : ScriptableObject
    {
        public virtual void ApplyTo(GridPawnUnit InCaster, GridObject InObject)
        {
            
        }
        
        public virtual void ApplyTo(GridPawnUnit InCaster, LevelCellBase InCell)
        {

        }

        public virtual void Execute(GridPawnUnit InCaster, GridPawnUnit InTarget, ref List<CommandResult> results)
        {
            // Do nothing by default
        }

        public virtual string GetAbilityInfo()
        {
            return name;
        }
    }
}
