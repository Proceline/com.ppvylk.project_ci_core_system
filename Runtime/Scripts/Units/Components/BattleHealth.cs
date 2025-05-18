using UnityEngine;
using UnityEngine.Events;
using ProjectCI_CoreSystem.Runtime.Scripts.Units.Data;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Units.Components
{
    /// <summary>
    /// Represents the health system for a battle unit, including health, armor, and magical armor
    /// </summary>
    public class BattleHealth : MonoBehaviour
    {
        [SerializeField] private float initialHealth = 100f;
        [SerializeField] private int initialArmor = 50;
        [SerializeField] private int initialMagicalArmor = 50;

        private UnitRuntimeData runtimeData;

        // Events
        [HideInInspector] public UnityEvent OnHealthDepleted = new UnityEvent();
        [HideInInspector] public UnityEvent OnArmorDepleted = new UnityEvent();
        [HideInInspector] public UnityEvent OnMagicalArmorDepleted = new UnityEvent();
        [HideInInspector] public UnityEvent OnHit = new UnityEvent();
        [HideInInspector] public UnityEvent OnHeal = new UnityEvent();

        private void Awake()
        {
            runtimeData = new UnitRuntimeData(initialHealth, initialArmor, initialMagicalArmor);
        }

        public float GetHealth() => runtimeData.Health.CurrentValue;
        public int GetArmor() => runtimeData.Armor.CurrentValue;
        public int GetMagicArmor() => runtimeData.MagicalArmor.CurrentValue;

        public float GetHealthPercentage() => runtimeData.Health.CurrentValue / runtimeData.Health.MaxValue;
        public float GetArmorPercentage() => (float)runtimeData.Armor.CurrentValue / runtimeData.Armor.MaxValue;
        public float GetMagicArmorPercentage() => (float)runtimeData.MagicalArmor.CurrentValue / runtimeData.MagicalArmor.MaxValue;

        public void SetHealth(float health)
        {
            runtimeData.Health.SetValue(health, health);
        }

        public void SetArmor(int armor)
        {
            runtimeData.Armor.SetValue(armor, armor);
        }

        public void SetMagicArmor(int magicalArmor)
        {
            runtimeData.MagicalArmor.SetValue(magicalArmor, magicalArmor);
        }

        public void Damage(float damage)
        {
            OnHit.Invoke();

            float healthBefore = runtimeData.Health.CurrentValue;
            int armorBefore = runtimeData.Armor.CurrentValue;

            float healthDamage = damage;
            if (runtimeData.Armor.CurrentValue > 0)
            {
                healthDamage = Mathf.Max(0, damage - runtimeData.Armor.CurrentValue);
            }

            runtimeData.Armor.ModifyValue(-Mathf.RoundToInt(damage));
            runtimeData.Health.ModifyValue(-healthDamage);

            if (runtimeData.Armor.CurrentValue <= 0 && armorBefore > 0)
            {
                OnArmorDepleted.Invoke();
            }

            if (runtimeData.Health.CurrentValue <= 0 && healthBefore > 0)
            {
                OnHealthDepleted.Invoke();
            }
        }

        public void MagicDamage(float damage)
        {
            OnHit.Invoke();

            float healthBefore = runtimeData.Health.CurrentValue;
            int magicArmorBefore = runtimeData.MagicalArmor.CurrentValue;

            float healthDamage = damage;
            if (runtimeData.MagicalArmor.CurrentValue > 0)
            {
                healthDamage = Mathf.Max(0, damage - runtimeData.MagicalArmor.CurrentValue);
            }

            runtimeData.MagicalArmor.ModifyValue(-Mathf.RoundToInt(damage));
            runtimeData.Health.ModifyValue(-healthDamage);

            if (runtimeData.MagicalArmor.CurrentValue <= 0 && magicArmorBefore > 0)
            {
                OnMagicalArmorDepleted.Invoke();
            }

            if (runtimeData.Health.CurrentValue <= 0 && healthBefore > 0)
            {
                OnHealthDepleted.Invoke();
            }
        }

        public void Heal(float heal)
        {
            runtimeData.Health.ModifyValue(heal);
            OnHeal.Invoke();
        }

        public void ReplenishArmor(int armor)
        {
            runtimeData.Armor.ModifyValue(armor);
            OnHeal.Invoke();
        }

        public void ReplenishMagicArmor(int magicalArmor)
        {
            runtimeData.MagicalArmor.ModifyValue(magicalArmor);
            OnHeal.Invoke();
        }

        public void IncreaseMaxHealth(float increaseBy)
        {
            runtimeData.Health.SetValue(runtimeData.Health.CurrentValue, runtimeData.Health.MaxValue + increaseBy);
        }

        public void IncreaseMaxArmor(int increaseBy)
        {
            runtimeData.Armor.SetValue(runtimeData.Armor.CurrentValue, runtimeData.Armor.MaxValue + increaseBy);
        }

        public void IncreaseMaxMagicArmor(int increaseBy)
        {
            runtimeData.MagicalArmor.SetValue(runtimeData.MagicalArmor.CurrentValue, runtimeData.MagicalArmor.MaxValue + increaseBy);
        }
    }
} 