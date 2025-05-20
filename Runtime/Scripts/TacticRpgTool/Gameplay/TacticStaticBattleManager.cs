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

        public override LevelGridBase LevelGrid => m_LevelGrid;

        [Space(10)]
        [Header("Team Data")]

        [SerializeField]
        private HumanTeamData m_FriendlyTeamData;

        [SerializeField]
        private TeamData m_HostileTeamData;

        public override HumanTeamData FriendlyTeamData => m_FriendlyTeamData;

        public override TeamData HostileTeamData => m_HostileTeamData;

        private void Start()
        {
            if (!LevelGrid)
            {
                Debug.Log("([ProjectCI]::TacticBattleManager::Start) Missing Grid");
            }

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

            if (!m_GameRules)
            {
                Debug.Log("([ProjectCI]::TacticBattleManager::Start) Missing GameRules");
            }

            if (m_WinConditions.Length == 0)
            {
                Debug.Log("([ProjectCI]::TacticBattleManager::Start) Missing WinConditions");
            }

            Initialize();
        }

        public static TeamData GetDataForTeam(GameTeam InTeam)
        {
            switch (InTeam)
            {
                case GameTeam.Friendly:
                    return GetFriendlyTeamData();
                case GameTeam.Hostile:
                    return GetHostileTeamData();
            }

            Debug.Log("([ProjectCI]::TacticBattleManager::GetDataForTeam) Trying to get TeamData for invalid team: " + InTeam.ToString());
            return new TeamData();
        }
    }
}