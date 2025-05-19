using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "DefeatAllTargets", menuName = "TurnBasedTools/WinCondition/Create DefeatAllTargets", order = 1)]
    public class DefeatAllTargets_WinCondition : WinCondition
    {
        protected override bool DidTeamWin(GameTeam InTeam)
        {
            if (InTeam == GameTeam.Friendly)
            {
                return TacticBattleManager.KilledAllTargets(GameTeam.Hostile);
            }
            else if (InTeam == GameTeam.Hostile)
            {
                return TacticBattleManager.KilledAllTargets(GameTeam.Friendly);
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

            int numTargets = TacticBattleManager.GetNumOfTargets(TargetTeam);
            int numTargetsKilled = TacticBattleManager.GetNumTargetsKilled(TargetTeam);

            return "(" + numTargetsKilled + "/" + numTargets + ")";
        }
    }
}
