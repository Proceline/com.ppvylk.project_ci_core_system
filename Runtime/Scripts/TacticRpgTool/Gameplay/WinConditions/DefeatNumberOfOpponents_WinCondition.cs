using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "DefeatNumberOfOpponents", menuName = "ProjectCI Tools/WinCondition/Create DefeatNumberOfOpponents", order = 1)]
    public class DefeatNumberOfOpponents_WinCondition : WinCondition
    {
        public int m_NumberRequired;

        protected override bool DidTeamWin(BattleTeam InTeam)
        {
            if (InTeam == BattleTeam.Friendly)
            {
                return TacticBattleManager.NumUnitsKilled(BattleTeam.Hostile) == m_NumberRequired;
            }
            else if (InTeam == BattleTeam.Hostile)
            {
                return TacticBattleManager.NumUnitsKilled(BattleTeam.Friendly) == m_NumberRequired;
            }

            return false;
        }
        
        public override string GetConditionStateString()
        {
            BattleTeam TargetTeam = BattleTeam.Friendly;

            BattleTeam CurrentTeam = TacticBattleManager.GetRules().GetCurrentTeam();

            if(TacticBattleManager.IsTeamHuman(CurrentTeam))
            {
                TargetTeam = (CurrentTeam == BattleTeam.Friendly ? BattleTeam.Hostile : BattleTeam.Friendly);
            }

            int numTargets = TacticBattleManager.GetUnitsOnTeam(TargetTeam).Count;
            int numTargetsKilled = TacticBattleManager.NumUnitsKilled(TargetTeam);

            return "(" + numTargetsKilled + "/" + m_NumberRequired + ")";
        }
    }
}
