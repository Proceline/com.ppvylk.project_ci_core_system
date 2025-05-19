using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Levels.Components;
using ProjectCI.CoreSystem.Runtime.Enums;
using ProjectCI.CoreSystem.Runtime.Units.Components;
using ProjectCI.CoreSystem.Runtime.Levels;

namespace ProjectCI.CoreSystem.Runtime.Core.Rules
{
    [System.Serializable]
    public class TeamTurnChangeEvent : UnityEvent<BattleTeam> { }

    public abstract class BattleRules : ScriptableObject
    {
        public BattleTeam StartingTeam;
        protected BattleTeam currentTeam;
        protected int turnNumber = 0;

        public TeamTurnChangeEvent OnTeamTurnBegin = new TeamTurnChangeEvent();

        protected virtual void Init()
        {
            StartGame();
        }

        public BattleTeam GetCurrentTeam()
        {
            return currentTeam;
        }

        public int GetTurnNumber()
        {
            return turnNumber;
        }

        public void InitializeRules()
        {
            turnNumber = 0;
            currentTeam = BattleTeam.Neutral;
            Init();
        }

        protected void StartGame()
        {
            currentTeam = StartingTeam;
            turnNumber = 0;
            BattleManager.HandleGameStarted();
            BeginTeamTurn(currentTeam);
        }

        public void EndTurn()
        {
            EndTeamTurn(currentTeam);
            
            if(currentTeam == BattleTeam.Player)
            {
                currentTeam = BattleTeam.Enemy;
                turnNumber++;
            }
            else if (currentTeam == BattleTeam.Enemy)
            {
                currentTeam = BattleTeam.Player;
            }

            if (!BattleManager.GetTeamList().Contains(currentTeam))
            {
                EndTurn();
            }
            else
            {
                OnTeamTurnBegin.Invoke(currentTeam);
                BeginTeamTurn(currentTeam);
            }
        }
        
        public abstract void Update();

        public abstract BattleUnit GetSelectedUnit();

        public virtual void BeginTeamTurn(BattleTeam team)
        {
        }

        public virtual void EndTeamTurn(BattleTeam team)
        {
        }

        public virtual void HandlePlayerSelected(IUnit playerUnit)
        {
        }

        public virtual void HandleEnemySelected(IUnit enemyUnit)
        {
        }
        
        public virtual void HandleCellSelected(BattleCell cell)
        {
        }

        public virtual void HandleNumPressed(int numPressed)
        {
        }

        public virtual void HandleTeamWon(BattleTeam team)
        {
        }
    }
} 