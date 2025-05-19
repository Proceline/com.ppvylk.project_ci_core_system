using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "SurviveNumberOfTurns", menuName = "TurnBasedTools/WinCondition/Create SurviveNumberOfTurns", order = 1)]
    public class SurviveNumberOfTurns_WinCondition : WinCondition
    {
        public int m_NumberRequired;

        protected override bool DidTeamWin(GameTeam InTeam)
        {
            return GameManager.GetRules().GetTurnNumber() >= m_NumberRequired;
        }

        protected override bool DidTeamLose(GameTeam InTeam)
        {
            bool bAllUnitsDead = GameManager.AreAllUnitsOnTeamDead(InTeam);
            return bAllUnitsDead;
        }

        public override string GetConditionStateString()
        {
            int turnNumber = GameManager.GetRules().GetTurnNumber();
            return turnNumber + "/" + m_NumberRequired;
        }
    }
}
