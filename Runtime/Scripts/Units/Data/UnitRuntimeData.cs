using ProjectCI_CoreSystem.Runtime.Scripts.Units.Interfaces;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Units.Data
{
    /// <summary>
    /// Represents the runtime data for a unit, including all dynamic resources
    /// </summary>
    public class UnitRuntimeData
    {
        public IUnitResource<float> Health { get; }
        public IUnitResource<int> Armor { get; }
        public IUnitResource<int> MagicalArmor { get; }

        public UnitRuntimeData(float initialHealth, int initialArmor, int initialMagicalArmor)
        {
            Health = new UnitPreciseResource(initialHealth);
            Armor = new UnitDiscreteResource(initialArmor);
            MagicalArmor = new UnitDiscreteResource(initialMagicalArmor);
        }
    }
} 