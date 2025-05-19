using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.Enums;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Units.Components;
using ProjectCI.CoreSystem.Runtime.Levels;
using ProjectCI.CoreSystem.Runtime.Levels.Enums;
using ProjectCI.CoreSystem.Runtime.Core.Teams;
using ProjectCI.CoreSystem.Runtime.Core.Rules;

namespace ProjectCI.CoreSystem.Runtime.Core
{
    [System.Serializable]
    public class BattleTeamEvent : UnityEvent<BattleTeam>
    { }

    [System.Serializable]
    public class BattleUnitEvent : UnityEvent<BattleUnit>
    { }

    public class BattleManager : MonoBehaviour
    {
        private static BattleManager instance = null;
        private BattleGrid _levelGrid;

        [Space(10)]
        [SerializeField] private BattleRules battleRules;
        [SerializeField] private CameraController cameraController;

        [Space(10)]
        [SerializeField] private GameObject selectedHoverObject;

        [Space(10)]
        [Header("Team Data")]
        [SerializeField] private BattleHumanTeamData friendlyTeamData;
        [SerializeField] private BattleTeamData hostileTeamData;

        [Space(10)]
        [SerializeField] private WinCondition[] winConditions;
        [SerializeField] private GameObject[] spawnOnStart;
        [SerializeField] private GameObject[] addToSpawnedUnits;
        [SerializeField] private AbilityParticle[] deathParticles;

        [Space(10)]
        [SerializeField] public BattleTeamEvent OnTeamWon;
        [HideInInspector] public BattleUnitEvent OnUnitSelected;
        [HideInInspector] public BattleUnitEvent OnUnitHover = new BattleUnitEvent();

        private List<BattleObject> spawnedCellObjects = new List<BattleObject>();
        private Dictionary<BattleTeam, List<BattleUnit>> teams = new Dictionary<BattleTeam, List<BattleUnit>>();
        private Dictionary<BattleTeam, int> numberOfKilledTargets = new Dictionary<BattleTeam, int>();
        private Dictionary<BattleTeam, int> numberOfKilledEntities = new Dictionary<BattleTeam, int>();
        private List<BattleCell> currentHoverCells = new List<BattleCell>();
        private UnityEvent onFinishedPerformedActions = new UnityEvent();
        private BattleCell currentHoverCell;
        private int numActionsBeingPerformed;
        private bool isPlaying = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void Start()
        {
            DirectionalCellSpawner[] cellSpawners = FindObjectsOfType<DirectionalCellSpawner>();
            foreach (DirectionalCellSpawner cellSpawner in cellSpawners)
            {
                Destroy(cellSpawner.gameObject);
            }

            if (!_levelGrid)
            {
                Debug.Log("([ProjectCI]::BattleManager::Start) Missing Grid");
            }

            if (!battleRules)
            {
                Debug.Log("([ProjectCI]::BattleManager::Start) Missing BattleRules");
            }

            if (winConditions.Length == 0)
            {
                Debug.Log("([ProjectCI]::BattleManager::Start) Missing WinConditions");
            }

            if (!friendlyTeamData)
            {
                Debug.Log("([ProjectCI]::BattleManager::Start) Missing Friendly Team Data");
            }

            if (!hostileTeamData)
            {
                Debug.Log("([ProjectCI]::BattleManager::Start) Missing Hostile Team Data");
            }

            if (friendlyTeamData)
            {
                friendlyTeamData.SetTeam(BattleTeam.Player);
            }

            if (hostileTeamData)
            {
                hostileTeamData.SetTeam(BattleTeam.Enemy);
            }

            if (battleRules)
            {
                inputHandler.OnNumPressed.AddListener(battleRules.HandleNumPressed);
                OnTeamWon.AddListener(battleRules.HandleTeamWon);
            }

            Initialize();

            foreach (GameObject spawnObj in spawnOnStart)
            {
                if (spawnObj)
                {
                    Instantiate(spawnObj);
                }
            }
        }

