using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules
{
    public class TeamTurnChangeEvent : UnityEvent<BattleTeam> { }

    public abstract class BattleGameRules : ScriptableObject
    {
        public BattleTeam CurrentTeam { get; protected set; }

        public virtual void InitializeRules()
        {
            CurrentTeam = BattleTeam.All;
            StartGame();
        }
        
        protected abstract void StartGame();

        public abstract void Update();

        public abstract GridPawnUnit GetSelectedUnit();
        
        public abstract void BeginTeamTurn(BattleTeam inTeam);
        
        public abstract void HandlePlayerSelected(GridPawnUnit playerUnit);

        public abstract void HandleEnemySelected(GridPawnUnit enemyUnit);

        public abstract void HandleCellSelected(LevelCellBase cell);
    }
}
