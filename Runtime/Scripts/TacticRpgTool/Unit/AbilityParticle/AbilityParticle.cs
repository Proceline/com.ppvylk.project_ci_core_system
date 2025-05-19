using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities
{
    public class AbilityParticle : MonoBehaviour
    {
        public float DeleteAfterTime;

        public virtual void Setup(UnitAbility InAbility, GridUnit InCaster, ILevelCell InTarget)
        {
            Destroy(gameObject, DeleteAfterTime);
        }
    }
}