        private void Update()
        {
            if (battleRules)
            {
                battleRules.Update();
            }
        }

        private void Initialize()
        {
            SetupGrid();
            if (battleRules)
            {
                battleRules.InitializeRules();
            }
        }

        private void SetupGrid()
        {
            if (_levelGrid)
            {
                _levelGrid.SetupAllCellAdjacencies();
                _levelGrid.OnCellInteraction.AddListener(HandleInteraction);
            }
            SetupMaterials();
        }

        private void SetupMaterials()
        {
            if (_levelGrid)
            {
                BattleCell testCell = null;

                List<BattleCell> levelCells = _levelGrid.GetAllCells();
                if (levelCells.Count > 0)
                {
                    testCell = levelCells[0];
                }
                if (testCell != null)
                {
                    foreach (BattleCell currCell in levelCells)
                    {
                        BattleCellState cellState = currCell.GetNormalState();
                        currCell.SetCellState(cellState);
                        currCell.SetMaterial(cellState);
                    }
                }
            }
        }

        #region Public Statics

        public static BattleManager Get()
        {
            return instance;
        }

        public static BattleGrid GetGrid()
        {
            return instance._levelGrid;
        }

        public static BattleRules GetRules()
        {
            return instance.battleRules;
        }

        public static CameraController GetCameraController()
        {
            return instance.cameraController;
        }

        public static GameObject GetSelectedHoverPrefab()
        {
            return instance.selectedHoverObject;
        }

        public static List<WinCondition> GetWinConditions()
        {
            return new List<WinCondition>(instance.winConditions);
        }

        public static BattleTeam GetTeamAffinity(BattleTeam team1, BattleTeam team2)
        {
            if (team1 == team2)
            {
                return BattleTeam.Player;
            }

            return BattleTeam.Enemy;
        }

        public static Dictionary<BattleTeam, List<BattleUnit>> GetTeamsMap()
        {
            return instance.teams;
        }

        public static List<BattleTeam> GetTeamList()
        {
            List<BattleTeam> outTeams = new List<BattleTeam>();

            foreach (var item in GetTeamsMap().Keys)
            {
                outTeams.Add(item);
            }

            return outTeams;
        }

        public static bool IsActionBeingPerformed()
        {
            return (instance.numActionsBeingPerformed > 0);
        }

        public static void AddActionBeingPerformed()
        {
            ++instance.numActionsBeingPerformed;
        }

        public static void RemoveActionBeingPerformed()
        {
            --instance.numActionsBeingPerformed;

            if (instance.numActionsBeingPerformed == 0)
            {
                instance.onFinishedPerformedActions.Invoke();
            }
        }

        public static void BindToOnFinishedPerformedActions(UnityAction action)
        {
            instance.onFinishedPerformedActions.AddListener(action);
        }

        public static void UnBindFromOnFinishedPerformedActions(UnityAction action)
        {
            instance.onFinishedPerformedActions.RemoveListener(action);
        }

        public static int GetNumTargetsKilled(BattleTeam team)
        {
            if (instance.numberOfKilledTargets.ContainsKey(team))
            {
                return instance.numberOfKilledTargets[team];
            }

            return 0;
        }

        public static int NumUnitsKilled(BattleTeam team)
        {
            if (instance.numberOfKilledEntities.ContainsKey(team))
            {
                return instance.numberOfKilledEntities[team];
            }

            return 0;
        }

        public static bool AreAllUnitsOnTeamDead(BattleTeam team)
        {
            return NumUnitsKilled(team) == instance.teams[team].Count;
        }

        public static bool CanFinishTurn()
        {
            bool actionBeingPerformed = IsActionBeingPerformed();
            bool isTeamHuman = true;

            BattleRules battleRules = GetRules();
            if (battleRules)
            {
                isTeamHuman = IsTeamHuman(battleRules.GetCurrentTeam());
            }

            return (!actionBeingPerformed && isTeamHuman);
        }

