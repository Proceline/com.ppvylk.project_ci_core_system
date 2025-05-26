using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GUI
{
    public class HoverUnitInfoUIElementBase : MonoBehaviour
    {
        public GameObject m_ScreenObject;

        [Space(5)]

        public Text m_UnitName;

        [Space(5)]

        public Text m_HealthText;
        public Text m_ArmorText;
        public Text m_MagicArmorText;
        public Text m_APText;

        [Space(5)]

        public Slider m_HealthSlider;
        public Slider m_ArmorSlider;
        public Slider m_MagicArmorSlider;
        public Slider m_APSlider;

        GridPawnUnit m_CurrUnit = null;

        bool bEnabled = true;

        private void Awake()
        {
            SetupScreen();
        }

        private void Start()
        {
            TacticBattleManager.Get().OnUnitHover.AddListener(HandleUnitHover);
            TacticBattleManager.Get().OnTeamWon.AddListener(HandleGameDone);
        }

        private void Update()
        {
            SetupScreen();
        }

        private void HandleUnitHover(GridPawnUnit InUnit)
        {
            if ( m_CurrUnit )
            {
                BattleHealth hpComp = m_CurrUnit.GetComponent<BattleHealth>();
                if ( hpComp )
                {
                    hpComp.OnHealthDepleted.RemoveListener( HandleUnitDeath );
                }
            }

            m_CurrUnit = InUnit;

            if ( m_CurrUnit )
            {
                BattleHealth hpComp = m_CurrUnit.GetComponent<BattleHealth>();
                if ( hpComp )
                {
                    hpComp.OnHealthDepleted.AddListener( HandleUnitDeath );
                }
            }

            SetupScreen();
        }

        void HandleUnitDeath()
        {
            HandleUnitHover( null );
        }

        void SetupScreen()
        {
            if(m_CurrUnit && bEnabled)
            {
                m_ScreenObject.SetActive(true);

                SoUnitData unitData = m_CurrUnit.GetUnitData();

                int CurrAbilityPoints = m_CurrUnit.GetCurrentAbilityPoints();
                int TotalAbilityPoints = unitData.m_AbilityPoints;

                m_UnitName.text = unitData.m_UnitName;
                m_APText.text = CurrAbilityPoints.ToString();

                float APPercentage = ( (float)CurrAbilityPoints ) / ( (float)TotalAbilityPoints );
                m_APSlider.value = APPercentage;

                BattleHealth healthComp = m_CurrUnit.GetComponent<BattleHealth>();
                if(healthComp)
                {
                    m_HealthText.text = healthComp.GetHealth().ToString();
                    m_HealthSlider.value = healthComp.GetHealthPercentage();
                }
            }
            else
            {
                m_ScreenObject.SetActive(false);
            }
        }


        void HandleGameDone(BattleTeam InWinningTeam)
        {
            m_ScreenObject.SetActive(false);
            bEnabled = false;
        }
    }
}
