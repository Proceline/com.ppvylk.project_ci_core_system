using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "DefeatNumberOfOpponents", menuName = "TurnBasedTools/WinCondition/Create DefeatNumberOfOpponents", order = 1)]
    public class DefeatNumberOfOpponents_WinCondition : WinCondition
    {
        public int m_NumberRequired;

        protected override bool DidTeamWin(GameTeam InTeam)
        {
            if (InTeam == GameTeam.Friendly)
            {
                return GameManager.NumUnitsKilled(GameTeam.Hostile) == m_NumberRequired;
            }
            else if (InTeam == GameTeam.Hostile)
            {
                return GameManager.NumUnitsKilled(GameTeam.Friendly) == m_NumberRequired;
            }

            return false;
        }
        
        public override string GetConditionStateString()
        {
            GameTeam TargetTeam = GameTeam.Friendly;

            GameTeam CurrentTeam = GameManager.GetRules().GetCurrentTeam();

            if(GameManager.IsTeamHuman(CurrentTeam))
            {
                TargetTeam = (CurrentTeam == GameTeam.Friendly ? GameTeam.Hostile : GameTeam.Friendly);
            }

            int numTargets = GameManager.GetUnitsOnTeam(TargetTeam).Count;
            int numTargetsKilled = GameManager.NumUnitsKilled(TargetTeam);

            return "(" + numTargetsKilled + "/" + m_NumberRequired + ")";
        }
    }
}
