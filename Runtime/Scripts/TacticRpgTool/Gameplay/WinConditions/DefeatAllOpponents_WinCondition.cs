using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "DefeatAllOpponents", menuName = "TurnBasedTools/WinCondition/Create DefeatAllOpponents", order = 1)]
    public class DefeatAllOpponents_WinCondition : WinCondition
    {
        protected override bool DidTeamWin(GameTeam InTeam)
        {
            if (InTeam == GameTeam.Friendly)
            {
                return GameManager.AreAllUnitsOnTeamDead(GameTeam.Hostile);
            }
            else if (InTeam == GameTeam.Hostile)
            {
                return GameManager.AreAllUnitsOnTeamDead(GameTeam.Friendly);
            }

            return false;
        }

        public override string GetConditionStateString()
        {
            GameTeam TargetTeam = GameTeam.Friendly;

            GameTeam CurrentTeam = GameManager.GetRules().GetCurrentTeam();

            if (GameManager.IsTeamHuman(CurrentTeam))
            {
                TargetTeam = (CurrentTeam == GameTeam.Friendly ? GameTeam.Hostile : GameTeam.Friendly);
            }

            int numTargets = GameManager.GetUnitsOnTeam(TargetTeam).Count;
            int numTargetsKilled = GameManager.NumUnitsKilled(TargetTeam);

            return "(" + numTargetsKilled + "/" + numTargets + ")";
        }
    }
}
