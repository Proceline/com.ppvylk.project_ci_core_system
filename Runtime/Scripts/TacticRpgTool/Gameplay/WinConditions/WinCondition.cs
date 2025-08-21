using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    public class WinCondition : ScriptableObject
    {
        public string m_ConditionName;
        public string m_Description;
        public Texture2D m_Icon;

        public bool m_bCheckWinFirst = true;

        [SerializeField]
        protected bool m_bCheckFriendlyTeam = true;

        [SerializeField]
        protected bool m_bCheckHostileTeam = true;

        public bool CheckTeamWin(BattleTeam InTeam)
        {
            if(!AllowsTeam(InTeam))
            {
                return false;
            }

            return DidTeamWin(InTeam);
        }

        public bool CheckTeamLost(BattleTeam InTeam)
        {
            if (!AllowsTeam(InTeam))
            {
                return false;
            }

            return DidTeamLose(InTeam);
        }

        protected virtual bool DidTeamWin(BattleTeam InTeam)
        {
            return false;
        }

        protected virtual bool DidTeamLose(BattleTeam InTeam)
        {
            return TacticBattleManager.AreAllUnitsOnTeamDead(InTeam);
        }

        bool AllowsTeam(BattleTeam InTeam)
        {
            switch (InTeam)
            {
                case BattleTeam.Friendly:
                    return m_bCheckFriendlyTeam;
                case BattleTeam.Hostile:
                    return m_bCheckHostileTeam;
            }

            return false;
        }

        //The data that shows along in the UI, EX.) Kill All targets would return "(3/4)"
        public virtual string GetConditionStateString()
        {
            return "";
        }
    }
}
