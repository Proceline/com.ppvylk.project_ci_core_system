using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "DefeatAllOpponents", menuName = "ProjectCI Tools/WinCondition/Create DefeatAllOpponents", order = 1)]
    public class DefeatAllOpponents_WinCondition : WinCondition
    {
        protected override bool DidTeamWin(GameTeam InTeam)
        {
            if (InTeam == GameTeam.Friendly)
            {
                return TacticBattleManager.AreAllUnitsOnTeamDead(GameTeam.Hostile);
            }
            else if (InTeam == GameTeam.Hostile)
            {
                return TacticBattleManager.AreAllUnitsOnTeamDead(GameTeam.Friendly);
            }

            return false;
        }

        public override string GetConditionStateString()
        {
            GameTeam TargetTeam = GameTeam.Friendly;

            GameTeam CurrentTeam = TacticBattleManager.GetRules().GetCurrentTeam();

            if (TacticBattleManager.IsTeamHuman(CurrentTeam))
            {
                TargetTeam = (CurrentTeam == GameTeam.Friendly ? GameTeam.Hostile : GameTeam.Friendly);
            }

            int numTargets = TacticBattleManager.GetUnitsOnTeam(TargetTeam).Count;
            int numTargetsKilled = TacticBattleManager.NumUnitsKilled(TargetTeam);

            return "(" + numTargetsKilled + "/" + numTargets + ")";
        }
    }
}
