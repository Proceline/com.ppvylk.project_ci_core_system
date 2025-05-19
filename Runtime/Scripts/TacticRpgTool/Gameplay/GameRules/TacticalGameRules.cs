using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.PlayerData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules
{
    [CreateAssetMenu(fileName = "NewGameRules", menuName = "TurnBasedTools/GameRules/Create TacticalGameRules", order = 1)]
    public class TacticalGameRules : BattleGameRules
    {
        [SerializeField]
        bool m_bManuallyPlaceUnits;

        bool m_bSelectingPoints = false;

        TeamUnitSpawner CurrentSpawner;
        GameObject m_CurrentHoverObject;

        GridUnit SelectedUnit = null;
        [SerializeField] private InputActionReference cancelAction;

        protected override void Init()
        {
            SetupCellSpawns();
            cancelAction.action.performed += CancelActionPerformed;
            // TODO: Add num pressed event
            // HandleNumPressed
        }
        
        void SetupCellSpawns()
        {
            SpawnUnits( GameTeam.Hostile, HandleEnemyUnitsSpawned );
        }

        void SpawnUnits(GameTeam InTeam, UnityAction OnComplete)
        {
            m_bSelectingPoints = true;

            UnityEvent OnSpawnerComplete = new UnityEvent();
            OnSpawnerComplete.AddListener(OnComplete);

            HumanTeamData HumanData = GameManager.GetDataForTeam<HumanTeamData>( InTeam );
            if( HumanData )
            {
                List<ILevelCell> SpawnList = GameManager.GetGrid().GetTeamStartPoints( InTeam );

                if(InTeam == GameTeam.Friendly)
                {
                    foreach (ILevelCell levelCell in SpawnList)
                    {
                        if(levelCell)
                        {
                            levelCell.SetVisible(true);
                        }
                    }
                }

                if( m_bManuallyPlaceUnits )
                {
                    CurrentSpawner = new GameObject( "UnitSpawner" ).AddComponent<TeamUnitSpawner>();
                    CurrentSpawner.Init( HumanData, SpawnList, OnSpawnerComplete );
                    CurrentSpawner.StartSpawning();
                }
                else
                {
                    AutoPlaceUnits( HumanData, SpawnList );
                    OnSpawnerComplete.Invoke();
                }
            }
            else
            {
                SpawnAIUnits( InTeam );
                OnSpawnerComplete.Invoke();
            }
        }

        void SpawnAIUnits(GameTeam InTeam)
        {
            AITeamData AIData = GameManager.GetDataForTeam<AITeamData>( InTeam );
            if ( AIData )
            {
                foreach ( AIObjectSpawnInfo ObjInfo in AIData.m_AISpawnUnits )
                {
                    List<ILevelCell> CellsToSpawnAt = GameManager.GetGrid().GetCellsById(ObjInfo.m_SpawnAtCellId);

                    if(CellsToSpawnAt.Count > 0)
                    {

                        int selectedIndex = Random.Range(0, CellsToSpawnAt.Count - 1);
                        ILevelCell selectedCell = CellsToSpawnAt[ selectedIndex ];
                        if( selectedCell )
                        {
                            GridUnit SpawnedUnit = GameManager.SpawnUnit(ObjInfo.m_UnitData, InTeam, selectedCell.GetIndex(), ObjInfo.m_StartDirection);
                            SpawnedUnit.SetAsTarget(ObjInfo.m_bIsATarget);
                            SpawnedUnit.AddAI(ObjInfo.m_AssociatedAI);
                        }
                    }
                    else
                    {
                        Debug.Log("([TurnBasedTools]::TacticalGameRules::SpawnAIUnits) Couldn't find a cell for the AI unit: \"" + ObjInfo.m_UnitData.m_UnitName + "\" to spawn at. Either the m_SpawnAtCellId(" + ObjInfo.m_SpawnAtCellId + ") isn't set, or no cells are tagged.");
                    }


                }
            }
        }

        void AutoPlaceUnits(HumanTeamData InHumanData, List<ILevelCell> InSpawnList)
        {
            int NumStartPoints = InSpawnList.Count;

            int NumInRoster = InHumanData.m_UnitRoster.Count;
            for (int i = 0; i < NumInRoster; i++)
            {
                HumanUnitSpawnInfo HumanSpawnInfo = InHumanData.m_UnitRoster[i];
                if ( HumanSpawnInfo.m_UnitData )
                {
                    List<ILevelCell> spawnCells = GameManager.GetGrid().GetCellsById(HumanSpawnInfo.m_SpawnAtCellId);

                    List<ILevelCell> cellsToRemove = new List<ILevelCell>();
                    foreach (ILevelCell cell in spawnCells)
                    {
                        if (cell)
                        {
                            if (cell.IsObjectOnCell())
                            {
                                cellsToRemove.Add(cell);
                            }
                        }
                    }

                    foreach (ILevelCell cell in cellsToRemove)
                    {
                        spawnCells.Remove(cell);
                    }

                    if(spawnCells.Count > 0 && HumanSpawnInfo.m_SpawnAtCellId != "")
                    {
                        int selectedIndex = Random.Range(0, spawnCells.Count - 1);
                        ILevelCell selectedCell = spawnCells[selectedIndex];
                    
                        if(InSpawnList.Contains(selectedCell))
                        {
                            InSpawnList.Remove(selectedCell);
                        }
                    
                        GridUnit SpawnedUnit = GameManager.SpawnUnit(HumanSpawnInfo.m_UnitData, InHumanData.GetTeam(), selectedCell.GetIndex(), HumanSpawnInfo.m_StartDirection);
                        if (SpawnedUnit)
                        {
                            SpawnedUnit.SetAsTarget(HumanSpawnInfo.m_bIsATarget);
                        }
                    }
                    else if(i < NumStartPoints)
                    {
                            int selectedCellIndex = Random.Range(0, InSpawnList.Count);
                            ILevelCell selectedCell = InSpawnList[selectedCellIndex];
                            InSpawnList.RemoveAt(selectedCellIndex);
                            GridUnit SpawnedUnit = GameManager.SpawnUnit(HumanSpawnInfo.m_UnitData, InHumanData.GetTeam(), selectedCell.GetIndex(), HumanSpawnInfo.m_StartDirection);
                            if (SpawnedUnit)
                            {
                                SpawnedUnit.SetAsTarget(HumanSpawnInfo.m_bIsATarget);
                            }
                    }
                    else
                    {
                        Debug.Log("([TurnBasedTools]::TacticalGameRules::AutoPlaceUnits) Ran out of points to spawn at, can't spawn all units. Either there are not enough team spawn points, or it's not set up properly on the grid or on the TeamData asset");
                    }
                }
            }
        }

        void HandleEnemyUnitsSpawned()
        {
            CurrentSpawner = null;
            SpawnUnits( GameTeam.Friendly, HandleAllUnitsSpawned );
        }

        void HandleAllUnitsSpawned()
        {
            CurrentSpawner = null;
            m_bSelectingPoints = false;
            StartGame();
        }

        void CleanUpSelectedUnit()
        {
            if (SelectedUnit)
            {
                SelectedUnit.UnBindFromOnMovementComplete(UpdateSelectedHoverObject);
                UnselectUnit();
            }
        }

        void SetupTeam(GameTeam InTeam)
        {
            List<GridUnit> Units = GameManager.GetUnitsOnTeam(InTeam);
            foreach (GridUnit unit in Units)
            {
                unit.HandleTurnStarted();
            }
        }

        bool IsHoverObjectSpawned()
        {
            return m_CurrentHoverObject != null;
        }

        void UpdateSelectedHoverObject()
        {
            if (m_CurrentHoverObject)
            {
                Destroy(m_CurrentHoverObject);
            }

            if ( SelectedUnit && !SelectedUnit.IsDead() )
            {
                GameObject hoverObj = GameManager.GetSelectedHoverPrefab();
                if (hoverObj)
                {
                    m_CurrentHoverObject = Instantiate(hoverObj, SelectedUnit.GetCell().GetAllignPos(SelectedUnit), hoverObj.transform.rotation);
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if(!SelectedUnit && IsHoverObjectSpawned())
            {
                UpdateSelectedHoverObject();
            }
        }

        public void UnselectUnit()
        {
            if (SelectedUnit)
            {
                SelectedUnit.CleanUp();
                SelectedUnit = null;

                GameManager.Get().OnUnitSelected.Invoke(SelectedUnit);

                UpdateSelectedHoverObject();
            }

            GameManager.Get().UpdateHoverCells();
        }

        public override GridUnit GetSelectedUnit()
        {
            return SelectedUnit;
        }

        public override void HandleNumPressed(int InNumPressed)
        {
            if(m_bSelectingPoints)
            {
                return;
            }

            if(SelectedUnit)
            {
                SelectedUnit.SetupAbility(InNumPressed - 1);
            }
        }

        public override void BeginTeamTurn(GameTeam InTeam)
        {
            CleanUpSelectedUnit();
            SetupTeam(InTeam);
            
            AilmentHandler.HandleTurnStart(InTeam);

            if(InTeam == GameTeam.Hostile)
            {
                bool bIsHostileTeamAI = GameManager.IsTeamAI(GameTeam.Hostile);
                if( bIsHostileTeamAI )
                {
                    List<GridUnit> AIUnits = GameManager.GetUnitsOnTeam(GameTeam.Hostile);
                    AIManager.RunAI( AIUnits, EndTurn );
                }
            }
        }

        public override void EndTeamTurn(GameTeam InTeam)
        {
            AilmentHandler.HandleTurnEnd(InTeam);
        }
        
        public override void HandleEnemySelected(GridUnit InEnemyUnit)
        {
            
        }

        public override void HandlePlayerSelected(GridUnit InPlayerUnit)
        {
            if (m_bSelectingPoints)
            {
                return;
            }

            if (GameManager.IsActionBeingPerformed())
            {
                return;
            }

            GameTeam currTeam = GetCurrentTeam();
            if(currTeam == InPlayerUnit.GetTeam())
            {

                if(SelectedUnit)
                {
                    if(SelectedUnit.GetCurrentState() == UnitState.UsingAbility)
                    {
                        return;
                    }

                    CleanUpSelectedUnit();
                }

                SelectedUnit = InPlayerUnit;

                if(SelectedUnit)
                {
                    SelectedUnit.SelectUnit();
                    SelectedUnit.BindToOnMovementComplete(UpdateSelectedHoverObject);
                    GameManager.Get().OnUnitSelected.Invoke(SelectedUnit);
                }

                UpdateSelectedHoverObject();
            }
        }

        public override void HandleCellSelected(ILevelCell InCell)
        {

            if (GameManager.IsActionBeingPerformed())
            {
                return;
            }

            if (m_bSelectingPoints)
            {
                if(CurrentSpawner)
                {
                    CurrentSpawner.HandleTileSelected(InCell);
                }
            }
            else
            {
                if(SelectedUnit)
                {
                    UnitState currentState = SelectedUnit.GetCurrentState();

                    if (currentState == UnitState.Moving)
                    {
                        SelectedUnit.ExecuteMovement(InCell);
                    }
                    else if(currentState == UnitState.UsingAbility)
                    {
                        SelectedUnit.ExecuteAbility(InCell);
                    }
                }
            }
        }

        public override void HandleTeamWon(GameTeam InTeam)
        {
            UnselectUnit();
        }

        private void CancelActionPerformed(InputAction.CallbackContext context)
        {
            
            if (GameManager.IsActionBeingPerformed())
            {
                return;
            }

            if(m_bSelectingPoints)
            {
                return;
            }

            if (GameManager.IsPlaying() && SelectedUnit)
            {
                UnitState currentState = SelectedUnit.GetCurrentState();
                if (currentState == UnitState.UsingAbility)
                {
                    SelectedUnit.SetupMovement();
                }
                else if (currentState == UnitState.Moving)
                {
                    UnselectUnit();
                }
            }
        }
    }
}
