using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "DefeatAllOpponents", menuName = "ProjectCI Tools/WinCondition/Create DefeatAllOpponents", order = 1)]
    public class DefeatAllOpponents_WinCondition : WinCondition
    {
        protected override bool DidTeamWin(BattleTeam InTeam)
        {
            if (InTeam == BattleTeam.Friendly)
            {
                return TacticBattleManager.AreAllUnitsOnTeamDead(BattleTeam.Hostile);
            }
            else if (InTeam == BattleTeam.Hostile)
            {
                return TacticBattleManager.AreAllUnitsOnTeamDead(BattleTeam.Friendly);
            }

            return false;
        }

        public override string GetConditionStateString()
        {
            BattleTeam TargetTeam = BattleTeam.Friendly;

            BattleTeam CurrentTeam = TacticBattleManager.GetRules().GetCurrentTeam();

            if (TacticBattleManager.IsTeamHuman(CurrentTeam))
            {
                TargetTeam = (CurrentTeam == BattleTeam.Friendly ? BattleTeam.Hostile : BattleTeam.Friendly);
            }

            int numTargets = TacticBattleManager.GetUnitsOnTeam(TargetTeam).Count;
            int numTargetsKilled = TacticBattleManager.NumUnitsKilled(TargetTeam);

            return "(" + numTargetsKilled + "/" + numTargets + ")";
        }
    }
}
