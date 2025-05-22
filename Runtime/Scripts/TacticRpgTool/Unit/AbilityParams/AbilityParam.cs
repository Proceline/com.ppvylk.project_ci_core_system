using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    public class AbilityParam : ScriptableObject
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
