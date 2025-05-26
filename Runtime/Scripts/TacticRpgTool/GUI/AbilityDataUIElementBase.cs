using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI
{
    [System.Serializable]
    public struct AbilityConnectionsInfo
    {
        [Header("Range")]
        public Text m_RangeNumText;

        [Header("Action Points")]
        public Text m_APNumText;

        [Header("Ability Pop-up")]
        public GameObject m_AbilityPopup;
        public Text m_AbilityNameText;
        public Text m_AbilityInfoText;
    }

    public class AbilityDataUIElementBase : MonoBehaviour
    {
        public UnitAbilityCore AbilityData { get; private set; }
        
        int m_Index;
        public AbilityConnectionsInfo m_Connections;

        AbilityListUIElementBase m_Owner;

        void Start()
        {
            SetPopupVisibility(false);
            SetupUIElement();

            TacticBattleManager.Get().OnTeamWon.AddListener(HandleGameDone);
        }

        public void SetOwner(AbilityListUIElementBase InListUIElem)
        {
            m_Owner = InListUIElem;
        }

        public void SetAbility(UnitAbilityCore InAbility, int InIndex)
        {
            AbilityData = InAbility;
            m_Index = InIndex;
            SetupUIElement();
        }

        public void ClearAbility()
        {
            AbilityData = null;

            m_Connections.m_RangeNumText.text = "";
            m_Connections.m_APNumText.text = "";

            m_Connections.m_AbilityNameText.text = "";
            m_Connections.m_AbilityInfoText.text = "";
        }

        void SetupUIElement()
        {
            if(AbilityData)
            {
                m_Connections.m_RangeNumText.text = AbilityData.GetRadius().ToString();
                m_Connections.m_APNumText.text = AbilityData.GetActionPointCost().ToString();

                m_Connections.m_AbilityNameText.text = AbilityData.GetAbilityName();
                m_Connections.m_AbilityInfoText.text = GenerateAbilityInfo();
            }
        }

        string GenerateAbilityInfo()
        {
            if(!AbilityData)
            {
                return "";
            }

            string abilityInfo = "";

            List<AbilityParamBase> AbilityParams = AbilityData.GetParameters();
            foreach ( AbilityParamBase param in AbilityParams )
            {
                if( param )
                {
                    abilityInfo += param.GetAbilityInfo() + "\n";
                }
            }

            List<Ailment> AbilityAilments = AbilityData.GetAilments();
            foreach (Ailment ailment in AbilityAilments)
            {
                if ( ailment )
                {
                    abilityInfo += "-" + ailment.m_Description + "\n";
                }
            }

            return abilityInfo;
        }

        void SetPopupVisibility(bool bInVisible)
        {
            if(bInVisible)
            {
                if(!AbilityData)
                {
                    return;
                }
            }

            m_Connections.m_AbilityPopup.SetActive(bInVisible);
            if(bInVisible)
            {
                SetupUIElement();
            }
        }

        void Update()
        {

        }

        public void OnClicked()
        {
            if(AbilityData)
            {
                if(m_Owner)
                {
                    m_Owner.HandleAbilitySelected(m_Index);
                }
            }
        }

        public void OnHover(bool bStart)
        {
            SetPopupVisibility(bStart);
        }

        private void HandleGameDone(BattleTeam InWinningTeam)
        {
            gameObject.SetActive(false);
        }
    }
}
