using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "DefeatNumberOfOpponents", menuName = "ProjectCI Tools/WinCondition/Create DefeatNumberOfOpponents", order = 1)]
    public class DefeatNumberOfOpponents_WinCondition : WinCondition
    {
        public int m_NumberRequired;

        protected override bool DidTeamWin(GameTeam InTeam)
        {
            if (InTeam == GameTeam.Friendly)
            {
                return TacticBattleManager.NumUnitsKilled(GameTeam.Hostile) == m_NumberRequired;
            }
            else if (InTeam == GameTeam.Hostile)
            {
                return TacticBattleManager.NumUnitsKilled(GameTeam.Friendly) == m_NumberRequired;
            }

            return false;
        }
        
        public override string GetConditionStateString()
        {
            GameTeam TargetTeam = GameTeam.Friendly;

            GameTeam CurrentTeam = TacticBattleManager.GetRules().GetCurrentTeam();

            if(TacticBattleManager.IsTeamHuman(CurrentTeam))
            {
                TargetTeam = (CurrentTeam == GameTeam.Friendly ? GameTeam.Hostile : GameTeam.Friendly);
            }

            int numTargets = TacticBattleManager.GetUnitsOnTeam(TargetTeam).Count;
            int numTargetsKilled = TacticBattleManager.NumUnitsKilled(TargetTeam);

            return "(" + numTargetsKilled + "/" + m_NumberRequired + ")";
        }
    }
}
