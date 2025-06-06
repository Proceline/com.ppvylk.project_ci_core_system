﻿using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "SurviveNumberOfTurns", menuName = "ProjectCI Tools/WinCondition/Create SurviveNumberOfTurns", order = 1)]
    public class SurviveNumberOfTurns_WinCondition : WinCondition
    {
        public int m_NumberRequired;

        protected override bool DidTeamWin(BattleTeam InTeam)
        {
            return TacticBattleManager.GetRules().GetTurnNumber() >= m_NumberRequired;
        }

        protected override bool DidTeamLose(BattleTeam InTeam)
        {
            bool bAllUnitsDead = TacticBattleManager.AreAllUnitsOnTeamDead(InTeam);
            return bAllUnitsDead;
        }

        public override string GetConditionStateString()
        {
            int turnNumber = TacticBattleManager.GetRules().GetTurnNumber();
            return turnNumber + "/" + m_NumberRequired;
        }
    }
}
