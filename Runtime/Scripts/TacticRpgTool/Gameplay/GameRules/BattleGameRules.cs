using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules
{
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
    }
}
