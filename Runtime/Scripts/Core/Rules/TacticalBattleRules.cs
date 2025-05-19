using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Enums;
using UnityEngine.InputSystem;
using ProjectCI.CoreSystem.Runtime.Units.Components;
using ProjectCI.CoreSystem.Runtime.Levels;
using ProjectCI.CoreSystem.Runtime.Core.Teams;

namespace ProjectCI.CoreSystem.Runtime.Core.Rules
{
    [CreateAssetMenu(fileName = "NewBattleRules", menuName = "ProjectCI/BattleRules/Create TacticalBattleRules", order = 1)]
    public class TacticalBattleRules : BattleRules
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference selectAction;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference cancelAction;
        [SerializeField] private InputActionReference abilityAction;

        private bool isSelectingPoints = false;
        private GameObject currentHoverObject;
        private BattleUnit selectedUnit = null;

        protected override void Init()
        {
            SetupCellSpawns();
            cancelAction.action.canceled += HandleInputCancel;
        }
        
        private void SetupCellSpawns()
        {
            SpawnUnits(BattleTeam.Enemy, HandleEnemyUnitsSpawned);
        }

        private void SpawnUnits(BattleTeam team, UnityAction onComplete)
        {
            isSelectingPoints = true;

            UnityEvent onSpawnerComplete = new UnityEvent();
            onSpawnerComplete.AddListener(onComplete);

            BattleHumanTeamData humanData = BattleManager.GetDataForTeam<BattleHumanTeamData>(team);
            if(humanData != null)
            {
                List<BattleCell> spawnList = BattleManager.GetGrid().GetTeamStartPoints(team);

                if(team == BattleTeam.Player)
                {
                    foreach (BattleCell cell in spawnList)
                    {
                        if(cell != null)
                        {
                            cell.SetVisible(true);
                        }
                    }
                }
                
                AutoPlaceUnits(humanData, spawnList);
                onSpawnerComplete.Invoke();
            }
            else
            {
                SpawnAIUnits(team);
                onSpawnerComplete.Invoke();
            }
        }