        public static int GetNumOfTargets(BattleTeam team)
        {
            return GetTeamTargets(team).Count;
        }

        public static List<BattleUnit> GetTeamTargets(BattleTeam team)
        {
            List<BattleUnit> units = new List<BattleUnit>();

            if(instance.teams.ContainsKey(team))
            {
                foreach (BattleUnit unit in instance.teams[team])
                {
                    if (unit.IsTarget())
                    {
                        units.Add(unit);
                    }
                }
            }

            return units;
        }

        public static bool KilledAllTargets(BattleTeam team)
        {
            return GetNumTargetsKilled(team) == GetNumOfTargets(team);
        }

        public static BattleHumanTeamData GetFriendlyTeamData()
        {
            return instance.friendlyTeamData;
        }

        public static BattleTeamData GetHostileTeamData()
        {
            return instance.hostileTeamData;
        }

        public static BattleTeamData GetDataForTeam(BattleTeam team)
        {
            switch (team)
            {
                case BattleTeam.Player:
                    return GetFriendlyTeamData();
                case BattleTeam.Enemy:
                    return GetHostileTeamData();
            }

            Debug.Log("([ProjectCI]::BattleManager::GetDataForTeam) Trying to get TeamData for invalid team: " + team.ToString());
            return new BattleTeamData();
        }

        public static T GetDataForTeam<T>(BattleTeam team) where T : BattleTeamData
        {
            switch (team)
            {
                case BattleTeam.Player:
                    return GetFriendlyTeamData() as T;
                case BattleTeam.Enemy:
                    return GetHostileTeamData() as T;
            }

            return null;
        }

        public static BattleObject SpawnObjectOnCell(GameObject obj, BattleCell cell, Vector3 offset = default)
        {
            if (cell && obj)
            {
                BattleObject spawnedObject = Instantiate(obj).AddComponent<BattleObject>();

                float objHeight = spawnedObject.Bounds.y;
                float cellHeight = cell.GetRenderer().bounds.size.y;

                Vector3 heightOffset = new Vector3(0.0f, (cellHeight * 0.5f) + (objHeight * 0.5f), 0.0f);

                spawnedObject.gameObject.transform.position = cell.gameObject.transform.position + heightOffset + offset;

                spawnedObject.Initialize();
                spawnedObject.PostInitialize();

                return spawnedObject;
            }

            return null;
        }

        public static BattleUnit SpawnUnit(IUnitData unitData, BattleTeam team, Vector2 index, GridDirection startDirection = GridDirection.South)
        {
            BattleCell cell = instance._levelGrid[index];

            if (team == BattleTeam.Player)
            {
                cell.SetVisible(true);
            }

            BattleUnit spawnedUnit = Instantiate(unitData.Model).AddComponent<BattleUnit>();

            spawnedUnit.Initialize();
            spawnedUnit.SetUnitData(unitData);
            spawnedUnit.SetTeam(team);
            spawnedUnit.SetGrid(instance._levelGrid);
            spawnedUnit.SetCurrentCell(cell);
            spawnedUnit.AlignToGrid();
            spawnedUnit.PostInitialize();

            BattleCell dirCell = spawnedUnit.GetCell().GetAdjacentCell(startDirection);
            if (dirCell)
            {
                spawnedUnit.LookAtCell(dirCell);
            }

            foreach (GameObject obj in instance.addToSpawnedUnits)
            {
                if (obj)
                {
                    Instantiate(obj, spawnedUnit.gameObject.transform);
                }
            }

            instance.spawnedCellObjects.Add(spawnedUnit);

            if (!instance.teams.ContainsKey(team))
            {
                instance.teams.Add(team, new List<BattleUnit>());
            }

            instance.teams[team].Add(spawnedUnit);

            return spawnedUnit;
        }

