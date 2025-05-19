using System.Collections;
using System.Collections.Generic;
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

        public bool CheckTeamWin(GameTeam InTeam)
        {
            if(!AllowsTeam(InTeam))
            {
                return false;
            }

            return DidTeamWin(InTeam);
        }

        public bool CheckTeamLost(GameTeam InTeam)
        {
            if (!AllowsTeam(InTeam))
            {
                return false;
            }

            return DidTeamLose(InTeam);
        }

        protected virtual bool DidTeamWin(GameTeam InTeam)
        {
            return false;
        }

        protected virtual bool DidTeamLose(GameTeam InTeam)
        {
            return GameManager.AreAllUnitsOnTeamDead(InTeam);
        }

        bool AllowsTeam(GameTeam InTeam)
        {
            switch (InTeam)
            {
                case GameTeam.Friendly:
                    return m_bCheckFriendlyTeam;
                case GameTeam.Hostile:
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
