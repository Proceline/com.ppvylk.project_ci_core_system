using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components
{
    public abstract class BattleHealth : MonoBehaviour
    {
        public abstract void SetHealth(int inHealth);

        public abstract void SetMaxHealth(int inMaxHealth);
    }
}
