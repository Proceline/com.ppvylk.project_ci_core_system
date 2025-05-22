using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit
{
    public abstract class UnitAbilityAnimation : ScriptableObject
    {
        public abstract void PlayAnimation(GridPawnUnit InUnit);
        public abstract float ExecuteAfterTime(int executeOrder);
        public abstract float GetAnimationLength();
    }
} 