        private void SpawnAIUnits(BattleTeam team)
        {
            BattleAITeamData aiData = BattleManager.GetDataForTeam<BattleAITeamData>(team);
            if(aiData != null)
            {
                foreach(BattleAIUnitSpawnInfo objInfo in aiData.AISpawnUnits)
                {
                    List<BattleCell> cellsToSpawnAt = BattleManager.GetGrid().GetCellsById(objInfo.SpawnAtCellId);

                    if(cellsToSpawnAt.Count > 0)
                    {
                        int selectedIndex = Random.Range(0, cellsToSpawnAt.Count - 1);
                        BattleCell selectedCell = cellsToSpawnAt[selectedIndex];
                        if(selectedCell != null)
                        {
                            IUnit spawnedUnit = BattleManager.SpawnUnit(objInfo.UnitData, team, selectedCell.GetIndex(), objInfo.StartDirection);
                            if(spawnedUnit is BattleUnit battleUnit)
                            {
                                battleUnit.SetAsTarget(objInfo.IsTarget);
                                battleUnit.AddAI(objInfo.AssociatedAI);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[ProjectCI]::TacticalBattleRules::SpawnAIUnits Couldn't find a cell for the AI unit: \"{objInfo.UnitData.UnitName}\" to spawn at. Either the SpawnAtCellId({objInfo.SpawnAtCellId}) isn't set, or no cells are tagged.");
                    }
                }
            }
        }

        private void AutoPlaceUnits(BattleHumanTeamData humanData, List<BattleCell> spawnList)
        {
            int numStartPoints = spawnList.Count;
            int numInRoster = humanData.UnitRoster.Count;

            for(int i = 0; i < numInRoster; i++)
            {
                BattleHumanUnitSpawnInfo humanSpawnInfo = humanData.UnitRoster[i];
                if(humanSpawnInfo.UnitData != null)
                {
                    List<BattleCell> spawnCells = BattleManager.GetGrid().GetCellsById(humanSpawnInfo.SpawnAtCellId);

                    List<BattleCell> cellsToRemove = new List<BattleCell>();
                    foreach(BattleCell cell in spawnCells)
                    {
                        if(cell != null && cell.IsObjectOnCell())
                        {
                            cellsToRemove.Add(cell);
                        }
                    }

                    foreach(BattleCell cell in cellsToRemove)
                    {
                        spawnCells.Remove(cell);
                    }

                    if(spawnCells.Count > 0 && !string.IsNullOrEmpty(humanSpawnInfo.SpawnAtCellId))
                    {
                        int selectedIndex = Random.Range(0, spawnCells.Count - 1);
                        BattleCell selectedCell = spawnCells[selectedIndex];
                    
                        if(spawnList.Contains(selectedCell))
                        {
                            spawnList.Remove(selectedCell);
                        }
                    
                        IUnit spawnedUnit = BattleManager.SpawnUnit(humanSpawnInfo.UnitData, humanData.GetTeam(), selectedCell.GetIndex(), humanSpawnInfo.StartDirection);
                        if(spawnedUnit is BattleUnit battleUnit)
                        {
                            battleUnit.SetAsTarget(humanSpawnInfo.IsTarget);
                        }
                    }
                    else if(i < numStartPoints)
                    {
                        int selectedCellIndex = Random.Range(0, spawnList.Count);
                        BattleCell selectedCell = spawnList[selectedCellIndex];
                        spawnList.RemoveAt(selectedCellIndex);
                        IUnit spawnedUnit = BattleManager.SpawnUnit(humanSpawnInfo.UnitData, humanData.GetTeam(), selectedCell.GetIndex(), humanSpawnInfo.StartDirection);
                        if(spawnedUnit is BattleUnit battleUnit)
                        {
                            battleUnit.SetAsTarget(humanSpawnInfo.IsTarget);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[ProjectCI]::TacticalBattleRules::AutoPlaceUnits Ran out of points to spawn at, can't spawn all units. Either there are not enough team spawn points, or it's not set up properly on the grid or on the TeamData asset");
                    }
                }
            }
        }

        private void HandleEnemyUnitsSpawned()
        {
            SpawnUnits(BattleTeam.Player, HandleAllUnitsSpawned);
        }

        private void HandleAllUnitsSpawned()
        {
            isSelectingPoints = false;
            StartGame();
        }

        private void CleanUpSelectedUnit()
        {
            if(selectedUnit != null)
            {
                if(selectedUnit is BattleUnit battleUnit)
                {
                    battleUnit.UnBindFromOnMovementComplete(UpdateSelectedHoverObject);
                }
                UnselectUnit();
            }
        }

        private void SetupTeam(BattleTeam team)
        {
            List<BattleUnit> units = BattleManager.GetUnitsOnTeam(team);
            foreach(BattleUnit unit in units)
            {
                unit.HandleTurnStarted();
            }
        }

        private bool IsHoverObjectSpawned()
        {
            return currentHoverObject != null;
        }

        private void UpdateSelectedHoverObject()
        {
            // TODO: Handle Pool
            if(currentHoverObject != null)
            {
                Destroy(currentHoverObject);
            }

            if(selectedUnit != null && !selectedUnit.IsDead)
            {
                GameObject hoverObj = BattleManager.GetSelectedHoverPrefab();
                if(hoverObj != null)
                {
                    currentHoverObject = Instantiate(hoverObj, selectedUnit.GetCell().GetAllignPos(selectedUnit), hoverObj.transform.rotation);
                }
            }
        }

        private void HandleInputCancel(InputAction.CallbackContext context)
        {
            if(BattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if(isSelectingPoints)
            {
                return;
            }

            if(BattleManager.IsPlaying())
            {
                if(selectedUnit != null)
                {
                    if(selectedUnit is BattleUnit battleUnit)
                    {
                        UnitState currentState = battleUnit.GetCurrentState();
                        if(currentState == UnitState.UsingAbility)
                        {
                            battleUnit.SetupMovement();
                        }
                        else if(currentState == UnitState.Moving)
                        {
                            UnselectUnit();
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            if(selectedUnit == null && IsHoverObjectSpawned())
            {
                UpdateSelectedHoverObject();
            }
        }

        public void UnselectUnit()
        {
            if(selectedUnit != null)
            {
                if(selectedUnit is BattleUnit battleUnit)
                {
                    battleUnit.CleanUp();
                }
                selectedUnit = null;

                BattleManager.Get().OnUnitSelected.Invoke(selectedUnit);
                UpdateSelectedHoverObject();
            }

            BattleManager.Get().UpdateHoverCells();
        }

        public override BattleUnit GetSelectedUnit() => selectedUnit;

        public override void HandleNumPressed(int numPressed)
        {
            if(isSelectingPoints)
            {
                return;
            }

            if(selectedUnit != null && selectedUnit is BattleUnit battleUnit)
            {
                battleUnit.SetupAbility(numPressed - 1);
            }
        }

        public override void BeginTeamTurn(BattleTeam team)
        {
            CleanUpSelectedUnit();
            SetupTeam(team);
            
            AilmentHandler.HandleTurnStart(team);

            if(team == BattleTeam.Hostile)
            {
                bool isHostileTeamAI = BattleManager.IsTeamAI(BattleTeam.Hostile);
                if(isHostileTeamAI)
                {
                    List<IUnit> aiUnits = BattleManager.GetUnitsOnTeam(BattleTeam.Hostile);
                    AIManager.RunAI(aiUnits, EndTurn);
                }
            }
        }

        public override void EndTeamTurn(BattleTeam team)
        {
            AilmentHandler.HandleTurnEnd(team);
        }
        
        public override void HandleEnemySelected(IUnit enemyUnit)
        {
        }

        public override void HandlePlayerSelected(IUnit playerUnit)
        {
            if(isSelectingPoints)
            {
                return;
            }

            if(BattleManager.IsActionBeingPerformed())
            {
                return;
            }

            BattleTeam currTeam = GetCurrentTeam();
            if(currTeam == playerUnit.GetTeam())
            {
                if(selectedUnit != null)
                {
                    if(selectedUnit is BattleUnit battleUnit && battleUnit.GetCurrentState() == UnitState.UsingAbility)
                    {
                        return;
                    }

                    CleanUpSelectedUnit();
                }

                selectedUnit = playerUnit;

                if(selectedUnit != null && selectedUnit is BattleUnit battleUnit)
                {
                    battleUnit.SelectUnit();
                    battleUnit.BindToOnMovementComplete(UpdateSelectedHoverObject);
                    BattleManager.Get().OnUnitSelected.Invoke(selectedUnit);
                }

                UpdateSelectedHoverObject();
            }
        }

        public override void HandleCellSelected(BattleCell cell)
        {
            if(BattleManager.IsActionBeingPerformed())
            {
                return;
            }

            if(isSelectingPoints)
            {
                if(currentSpawner != null)
                {
                    currentSpawner.HandleTileSelected(cell);
                }
            }
            else
            {
                if(selectedUnit != null && selectedUnit is BattleUnit battleUnit)
                {
                    UnitState currentState = battleUnit.GetCurrentState();

                    if(currentState == UnitState.Moving)
                    {
                        battleUnit.ExecuteMovement(cell);
                    }
                    else if(currentState == UnitState.UsingAbility)
                    {
                        battleUnit.ExecuteAbility(cell);
                    }
                }
            }
        }

        public override void HandleTeamWon(BattleTeam team)
        {
            UnselectUnit();
        }
    }
} 