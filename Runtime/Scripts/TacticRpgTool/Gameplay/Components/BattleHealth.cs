using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components
{
    public class BattleHealth : MonoBehaviour
    {
        int m_Health;
        int m_Armor;
        int m_MagicalArmor;

        int m_MaxHealth;
        int m_MaxArmor;
        int m_MaxMagicalArmor;

        [HideInInspector]
        public UnityEvent OnHealthDepleted = new UnityEvent();

        [HideInInspector]
        public UnityEvent OnArmorDepleted = new UnityEvent();

        [HideInInspector]
        public UnityEvent OnMagicalArmorDepleted = new UnityEvent();

        [HideInInspector]
        public UnityEvent OnHit = new UnityEvent();

        [HideInInspector]
        public UnityEvent OnHeal = new UnityEvent();

        public int GetHealth()
        {
            return m_Health;
        }

        public int GetArmor()
        {
            return m_Armor;
        }

        public int GetMagicArmor()
        {
            return m_MagicalArmor;
        }

        public float GetHealthPercentage()
        {
            return ((float)m_Health) / ((float)m_MaxHealth);
        }

        public float GetArmorPercentage()
        {
            return ((float)m_Armor) / ((float)m_MaxArmor);
        }

        public float GetMagicArmorPercentage()
        {
            return ((float)m_MagicalArmor) / ((float)m_MaxMagicalArmor);
        }

        public void SetHealth(int InHealth)
        {
            m_Health = InHealth;
            m_MaxHealth = m_Health;
        }

        public void SetArmor(int InArmor)
        {
            m_Armor = InArmor;
            m_MaxArmor = m_Armor;
        }

        public void SetMagicArmor(int InMagicalArmor)
        {
            m_MagicalArmor = InMagicalArmor;
            m_MaxMagicalArmor = m_MagicalArmor;
        }

        public void Damage(int InDamage)
        {
            OnHit.Invoke();

            int HealthBefore = m_Health;
            int ArmorBefore = m_Armor;

            int HealthDamage = InDamage;
            if (m_Armor > 0)
            {
                HealthDamage = HealthDamage - m_Armor;
                if (HealthDamage < 0)
                {
                    HealthDamage = 0;
                }
            }
            m_Armor -= InDamage;
            if (m_Armor < 0)
            {
                m_Armor = 0;
            }
            m_Health -= HealthDamage;
            if (m_Health < 0)
            {
                m_Health = 0;
            }

            if (m_Armor <= 0 && ArmorBefore > 0)
            {
                OnArmorDepleted.Invoke();
            }

            if (m_Health <= 0 && HealthBefore > 0)
            {
                OnHealthDepleted.Invoke();
            }
        }

        public void MagicDamage(int InDamage)
        {
            OnHit.Invoke();

            int HealthBefore = m_Health;
            int MagicArmorBefore = m_MagicalArmor;

            int HealthDamage = InDamage;
            if (m_MagicalArmor > 0)
            {
                HealthDamage = HealthDamage - m_MagicalArmor;
                if (HealthDamage < 0)
                {
                    HealthDamage = 0;
                }
            }
            m_MagicalArmor -= InDamage;
            if (m_MagicalArmor < 0)
            {
                m_MagicalArmor = 0;
            }
            m_Health -= HealthDamage;
            if (m_Health < 0)
            {
                m_Health = 0;
            }

            if (m_MagicalArmor <= 0 && MagicArmorBefore > 0)
            {
                OnMagicalArmorDepleted.Invoke();
            }

            if (m_Health <= 0 && HealthBefore > 0)
            {
                OnHealthDepleted.Invoke();
            }
        }

        public void Heal(int InHeal)
        {
            m_Health += InHeal;
            if (m_Health > m_MaxHealth)
            {
                m_Health = m_MaxHealth;
            }

            OnHeal.Invoke();
        }

        public void ReplenishArmor(int InArmor)
        {
            m_Armor += InArmor;
            if (m_Armor > m_MaxArmor)
            {
                m_Armor = m_MaxArmor;
            }

            OnHeal.Invoke();
        }

        public void ReplenishMagicArmor(int InMagicArmor)
        {
            m_MagicalArmor += InMagicArmor;
            if (m_MagicalArmor > m_MaxMagicalArmor)
            {
                m_MagicalArmor = m_MaxMagicalArmor;
            }

            OnHeal.Invoke();
        }

        public void IncreaseMaxHealth(int InIncreaseBy)
        {
            m_MaxHealth += InIncreaseBy;
        }

        public void IncreaseMaxArmor(int InIncreaseBy)
        {
            m_MaxArmor += InIncreaseBy;
        }

        public void IncreaseMaxMagicArmor(int InIncreaseBy)
        {
            m_MaxMagicalArmor += InIncreaseBy;
        }
    }
}
