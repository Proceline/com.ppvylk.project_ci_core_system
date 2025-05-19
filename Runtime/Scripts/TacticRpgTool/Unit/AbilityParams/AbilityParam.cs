using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams
{
    public class AbilityParam : ScriptableObject
    {
        public virtual void ApplyTo(GridUnit InCaster, GridObject InObject)
        {
            
        }
        
        public virtual void ApplyTo(GridUnit InCaster, ILevelCell InCell)
        {

        }

        public virtual string GetAbilityInfo()
        {
            return name;
        }
    }
}
