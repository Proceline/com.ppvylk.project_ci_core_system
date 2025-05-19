using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.PlayerData
{
    public class TeamUnitSpawner : MonoBehaviour
    {
        HumanTeamData m_TeamData;
        List<LevelCellBase> m_SpawnPoints;
        UnityEvent m_OnSpawnComplete;

        int m_CurrentSpawnIndex;
        int m_UnitsSpawned;

        bool bIsSpawning = false;

        public void Init(HumanTeamData InTeamData, List<LevelCellBase> InSpawnPoints, UnityEvent InOnSpawnComplete)
        {
            m_TeamData = InTeamData;
            m_SpawnPoints = InSpawnPoints;
            m_OnSpawnComplete = InOnSpawnComplete;

            foreach (LevelCellBase playerCell in m_SpawnPoints)
            {
                TacticBattleManager.SetCellState(playerCell, CellState.eMovement);
            }
        }

        public void StartSpawning()
        {
            m_CurrentSpawnIndex = 0;
            m_UnitsSpawned = 0;
            bIsSpawning = true;
        }

        public void Finish()
        {
            foreach (LevelCellBase playerCell in m_SpawnPoints)
            {
                TacticBattleManager.ResetCellState(playerCell);
            }

            bIsSpawning = false;
            m_OnSpawnComplete.Invoke();
            Destroy(gameObject);
        }

        void Start()
        {
            
        }

        void Update()
        {
            
        }

        public void HandleTileSelected(LevelCellBase InCell)
        {
            if (bIsSpawning)
            {
                if(m_SpawnPoints.Contains(InCell))
                {
                    if(InCell.IsObjectOnCell())
                    {
                        return;
                    }

                    int RosterCount = m_TeamData.m_UnitRoster.Count;
                    if(m_CurrentSpawnIndex < RosterCount)
                    {
                        HumanUnitSpawnInfo unitSpawnInfo = m_TeamData.m_UnitRoster[m_CurrentSpawnIndex];
                        if(unitSpawnInfo.m_UnitData)
                        {
                            GridUnit SpawnedUnit = TacticBattleManager.SpawnUnit( unitSpawnInfo.m_UnitData, m_TeamData.GetTeam(), InCell.GetIndex(), unitSpawnInfo.m_StartDirection );
                            if(SpawnedUnit)
                            {
                                SpawnedUnit.SetAsTarget(unitSpawnInfo.m_bIsATarget);
                            }

                            m_CurrentSpawnIndex++;
                            m_UnitsSpawned++;

                            TacticBattleManager.ResetCellState(InCell);

                            if (m_CurrentSpawnIndex >= RosterCount)
                            {
                                Finish();
                            }
                        }
                    }
                }
            }
        }
    }
}
