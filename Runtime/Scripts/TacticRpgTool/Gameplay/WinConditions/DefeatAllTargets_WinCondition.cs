using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "DefeatAllTargets", menuName = "ProjectCI Tools/WinCondition/Create DefeatAllTargets", order = 1)]
    public class DefeatAllTargets_WinCondition : WinCondition
    {
        protected override bool DidTeamWin(BattleTeam InTeam)
        {
            if (InTeam == BattleTeam.Friendly)
            {
                return TacticBattleManager.KilledAllTargets(BattleTeam.Hostile);
            }
            else if (InTeam == BattleTeam.Hostile)
            {
                return TacticBattleManager.KilledAllTargets(BattleTeam.Friendly);
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

            int numTargets = TacticBattleManager.GetNumOfTargets(TargetTeam);
            int numTargetsKilled = TacticBattleManager.GetNumTargetsKilled(TargetTeam);

            return "(" + numTargetsKilled + "/" + numTargets + ")";
        }
    }
}
