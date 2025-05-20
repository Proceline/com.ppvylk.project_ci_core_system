using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.PlayerData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay
{
    public class TacticStaticBattleManager : TacticBattleManager
    {
        [SerializeField]
        private LevelGridBase m_LevelGrid;

        protected override LevelGridBase LevelGrid => m_LevelGrid;

        [Space(10)]
        [Header("Team Data")]

        [SerializeField]
        private HumanTeamData m_FriendlyTeamData;

        [SerializeField]
        private TeamData m_HostileTeamData;

        protected override HumanTeamData FriendlyTeamData => m_FriendlyTeamData;

        protected override TeamData HostileTeamData => m_HostileTeamData;

        protected override void Start()
        {
            if (FriendlyTeamData)
            {
                FriendlyTeamData.SetTeam(GameTeam.Friendly);
            }
            else
            {
                Debug.Log("([ProjectCI]::TacticStaticBattleManager::Start) Missing Friendly Team Data");
            }

            if (HostileTeamData)
            {
                HostileTeamData.SetTeam(GameTeam.Hostile);
            }
            else
            {
                Debug.Log("([ProjectCI]::TacticStaticBattleManager::Start) Missing Hostile Team Data");
            }

            base.Start();
        }
    }
}