        private void SpawnDeathParticlesForUnit(BattleUnit unit)
        {
            foreach (AbilityParticle particle in deathParticles)
            {
                if (particle)
                {
                    AbilityParticle spawnedParticle = Instantiate(particle.gameObject, unit.gameObject.transform.position, particle.gameObject.transform.rotation).GetComponent<AbilityParticle>();
                    if (spawnedParticle)
                    {
                        spawnedParticle.Setup(null, null, null);
                    }
                }
            }
        }

        public static List<Renderer> GetAllRenderersOfObject(GameObject obj)
        {
            List<Renderer> renderers = new List<Renderer>();

            renderers.Add(obj.GetComponent<Renderer>());
            renderers.AddRange(obj.GetComponentsInChildren<Renderer>());

            return renderers;
        }

        public static Vector3 GetBoundsOfObject(GameObject obj)
        {
            if (obj)
            {
                Vector3 bounds = new Vector3();

                List<Renderer> renderers = GetAllRenderersOfObject(obj);
                foreach (Renderer currRenderer in renderers)
                {
                    if (currRenderer)
                    {
                        Vector3 rendererBound = currRenderer.bounds.size;
                        if (rendererBound.x > bounds.x)
                        {
                            bounds.x = rendererBound.x;
                        }
                        if (rendererBound.y > bounds.y)
                        {
                            bounds.y = rendererBound.y;
                        }
                        if (rendererBound.z > bounds.z)
                        {
                            bounds.z = rendererBound.z;
                        }
                    }
                }

                return bounds;
            }

            return new Vector3();
        }

        public static List<BattleUnit> GetUnitsOnTeam(BattleTeam team)
        {
            if (team == BattleTeam.None)
            {
                Debug.Log("([ProjectCI]::BattleManager::GetUnitsOnTeam) Trying to get units for invalid team: " + team.ToString());
            }

            if (instance.teams.ContainsKey(team))
            {
                return instance.teams[team];
            }

            return new List<BattleUnit>();
        }

        public static bool IsTeamHuman(BattleTeam team)
        {
            return GetDataForTeam<BattleHumanTeamData>(team) != null;
        }

        public static bool IsTeamAI(BattleTeam team)
        {
            return GetDataForTeam<BattleAITeamData>(team) != null;
        }

        public static bool IsUnitOnTeam(BattleUnit unit, BattleTeam team)
        {
            if(instance.teams.ContainsKey(team))
            {
                return instance.teams[team].Contains(unit);
            }
            return false;
        }

        public static bool IsPlaying()
        {
            return instance.isPlaying;
        }

        public static void HandleGameStarted()
        {
            instance.isPlaying = true;
        }

        public static BattleTeam GetUnitTeam(BattleUnit unit)
        {
            if (IsUnitOnTeam(unit, BattleTeam.Player))
            {
                return BattleTeam.Player;
            }
            if (IsUnitOnTeam(unit, BattleTeam.Enemy))
            {
                return BattleTeam.Enemy;
            }

            return BattleTeam.None;
        }

        public static void ResetCellState(BattleCell cell)
        {
            SetCellState(cell, cell.GetNormalState());
        }

        public static void SetCellState(BattleCell cell, BattleCellState cellState)
        {
            if (instance._levelGrid)
            {
                cell.SetMaterial(cellState);
                cell.SetCellState(cellState);
                if (cell.IsMouseOver())
                {
                    instance.BeginHover(cell);
                }
            }
        }

        public static bool CanCasterEffectTarget(BattleCell caster, BattleCell target, BattleTeam effectedTeam, bool allowBlocked)
        {
            if (!caster || !target)
            {
                return false;
            }

            if ((caster.IsBlocked() || target.IsBlocked()) && !allowBlocked)
            {
                return false;
            }

            if (effectedTeam == BattleTeam.None)
            {
                return false;
            }

            if (caster.GetCellTeam() != BattleTeam.None)
            {
                BattleTeam objAffinity = BattleManager.GetTeamAffinity(caster.GetCellTeam(), target.GetCellTeam());
                if (objAffinity == BattleTeam.Player && effectedTeam == BattleTeam.Enemy)
                {
                    return false;
                }
            }

            return true;
        }

