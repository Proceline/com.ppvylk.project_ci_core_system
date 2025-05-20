using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.InputSupport
{
    [CreateAssetMenu(fileName = "InputActionManager", menuName = "ProjectCI/Input/Input Action Manager")]
    public class InputActionManager : ScriptableObject
    {
        [Header("Grid Navigation")]
        [SerializeField] private InputActionReference m_GridMoveAction;      // 网格移动
        [SerializeField] private InputActionReference m_GridRotateAction;   // 视角旋转
        [SerializeField] private InputActionReference m_GridZoomAction;     // 视角缩放

        [Header("Unit Control")]
        [SerializeField] private InputActionReference m_SelectUnitAction;   // 选择单位
        [SerializeField] private InputActionReference m_CancelAction;       // 取消操作
        [SerializeField] private InputActionReference m_ConfirmAction;      // 确认操作

        [Header("Combat")]
        [SerializeField] private InputActionReference m_SkillAction;        // 技能
        [SerializeField] private InputActionReference m_EndTurnAction;      // 结束回合

        [Header("UI")]
        [SerializeField] private InputActionReference m_MenuAction;         // 打开菜单
        [SerializeField] private InputActionReference m_UnitInfoAction;     // 查看单位信息

        #region Properties
        public InputActionReference GridMoveAction => m_GridMoveAction;
        public InputActionReference GridRotateAction => m_GridRotateAction;
        public InputActionReference GridZoomAction => m_GridZoomAction;
        public InputActionReference SelectUnitAction => m_SelectUnitAction;
        public InputActionReference CancelAction => m_CancelAction;
        public InputActionReference ConfirmAction => m_ConfirmAction;
        public InputActionReference SkillAction => m_SkillAction;
        public InputActionReference EndTurnAction => m_EndTurnAction;
        public InputActionReference MenuAction => m_MenuAction;
        public InputActionReference UnitInfoAction => m_UnitInfoAction;
        #endregion

        #region Enable/Disable All
        public void EnableAllActions()
        {
            EnableGridNavigation();
            EnableUnitControl();
            EnableCombat();
            EnableUI();
        }

        public void DisableAllActions()
        {
            DisableGridNavigation();
            DisableUnitControl();
            DisableCombat();
            DisableUI();
        }
        #endregion

        #region Grid Navigation
        public void EnableGridNavigation()
        {
            EnableAction(m_GridMoveAction);
            EnableAction(m_GridRotateAction);
            EnableAction(m_GridZoomAction);
        }

        public void DisableGridNavigation()
        {
            DisableAction(m_GridMoveAction);
            DisableAction(m_GridRotateAction);
            DisableAction(m_GridZoomAction);
        }
        #endregion

        #region Unit Control
        public void EnableUnitControl()
        {
            EnableAction(m_SelectUnitAction);
            EnableAction(m_CancelAction);
            EnableAction(m_ConfirmAction);
        }

        public void DisableUnitControl()
        {
            DisableAction(m_SelectUnitAction);
            DisableAction(m_CancelAction);
            DisableAction(m_ConfirmAction);
        }
        #endregion

        #region Combat
        public void EnableCombat()
        {
            EnableAction(m_SkillAction);
            EnableAction(m_EndTurnAction);
        }

        public void DisableCombat()
        {
            DisableAction(m_SkillAction);
            DisableAction(m_EndTurnAction);
        }
        #endregion

        #region UI
        public void EnableUI()
        {
            EnableAction(m_MenuAction);
            EnableAction(m_UnitInfoAction);
        }

        public void DisableUI()
        {
            DisableAction(m_MenuAction);
            DisableAction(m_UnitInfoAction);
        }
        #endregion

        #region Utility Methods
        private void EnableAction(InputActionReference actionReference)
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.Enable();
            }
        }

        private void DisableAction(InputActionReference actionReference)
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.Disable();
            }
        }
        #endregion

        #region Common Scenarios
        public void EnableOnlyUI()
        {
            DisableAllActions();
            EnableUI();
        }

        public void EnableOnlyGridNavigation()
        {
            DisableAllActions();
            EnableGridNavigation();
        }

        public void EnableOnlyUnitControl()
        {
            DisableAllActions();
            EnableUnitControl();
        }

        public void EnableOnlyCombat()
        {
            DisableAllActions();
            EnableCombat();
        }

        public void EnableGameplayActions()
        {
            DisableAllActions();
            EnableGridNavigation();
            EnableUnitControl();
            EnableCombat();
        }
        #endregion
    }
} 