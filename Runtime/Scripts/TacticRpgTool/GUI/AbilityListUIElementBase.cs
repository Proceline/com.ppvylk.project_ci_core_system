using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI
{
    /// <summary>
    /// Add it to SpawnOnStart in TacticBattleManager
    /// </summary>
    public class AbilityListUIElementBase : MonoBehaviour
    {
        [NonSerialized]
        protected GridPawnUnit m_SelectedUnit;

        [SerializeField]
        protected Canvas uiContainer;

        [SerializeField]
        private Transform listContainer;

        [SerializeField]
        private SlotDataUIElementBase abilityUIElementPrefab;

        private readonly List<SlotDataUIElementBase> m_UIAbilities = 
            new List<SlotDataUIElementBase>();

        [NonSerialized]
        private Camera m_UICamera;

        private void Start()
        {
            UpdateRotation();
        }

        public virtual void InitializeUI(Camera InUICamera)
        {
            UpdateRotation();
            m_UICamera = InUICamera;
            uiContainer.worldCamera = m_UICamera;
            ClearAbilityList();
            uiContainer.gameObject.SetActive(false);
        }

        private void UpdateRotation()
        {
            if (m_UICamera)
            {
                uiContainer.transform.rotation = m_UICamera.transform.rotation;
            }
        }

        protected void HandleUnitSelected(GridPawnUnit InUnit)
        {
            if(InUnit)
            {
                m_SelectedUnit = InUnit;
                SetupAbilityList();
                HandleUnitPostSelected(true);
            }
            else
            {
                ClearAbilityList();
                HandleUnitPostSelected(false);
                m_SelectedUnit = null;
            }
        }

        protected virtual void HandleUnitPostSelected(bool bIsSelected)
        {
            // Do nothing
        }

        private void SetupAbilityList()
        {
            if (!m_SelectedUnit)
            {
                return;
            }

            var abilities = m_SelectedUnit.GetAbilities();
            int requiredCount = abilities.Count;

            while (m_UIAbilities.Count < requiredCount)
            {
                var newAbilityUI = Instantiate(abilityUIElementPrefab, listContainer);
                m_UIAbilities.Add(newAbilityUI);
            }

            for (int i = 0; i < requiredCount; i++)
            {
                var abilityUI = m_UIAbilities[i];
                var ability = abilities[i];

                if (ability)
                {
                    SetupAbilitySlot(abilityUI, ability, i);
                }
                else
                {
                    abilityUI.gameObject.SetActive(false);
                }
            }

            for (int i = requiredCount; i < m_UIAbilities.Count; i++)
            {
                m_UIAbilities[i].gameObject.SetActive(false);
            }
        }

        protected virtual void SetupAbilitySlot(SlotDataUIElementBase slot, 
            UnitAbilityCore ability, int InIndex)
        {
            slot.gameObject.SetActive(true);
            slot.SetOwner(this);
            slot.SetAbility(ability, InIndex);
        }

        private void ClearAbilityList()
        {
            foreach (SlotDataUIElementBase abilityUI in m_UIAbilities)
            {
                if (abilityUI)
                {
                    ClearAbilitySlot(abilityUI);
                }
            }
        }

        protected virtual void ClearAbilitySlot(SlotDataUIElementBase slot)
        {
            slot.SetOwner(this);
            slot.ClearAbility();
            slot.gameObject.SetActive(false);
        }

        public void HandleAbilitySelected(int InIndex)
        {
            if(m_SelectedUnit)
            {
                m_SelectedUnit.SetupAbility(InIndex);
            }
        }
    }
}
