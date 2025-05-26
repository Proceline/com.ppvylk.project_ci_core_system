using System;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components
{
    public abstract class BattleHealth : MonoBehaviour
    {
        [HideInInspector]
        public UnityEvent OnHitPreReceived = new UnityEvent();
        
        [HideInInspector]
        public UnityEvent OnDefensePreReceived = new UnityEvent();
        
        [HideInInspector]
        public UnityEvent OnDodgePreReceived = new UnityEvent();

        [HideInInspector]
        public UnityEvent OnHealPreReceived = new UnityEvent();
        
        [HideInInspector]
        public UnityEvent OnHealthPreDepleted = new UnityEvent();

        public event Action<int> OnPostHitReceived;
        public event Action<int> OnPostDefenseReceived;
        public event Action<int> OnPostDodgeReceived;
        public event Action<int> OnPostHealReceived;

        public abstract int GetHealth();

        public abstract int GetMaxHealth();

        public abstract float GetHealthPercentage();

        public abstract void SetHealth(int InHealth);

        public abstract void SetMaxHealth(int InMaxHealth);

        public virtual void ReceiveHitDamage(int InDamage)
        {
            OnHitPreReceived?.Invoke();
        }

        public virtual void ReactToDefense(int InDefense)
        {
            OnDefensePreReceived?.Invoke();
        }

        public virtual void ReactToDodge(int InDodge)
        {
            OnDodgePreReceived?.Invoke();
        }

        public virtual void Heal(int InHeal)
        {
            OnHealPreReceived?.Invoke();
        }

        protected void CallPostHitReceived(int InDamage)
        {
            OnPostHitReceived?.Invoke(InDamage);
        }

        protected void CallPostDefenseReceived(int InDefense)
        {
            OnPostDefenseReceived?.Invoke(InDefense);
        }

        protected void CallPostDodgeReceived(int InDodge)
        {
            OnPostDodgeReceived?.Invoke(InDodge);
        }

        protected void CallPostHealReceived(int InHeal)
        {
            OnPostHealReceived?.Invoke(InHeal);
        }
    }
}
