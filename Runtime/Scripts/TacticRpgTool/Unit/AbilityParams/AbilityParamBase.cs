﻿using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Commands;
using ProjectCI.CoreSystem.Runtime.Attributes;

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

        public virtual void Execute(string resultId, UnitAbilityCore ability, GridPawnUnit fromUnit,
            GridPawnUnit toUnit, List<CommandResult> results)
        {
            // Do nothing by default
        }

        public virtual string GetAbilityInfo()
        {
            return name;
        }
    }
}
