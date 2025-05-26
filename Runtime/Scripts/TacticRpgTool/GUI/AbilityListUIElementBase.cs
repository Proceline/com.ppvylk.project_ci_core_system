using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI
{
    /// <summary>
    /// Add it to SpawnOnStart in TacticBattleManager
    /// </summary>
    public class AbilityListUIElementBase : MonoBehaviour
    {
        [NonSerialized]
        private GridPawnUnit m_SelectedUnit;

        [SerializeField]
        private Canvas uiContainer;

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

        public void InitializeUI(Camera InUICamera)
        {
            UpdateRotation();
            m_UICamera = InUICamera;
            uiContainer.worldCamera = m_UICamera;
            ClearAbilityList();
            TacticBattleManager.Get().OnUnitSelected.AddListener(HandleUnitSelected);
            uiContainer.gameObject.SetActive(false);
        }

        private void UpdateRotation()
        {
            if (m_UICamera)
            {
                uiContainer.transform.rotation = m_UICamera.transform.rotation;
            }
        }

        private void OnDestroy()
        {
            TacticBattleManager.Get().OnUnitSelected.RemoveListener(HandleUnitSelected);
        }

        public void HandleUnitSelected(GridPawnUnit InUnit)
        {
            if(InUnit)
            {
                uiContainer.gameObject.SetActive(true);
                m_SelectedUnit = InUnit;
                SetupAbilityList();
            }
            else
            {
                ClearAbilityList();
                m_SelectedUnit = null;
                uiContainer.gameObject.SetActive(false);
            }
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
                    abilityUI.gameObject.SetActive(true);
                    abilityUI.SetOwner(this);
                    abilityUI.SetAbility(ability, i);
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

        private void ClearAbilityList()
        {
            foreach (SlotDataUIElementBase abilityUI in m_UIAbilities)
            {
                if (abilityUI)
                {
                    abilityUI.SetOwner(this);
                    abilityUI.ClearAbility();
                    abilityUI.gameObject.SetActive(false);
                }
            }
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