        public static void FinishTurn()
        {
            if (CanFinishTurn())
            {
                if (instance.battleRules)
                {
                    instance.battleRules.EndTurn();
                }

                CheckWinConditions();
            }
        }

        public static void CheckWinConditions()
        {
            if (!instance.isPlaying)
            {
                return;
            }

            Dictionary<BattleTeam, int> teamToWinCount = new Dictionary<BattleTeam, int>()
            {
                { BattleTeam.Player, 0},
                { BattleTeam.Enemy, 0}
            };

            int numWinConditions = instance.winConditions.Length;

            foreach (WinCondition currWinCondition in instance.winConditions)
            {
                if (currWinCondition)
                {
                    if (currWinCondition.CheckWinFirst)
                    {
                        if (DidTeamPassCondition(currWinCondition, BattleTeam.Player))
                        {
                            if (++teamToWinCount[BattleTeam.Player] >= numWinConditions)
                            {
                                TeamWon(BattleTeam.Player);
                            }
                        }

                        if (DidTeamPassCondition(currWinCondition, BattleTeam.Enemy))
                        {
                            if (++teamToWinCount[BattleTeam.Enemy] >= numWinConditions)
                            {
                                TeamWon(BattleTeam.Enemy);
                            }
                        }

                        CheckLost(currWinCondition, BattleTeam.Player);
                        CheckLost(currWinCondition, BattleTeam.Enemy);
                    }
                    else
                    {
                        CheckLost(currWinCondition, BattleTeam.Player);
                        CheckLost(currWinCondition, BattleTeam.Enemy);

                        if (DidTeamPassCondition(currWinCondition, BattleTeam.Player))
                        {
                            if (++teamToWinCount[BattleTeam.Player] >= numWinConditions)
                            {
                                TeamWon(BattleTeam.Player);
                            }
                        }

                        if (DidTeamPassCondition(currWinCondition, BattleTeam.Enemy))
                        {
                            if (++teamToWinCount[BattleTeam.Enemy] >= numWinConditions)
                            {
                                TeamWon(BattleTeam.Enemy);
                            }
                        }
                    }
                }
            }
        }

        public static void HandleUnitDeath(BattleUnit unit)
        {
            if (!instance.numberOfKilledEntities.ContainsKey(unit.GetTeam()))
            {
                instance.numberOfKilledEntities.Add(unit.GetTeam(), 0);
            }

            instance.numberOfKilledEntities[unit.GetTeam()]++;

            if (unit.IsTarget())
            {
                if (!instance.numberOfKilledTargets.ContainsKey(unit.GetTeam()))
                {
                    instance.numberOfKilledTargets.Add(unit.GetTeam(), 0);
                }

                instance.numberOfKilledTargets[unit.GetTeam()]++;
            }

            instance.SpawnDeathParticlesForUnit(unit);

            CheckWinConditions();
        }

        public static void HandleUnitActivated(BattleUnit unit)
        {
            // To be implemented
        }

        #endregion

        #region Event Stuff

        private void BeginHover(BattleCell cell)
        {
            currentHoverCell = cell;
            UpdateHoverCells();
        }

