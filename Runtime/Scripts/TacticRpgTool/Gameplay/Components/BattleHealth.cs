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

        public abstract void SetHealth(int InHealth);

        public abstract void SetMaxHealth(int InMaxHealth);

        public abstract void ReceiveHealthDamage(int InDamage);

        public virtual void ReceiveHitDamage()
        {
            OnHitPreReceived?.Invoke();
        }

        public virtual void ReactToDefense()
        {
            OnDefensePreReceived?.Invoke();
        }

        public virtual void ReactToDodge()
        {
            OnDodgePreReceived?.Invoke();
        }

        public virtual void Heal()
        {
            OnHealPreReceived?.Invoke();
        }
    }
}
