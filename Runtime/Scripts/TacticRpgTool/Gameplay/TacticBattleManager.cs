using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.PlayerData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay
{
    [System.Serializable]
    public enum CellState
    {
        eNormal,
        eHover,
        ePositive,
        eNegative,
        eMovement
    }

    [System.Serializable]
    public class GameTeamEvent : UnityEvent<GameTeam>
    { }

    [System.Serializable]
    public class GridUnitEvent : UnityEvent<GridUnit>
    { }

    [System.Serializable]
    public struct TeamInfo
    {
        public int TeamId;

        public TeamInfo(int InTeamId)
        {
            TeamId = InTeamId;
        }

        public static TeamInfo InvalidTeam()
        {
            return new TeamInfo(-1);
        }

        public bool IsValid()
        {
            return TeamId != -1;
        }

        public override bool Equals(object obj)
        {
            TeamInfo otherTeamInfo = (TeamInfo)obj;
            return otherTeamInfo.TeamId == TeamId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class TacticBattleManager : MonoBehaviour
    {
        static TacticBattleManager sInstance = null;
        protected virtual LevelGridBase LevelGrid { get; set; }

        protected virtual HumanTeamData FriendlyTeamData { get; set; }

        protected virtual TeamData HostileTeamData { get; set; }

        [Space(10)]

        [SerializeField]
        BattleGameRules m_GameRules;

        [SerializeField]
        FogOfWar m_FogOfWar;

        [SerializeField]
        CameraController m_CameraController;

        [Space(10)]

        [SerializeField]
        GameObject m_SelectedHoverObject;

        [Space(10)]

        [SerializeField]
        WinCondition[] m_WinConditions;

        [SerializeField]
        protected GameObject[] m_SpawnOnStart;

        [SerializeField]
        GameObject[] m_AddToSpawnedUnits;

        [SerializeField]
        AbilityParticle[] m_DeathParticles;

        [Space(10)]

        [SerializeField]
        public GameTeamEvent OnTeamWon;

        [HideInInspector]
        public GridUnitEvent OnUnitSelected;

        [HideInInspector]
        public GridUnitEvent OnUnitHover = new GridUnitEvent();

        List<GridObject> SpawnedCellObjects = new List<GridObject>();

        Dictionary<GameTeam, List<GridUnit>> m_Teams = new Dictionary<GameTeam, List<GridUnit>>();

        Dictionary<GameTeam, int> m_NumberOfKilledTargets = new Dictionary<GameTeam, int>();
        Dictionary<GameTeam, int> m_NumberOfKilledEntities = new Dictionary<GameTeam, int>();

        List<LevelCellBase> CurrentHoverCells = new List<LevelCellBase>();

        UnityEvent OnFinishedPerformedActions = new UnityEvent();

        LevelCellBase m_CurrentHoverCell;

        int m_NumActionsBeingPerformed;

        bool m_bIsPlaying = false;

        void Awake()
        {
            if (sInstance == null)
            {
                sInstance = this;
            }
        }

        protected virtual void Start()
        {
            if (!LevelGrid)
            {
                Debug.Log("([ProjectCI]::TacticBattleManager::Start) Missing Grid");
            }

            if (!m_GameRules)
            {
                Debug.Log("([ProjectCI]::TacticBattleManager::Start) Missing GameRules");
            }

            if (m_WinConditions.Length == 0)
            {
                Debug.Log("([ProjectCI]::TacticBattleManager::Start) Missing WinConditions");
            }

            if (m_GameRules)
            {
                OnTeamWon.AddListener(m_GameRules.HandleTeamWon);
            }

            Initalize();

            foreach (GameObject SpawnObj in m_SpawnOnStart)
            {
                if (SpawnObj)
                {
                    Instantiate(SpawnObj);
                }
            }
        }

        void Update()
        {
            if (m_GameRules)
            {
                m_GameRules.Update();
            }
        }

        void Initalize()
        {
            SetupGrid();
            if (m_GameRules)
            {
                m_GameRules.InitalizeRules();
            }

            if (m_FogOfWar)
            {
                m_FogOfWar.SpawnFogObjects();
            }
        }

        void SetupGrid()
        {
            if (LevelGrid)
            {
                LevelGrid.SetupAllCellAdjacencies();
                LevelGrid.OnCellInteraction.AddListener(HandleInteraction);
            }
            SetupMaterials();
        }

        void SetupMaterials()
        {
            if (LevelGrid)
            {
                LevelCellBase TestCell = null;

                List<LevelCellBase> LevelCells = LevelGrid.GetAllCells();
                if (LevelCells.Count > 0)
                {
                    TestCell = LevelCells[0];
                }
                if (TestCell != null)
                {
                    foreach (LevelCellBase currCell in LevelCells)
                    {
                        CellState cellState = currCell.GetNormalState();
                        currCell.SetCellState(cellState);
                        currCell.SetMaterial(cellState);
                    }
                }
            }
        }

        #region Public Statics

        public static TacticBattleManager Get()
        {
            return sInstance;
        }

        public static LevelGridBase GetGrid()
        {
            return sInstance.LevelGrid;
        }

        public static BattleGameRules GetRules()
        {
            return sInstance.m_GameRules;
        }
        
        public static FogOfWar GetFogOfWar()
        {
            return sInstance.m_FogOfWar;
        }

        public static CameraController GetCameraController()
        {
            return sInstance.m_CameraController;
        }

        public static GameObject GetSelectedHoverPrefab()
        {
            return sInstance.m_SelectedHoverObject;
        }

        public static List<WinCondition> GetWinConditions()
        {
            return new List<WinCondition>(sInstance.m_WinConditions);
        }

        public static GameTeam GetTeamAffinity(GameTeam InTeam1, GameTeam InTeam2)
        {
            if (InTeam1 == InTeam2)
            {
                return GameTeam.Friendly;
            }

            return GameTeam.Hostile;
        }

        public static Dictionary<GameTeam, List<GridUnit>> GetTeamsMap()
        {
            return sInstance.m_Teams;
        }

        public static List<GameTeam> GetTeamList()
        {
            List<GameTeam> outTeams = new List<GameTeam>();

            foreach (var item in GetTeamsMap().Keys)
            {
                outTeams.Add(item);
            }

            return outTeams;
        }

        //Checks if a player is moving, attacking or basically anything that should prevent input.
        public static bool IsActionBeingPerformed()
        {
            return (sInstance.m_NumActionsBeingPerformed > 0);
        }

        public static void AddActionBeingPerformed()
        {
            ++sInstance.m_NumActionsBeingPerformed;
        }

        public static void RemoveActionBeingPerformed()
        {
            --sInstance.m_NumActionsBeingPerformed;

            if (sInstance.m_NumActionsBeingPerformed == 0)
            {
                sInstance.OnFinishedPerformedActions.Invoke();
            }
        }

        public static void BindToOnFinishedPerformedActions(UnityAction InAction)
        {
            sInstance.OnFinishedPerformedActions.AddListener(InAction);
        }

        public static void UnBindFromOnFinishedPerformedActions(UnityAction InAction)
        {
            sInstance.OnFinishedPerformedActions.RemoveListener(InAction);
        }

        public static int GetNumTargetsKilled(GameTeam InTeam)
        {
            if (sInstance.m_NumberOfKilledTargets.ContainsKey(InTeam))
            {
                return sInstance.m_NumberOfKilledTargets[InTeam];
            }

            return 0;
        }

        public static int NumUnitsKilled(GameTeam InTeam)
        {
            if (sInstance.m_NumberOfKilledEntities.ContainsKey(InTeam))
            {
                return sInstance.m_NumberOfKilledEntities[InTeam];
            }

            return 0;
        }

        public static bool AreAllUnitsOnTeamDead(GameTeam InTeam)
        {
            return NumUnitsKilled(InTeam) == sInstance.m_Teams[InTeam].Count;
        }

        public static bool CanFinishTurn()
        {
            bool bActionBeingPerformed = IsActionBeingPerformed();
            bool bIsTeamHuman = true;

            BattleGameRules gameRules = GetRules();
            if (gameRules)
            {
                bIsTeamHuman = IsTeamHuman(gameRules.GetCurrentTeam());
            }

            return (!bActionBeingPerformed && bIsTeamHuman);
        }

        public static int GetNumOfTargets(GameTeam InTeam)
        {
            return GetTeamTargets(InTeam).Count;
        }

        public static List<GridUnit> GetTeamTargets(GameTeam InTeam)
        {
            List<GridUnit> units = new List<GridUnit>();

            if(sInstance.m_Teams.ContainsKey(InTeam))
            {
                foreach (GridUnit unit in sInstance.m_Teams[InTeam])
                {
                    if (unit.IsTarget())
                    {
                        units.Add(unit);
                    }
                }
            }

            return units;
        }

        public static bool KilledAllTargets(GameTeam InTeam)
        {
            return GetNumTargetsKilled(InTeam) == GetNumOfTargets(InTeam);
        }

        public static HumanTeamData GetFriendlyTeamData()
        {
            return sInstance.FriendlyTeamData;
        }

        public static TeamData GetHostileTeamData()
        {
            return sInstance.HostileTeamData;
        }

        public static TeamData GetDataForTeam(GameTeam InTeam)
        {
            switch (InTeam)
            {
                case GameTeam.Friendly:
                    return GetFriendlyTeamData();
                case GameTeam.Hostile:
                    return GetHostileTeamData();
            }

            Debug.Log("([ProjectCI]::TacticBattleManager::GetDataForTeam) Trying to get TeamData for invalid team: " + InTeam.ToString());
            return new TeamData();
        }

        public static T GetDataForTeam<T>(GameTeam InTeam) where T : TeamData
        {
            switch (InTeam)
            {
                case GameTeam.Friendly:
                    return GetFriendlyTeamData() as T;
                case GameTeam.Hostile:
                    return GetHostileTeamData() as T;
            }

            return null;
        }

        public static GridObject SpawnObjectOnCell(GameObject InObject, LevelCellBase InCell, Vector3 InOffset = default(Vector3))
        {
            if ( InCell && InObject )
            {
                GridObject SpawnedGridObject = Instantiate(InObject).AddComponent<GridObject>();

                float ObjHeight = SpawnedGridObject.GetBounds().y;
                float CellHeight = InCell.GetRenderer().bounds.size.y;

                Vector3 HeightOffset = new Vector3(0.0f, (CellHeight * 0.5f) + (ObjHeight * 0.5f), 0.0f);

                SpawnedGridObject.gameObject.transform.position = InCell.gameObject.transform.position + HeightOffset + InOffset;

                SpawnedGridObject.Initalize();
                SpawnedGridObject.SetGrid( GetGrid() );
                SpawnedGridObject.SetCurrentCell( InCell );
                SpawnedGridObject.PostInitalize();

                return SpawnedGridObject;
            }

            return null;
        }

        public static GridUnit SpawnUnit(UnitData InUnitData, GameTeam InTeam, Vector2 InIndex, CompassDir InStartDirection = CompassDir.S)
        {
            LevelCellBase cell = sInstance.LevelGrid[InIndex];

            if (InTeam == GameTeam.Friendly)
            {
                cell.SetVisible(true);
            }

            GridUnit SpawnedGridUnit;

            if (InUnitData.m_UnitClass == "")
            {
                SpawnedGridUnit = Instantiate(InUnitData.m_Model).AddComponent<GridUnit>();
            }
            else
            {
                System.Type classType = GameUtils.FindType(InUnitData.m_UnitClass);
                SpawnedGridUnit = Instantiate(InUnitData.m_Model).AddComponent(classType) as GridUnit;
            }

            SpawnedGridUnit.Initalize();
            SpawnedGridUnit.SetUnitData(InUnitData);
            SpawnedGridUnit.SetTeam(InTeam);
            SpawnedGridUnit.SetGrid(sInstance.LevelGrid);
            SpawnedGridUnit.SetCurrentCell(cell);
            SpawnedGridUnit.AlignToGrid();
            SpawnedGridUnit.PostInitalize();

            LevelCellBase DirCell = SpawnedGridUnit.GetCell().GetAdjacentCell(InStartDirection);
            if (DirCell)
            {
                SpawnedGridUnit.LookAtCell(DirCell);
            }

            foreach (GameObject obj in sInstance.m_AddToSpawnedUnits)
            {
                if (obj)
                {
                    Instantiate(obj, SpawnedGridUnit.gameObject.transform);
                }
            }

            AddUnitToTeam(SpawnedGridUnit, InTeam);

            return SpawnedGridUnit;
        }

        public static void AddUnitToTeam(GridUnit InUnit, GameTeam InTeam)
        {
            sInstance.SpawnedCellObjects.Add(InUnit);

            if (!sInstance.m_Teams.ContainsKey(InTeam))
            {
                sInstance.m_Teams.Add(InTeam, new List<GridUnit>());
            }

            sInstance.m_Teams[InTeam].Add(InUnit);

            if (InTeam == GameTeam.Friendly)
            {
                if (sInstance.m_FogOfWar)
                {
                    sInstance.m_FogOfWar.CheckPoint(InUnit.GetCell());
                }
            }
        }

        void SpawnDeathParticlesForUnit(GridUnit InUnit)
        {
            foreach (AbilityParticle particle in m_DeathParticles)
            {
                if (particle)
                {
                    AbilityParticle spawnedParticle = Instantiate(particle.gameObject, InUnit.gameObject.transform.position, particle.gameObject.transform.rotation).GetComponent<AbilityParticle>();
                    if (spawnedParticle)
                    {
                        spawnedParticle.Setup(null, null, null);
                    }
                }
            }
        }

        public static List<Renderer> GetAllRenderersOfObject(GameObject InObject)
        {
            List<Renderer> Renderers = new List<Renderer>();

            Renderers.Add(InObject.GetComponent<Renderer>());
            Renderers.AddRange(InObject.GetComponentsInChildren<Renderer>());

            return Renderers;
        }

        public static Vector3 GetBoundsOfObject(GameObject InObject)
        {
            if (InObject)
            {
                Vector3 bounds = new Vector3();

                List<Renderer> Renderers = GetAllRenderersOfObject(InObject);
                foreach (Renderer currRenderer in Renderers)
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

        public static List<GridUnit> GetUnitsOnTeam(GameTeam InTeam)
        {
            if (InTeam == GameTeam.None)
            {
                Debug.Log("([ProjectCI]::TacticBattleManager::GetUnitsOnTeam) Trying to get units for invalid team: " + InTeam.ToString());
            }

            if (sInstance.m_Teams.ContainsKey(InTeam))
            {
                return sInstance.m_Teams[InTeam];
            }

            return new List<GridUnit>();
        }

        public static bool IsTeamHuman(GameTeam InTeam)
        {
            return GetDataForTeam<HumanTeamData>(InTeam) != null;
        }

        public static bool IsTeamAI(GameTeam InTeam)
        {
            return GetDataForTeam<AITeamData>(InTeam) != null;
        }

        public static bool IsUnitOnTeam(GridUnit InUnit, GameTeam InTeam)
        {
            if(sInstance.m_Teams.ContainsKey(InTeam))
            {
                return sInstance.m_Teams[InTeam].Contains(InUnit);
            }
            return false;
        }

        public static bool IsPlaying()
        {
            return sInstance.m_bIsPlaying;
        }

        public static void HandleGameStarted()
        {
            sInstance.m_bIsPlaying = true;
        }

        public static GameTeam GetUnitTeam(GridUnit InUnit)
        {
            if (IsUnitOnTeam(InUnit, GameTeam.Friendly))
            {
                return GameTeam.Friendly;
            }
            if (IsUnitOnTeam(InUnit, GameTeam.Hostile))
            {
                return GameTeam.Hostile;
            }

            return GameTeam.None;
        }

        public static void ResetCellState(LevelCellBase InCell)
        {
            SetCellState(InCell, InCell.GetNormalState());
        }

        public static void SetCellState(LevelCellBase InCell, CellState InCellState)
        {
            if (sInstance.LevelGrid)
            {
                InCell.SetMaterial(InCellState);
                InCell.SetCellState(InCellState);
                if (InCell.IsMouseOver())
                {
                    sInstance.BeginHover(InCell);
                }
            }
        }

        public static bool CanCasterEffectTarget(LevelCellBase InCaster, LevelCellBase InTarget, GameTeam InEffectedTeam, bool bAllowBlocked)
        {
            if (!InCaster || !InTarget)
            {
                return false;
            }

            if ((InCaster.IsBlocked() || InTarget.IsBlocked()) && !bAllowBlocked)
            {
                return false;
            }

            if (InEffectedTeam == GameTeam.None)
            {
                return false;
            }

            if (InCaster.GetCellTeam() != GameTeam.None)
            {
                GameTeam ObjAffinity = TacticBattleManager.GetTeamAffinity(InCaster.GetCellTeam(), InTarget.GetCellTeam());
                if (ObjAffinity == GameTeam.Friendly && InEffectedTeam == GameTeam.Hostile)
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
                if (sInstance.m_GameRules)
                {
                    sInstance.m_GameRules.EndTurn();
                }

                CheckWinConditions();
            }
        }

        public static void CheckWinConditions()
        {
            if (!sInstance.m_bIsPlaying)
            {
                return;
            }

            Dictionary<GameTeam, int> TeamToWinCount = new Dictionary<GameTeam, int>()
            {
                { GameTeam.Friendly, 0},
                { GameTeam.Hostile, 0}
            };

            int NumWinConditions = sInstance.m_WinConditions.Length;

            foreach (WinCondition currWinCondition in sInstance.m_WinConditions)
            {
                if (currWinCondition)
                {
                    if (currWinCondition.m_bCheckWinFirst)
                    {
                        if (DidTeamPassCondition(currWinCondition, GameTeam.Friendly))
                        {
                            if (++TeamToWinCount[GameTeam.Friendly] >= NumWinConditions)
                            {
                                TeamWon(GameTeam.Friendly);
                            }
                        }

                        if (DidTeamPassCondition(currWinCondition, GameTeam.Hostile))
                        {
                            if (++TeamToWinCount[GameTeam.Hostile] >= NumWinConditions)
                            {
                                TeamWon(GameTeam.Hostile);
                            }
                        }

                        CheckLost(currWinCondition, GameTeam.Friendly);
                        CheckLost(currWinCondition, GameTeam.Hostile);
                    }
                    else
                    {
                        CheckLost(currWinCondition, GameTeam.Friendly);
                        CheckLost(currWinCondition, GameTeam.Hostile);

                        if (DidTeamPassCondition(currWinCondition, GameTeam.Friendly))
                        {
                            if (++TeamToWinCount[GameTeam.Friendly] >= NumWinConditions)
                            {
                                TeamWon(GameTeam.Friendly);
                            }
                        }

                        if (DidTeamPassCondition(currWinCondition, GameTeam.Hostile))
                        {
                            if (++TeamToWinCount[GameTeam.Hostile] >= NumWinConditions)
                            {
                                TeamWon(GameTeam.Hostile);
                            }
                        }
                    }
                }
            }
        }

        public static void HandleUnitDeath(GridUnit InUnit)
        {
            if (!sInstance.m_NumberOfKilledEntities.ContainsKey(InUnit.GetTeam()))
            {
                sInstance.m_NumberOfKilledEntities.Add(InUnit.GetTeam(), 0);
            }

            sInstance.m_NumberOfKilledEntities[InUnit.GetTeam()]++;

            if (InUnit.IsTarget())
            {
                if (!sInstance.m_NumberOfKilledTargets.ContainsKey(InUnit.GetTeam()))
                {
                    sInstance.m_NumberOfKilledTargets.Add(InUnit.GetTeam(), 0);
                }

                sInstance.m_NumberOfKilledTargets[InUnit.GetTeam()]++;
            }

            sInstance.SpawnDeathParticlesForUnit(InUnit);

            CheckWinConditions();
        }

        public static void HandleUnitActivated(GridUnit InUnit)
        {

        }

        #endregion

        #region EventStuff

        void BeginHover(LevelCellBase InCell)
        {
            m_CurrentHoverCell = InCell;
            UpdateHoverCells();
        }

        public void UpdateHoverCells()
        {
            CleanupHoverCells();

            if (m_CurrentHoverCell)
            {
                GridUnit hoverGrid = m_CurrentHoverCell.GetUnitOnCell();
                if (hoverGrid)
                {
                    OnUnitHover.Invoke(hoverGrid);
                }

                CurrentHoverCells.Add(m_CurrentHoverCell);

                BattleGameRules gameRules = GetRules();
                if (gameRules)
                {
                    GridUnit selectedUnit = gameRules.GetSelectedUnit();
                    if (selectedUnit)
                    {
                        UnitState unitState = selectedUnit.GetCurrentState();
                        if (unitState == UnitState.UsingAbility)
                        {
                            CurrentHoverCells.AddRange(selectedUnit.GetAbilityHoverCells(m_CurrentHoverCell));
                        }
                        else if (unitState == UnitState.Moving)
                        {
                            List<LevelCellBase> AllowedMovementCells = selectedUnit.GetAllowedMovementCells();

                            if (AllowedMovementCells.Contains(m_CurrentHoverCell))
                            {
                                List<LevelCellBase> PathToCursor = selectedUnit.GetPathTo(m_CurrentHoverCell, AllowedMovementCells);

                                foreach (LevelCellBase pathCell in PathToCursor)
                                {
                                    if (pathCell)
                                    {
                                        if (AllowedMovementCells.Contains(pathCell))
                                        {
                                            CurrentHoverCells.Add(pathCell);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (LevelCellBase currCell in CurrentHoverCells)
                {
                    currCell.SetMaterial(CellState.eHover);
                }

                m_CurrentHoverCell.HandleMouseOver();
            }
        }

        void EndHover(LevelCellBase InCell)
        {
            CleanupHoverCells();

            if (InCell)
            {
                InCell.HandleMouseExit();
            }

            m_CurrentHoverCell = null;

            OnUnitHover.Invoke(null);
        }

        void CleanupHoverCells()
        {
            foreach (LevelCellBase currCell in CurrentHoverCells)
            {
                if (currCell)
                {
                    currCell.SetMaterial(currCell.GetCellState());
                }
            }

            CurrentHoverCells.Clear();
        }

        void HandleCellClicked(LevelCellBase InCell)
        {
            if (!InCell)
            {
                return;
            }

            if (!m_GameRules)
            {
                return;
            }

            GridUnit gridUnit = InCell.GetUnitOnCell();
            if (gridUnit)
            {
                GameTeam CurrentTurnTeam = m_GameRules.GetCurrentTeam();
                GameTeam UnitsTeam = gridUnit.GetTeam();

                if (UnitsTeam == CurrentTurnTeam)
                {
                    m_GameRules.HandlePlayerSelected(gridUnit);
                }
                else
                {
                    if (UnitsTeam == GameTeam.Hostile)
                    {
                        m_GameRules.HandleEnemySelected(gridUnit);
                    }
                }
            }
            m_GameRules.HandleCellSelected(InCell);
        }

        static bool DidTeamPassCondition(WinCondition InCondition, GameTeam InTeam)
        {
            if (InCondition)
            {
                if (InCondition.CheckTeamWin(InTeam))
                {
                    return true;
                }
            }

            return false;
        }

        static void CheckLost(WinCondition InCondition, GameTeam InTeam)
        {
            if (InCondition)
            {
                if (InCondition.CheckTeamLost(InTeam))
                {
                    TeamLost(InTeam);
                }
            }
        }

        static void TeamWon(GameTeam InTeam)
        {
            sInstance.OnTeamWon.Invoke(InTeam);
            sInstance.HandleGameComplete();
        }

        static void TeamLost(GameTeam InTeam)
        {
            GameTeam WinningTeam = InTeam == GameTeam.Friendly ? GameTeam.Hostile : GameTeam.Friendly;
            TeamWon(WinningTeam);
        }

        void HandleGameComplete()
        {
            m_CurrentHoverCell = null;
            CleanupHoverCells();
            m_bIsPlaying = false;
        }

        void HandleInteraction(LevelCellBase InCell, CellInteractionState InInteractionState)
        {
            GridObject ObjOnCell = InCell.GetObjectOnCell();
            if (ObjOnCell)
            {
                ObjOnCell.HandleInteraction(InInteractionState);
            }

            switch (InInteractionState)
            {
                case CellInteractionState.eBeginHover:
                    BeginHover(InCell);
                    break;
                case CellInteractionState.eEndHover:
                    EndHover(InCell);
                    break;
                case CellInteractionState.eLeftClick:
                    HandleCellClicked(InCell);
                    break;
            }
        }

        #endregion
    }
}