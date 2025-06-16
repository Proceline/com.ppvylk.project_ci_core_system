using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules
{
    public class TeamTurnChangeEvent : UnityEvent<BattleTeam> { }

    [System.Serializable]
    public struct GameplayData
    {
        // If this is true, then hit animations will play if you get damaged while moving.
        [SerializeField]
        public bool bShowHitAnimOnMove;
    }

    public abstract class BattleGameRules : ScriptableObject
    {
        public BattleTeam m_StartingTeam;

        [SerializeField]
        GameplayData m_GameplayData;

        protected BattleTeam m_CurrentTeam;
        protected int m_TurnNumber = 0;

        public TeamTurnChangeEvent OnTeamTurnBegin = new TeamTurnChangeEvent();

        public BattleTeam GetCurrentTeam()
        {
            return m_CurrentTeam;
        }

        public int GetTurnNumber()
        {
            return m_TurnNumber;
        }

        public virtual void InitalizeRules()
        {
            m_TurnNumber = 0;
            m_CurrentTeam = BattleTeam.All;
            StartGame();
        }

        public GameplayData GetGameplayData()
        {
            return m_GameplayData;
        }

        protected abstract void StartGame();

        public void EndTurn()
        {
            EndTeamTurn(m_CurrentTeam);
            
            if(m_CurrentTeam == BattleTeam.Friendly)
            {
                m_CurrentTeam = BattleTeam.Hostile;
                m_TurnNumber++;
            }
            else if (m_CurrentTeam == BattleTeam.Hostile)
            {
                m_CurrentTeam = BattleTeam.Friendly;
            }

            if (!TacticBattleManager.GetTeamList().Contains(m_CurrentTeam))
            {
                EndTurn();
            }
            else
            {
                OnTeamTurnBegin.Invoke(m_CurrentTeam);
                BeginTeamTurn(m_CurrentTeam);
            }
        }
        
        public virtual void Update()
        {
            
        }

        public virtual GridPawnUnit GetSelectedUnit()
        {
            return null;
        }
        
        public virtual List<LevelCellBase> GetAbilityHoverCells(LevelCellBase InCell)
        {
            return new();
        }

        public virtual void BeginTeamTurn(BattleTeam InTeam)
        {
            
        }

        public virtual void EndTeamTurn(BattleTeam InTeam)
        {

        }

        public virtual void HandlePlayerSelected(GridPawnUnit InPlayerUnit)
        {

        }

        public virtual void HandleEnemySelected(GridPawnUnit InEnemyUnit)
        {

        }
        
        public virtual void HandleCellSelected(LevelCellBase InCell)
        {

        }

        public virtual void HandleNumPressed(int InNumPressed)
        {
            
        }

        public virtual void HandleTeamWon(BattleTeam InTeam)
        {
            
        }

        public abstract void CancelActionExtension(InputAction.CallbackContext context);

    }
}