        public void UpdateHoverCells()
        {
            CleanupHoverCells();

            if (currentHoverCell)
            {
                BattleUnit hoverUnit = currentHoverCell.GetUnitOnCell() as BattleUnit;
                if (hoverUnit)
                {
                    OnUnitHover.Invoke(hoverUnit);
                }

                currentHoverCells.Add(currentHoverCell);

                BattleRules battleRules = GetRules();
                if (battleRules)
                {
                    BattleUnit selectedUnit = battleRules.GetSelectedUnit();
                    if (selectedUnit)
                    {
                        UnitState unitState = selectedUnit.GetCurrentState();
                        if (unitState == UnitState.UsingAbility)
                        {
                            currentHoverCells.AddRange(selectedUnit.GetAbilityHoverCells(currentHoverCell));
                        }
                        else if (unitState == UnitState.Moving)
                        {
                            List<BattleCell> allowedMovementCells = selectedUnit.GetAllowedMovementCells();

                            if (allowedMovementCells.Contains(currentHoverCell))
                            {
                                List<BattleCell> pathToCursor = selectedUnit.GetPathTo(currentHoverCell, allowedMovementCells);

                                foreach (BattleCell pathCell in pathToCursor)
                                {
                                    if (pathCell)
                                    {
                                        if (allowedMovementCells.Contains(pathCell))
                                        {
                                            currentHoverCells.Add(pathCell);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (BattleCell currCell in currentHoverCells)
                {
                    currCell.SetMaterial(BattleCellState.Hover);
                }

                currentHoverCell.HandleMouseOver();
            }
        }

        private void EndHover(BattleCell cell)
        {
            CleanupHoverCells();

            if (cell)
            {
                cell.HandleMouseExit();
            }

            currentHoverCell = null;

            OnUnitHover.Invoke(null);
        }

        private void CleanupHoverCells()
        {
            foreach (BattleCell currCell in currentHoverCells)
            {
                if (currCell)
                {
                    currCell.SetMaterial(currCell.GetCellState());
                }
            }

            currentHoverCells.Clear();
        }

        private void HandleCellClicked(BattleCell cell)
        {
            if (!cell)
            {
                return;
            }

            if (!battleRules)
            {
                return;
            }

            BattleUnit unit = cell.GetUnitOnCell() as BattleUnit;
            if (unit)
            {
                BattleTeam currentTurnTeam = battleRules.GetCurrentTeam();
                BattleTeam unitsTeam = unit.Team;

                if (unitsTeam == currentTurnTeam)
                {
                    battleRules.HandlePlayerSelected(unit);
                }
                else
                {
                    if (unitsTeam == BattleTeam.Enemy)
                    {
                        battleRules.HandleEnemySelected(unit);
                    }
                }
            }
            battleRules.HandleCellSelected(cell);
        }

        private static bool DidTeamPassCondition(WinCondition condition, BattleTeam team)
        {
            if (condition)
            {
                if (condition.CheckTeamWin(team))
                {
                    return true;
                }
            }

            return false;
        }

        private static void CheckLost(WinCondition condition, BattleTeam team)
        {
            if (condition)
            {
                if (condition.CheckTeamLost(team))
                {
                    TeamLost(team);
                }
            }
        }

        private static void TeamWon(BattleTeam team)
        {
            instance.OnTeamWon.Invoke(team);
            instance.HandleGameComplete();
        }

        private static void TeamLost(BattleTeam team)
        {
            BattleTeam winningTeam = team == BattleTeam.Player ? BattleTeam.Enemy : BattleTeam.Player;
            TeamWon(winningTeam);
        }

        private void HandleGameComplete()
        {
            currentHoverCell = null;
            CleanupHoverCells();
            isPlaying = false;
        }

        private void HandleInteraction(ILevelCell cell, CellInteractionState interactionState)
        {
            GridObject objOnCell = cell.GetObjectOnCell();
            if (objOnCell)
            {
                objOnCell.HandleInteraction(interactionState);
            }

            switch (interactionState)
            {
                case CellInteractionState.BeginHover:
                    BeginHover(cell);
                    break;
                case CellInteractionState.EndHover:
                    EndHover(cell);
                    break;
                case CellInteractionState.LeftClick:
                    HandleCellClicked(cell);
                    break;
            }
        }

        #endregion
    }
} 