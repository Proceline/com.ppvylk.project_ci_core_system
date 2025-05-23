using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components
{
    public abstract class BattleHealth : MonoBehaviour
    {
        [HideInInspector]
        public UnityEvent OnDamageReceived = new UnityEvent();

        [HideInInspector]
        public UnityEvent OnHealReceived = new UnityEvent();
        
        [HideInInspector]
        public UnityEvent OnHealthDepleted = new UnityEvent();

        public abstract int GetHealth();

        public abstract int GetMaxHealth();

        public abstract float GetHealthPercentage();

        public abstract void SetHealth(int InHealth);

        public abstract void SetMaxHealth(int InMaxHealth);

        public virtual void Damage(int InDamage)
        {
            OnDamageReceived?.Invoke();
        }

        public virtual void Heal(int InHeal)
        {
            OnHealReceived?.Invoke();
        }
    }
}
