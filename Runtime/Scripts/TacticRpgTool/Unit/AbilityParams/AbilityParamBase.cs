using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

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

        public virtual string GetAbilityInfo()
        {
            return name;
        }
    }
}
