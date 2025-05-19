using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.Core.Rules
{
    public class TacticalBattleRules : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference selectAction;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference cancelAction;
        [SerializeField] private InputActionReference abilityAction;

        protected void Init()
        {
            cancelAction.action.canceled += null;
        }
    }
} 