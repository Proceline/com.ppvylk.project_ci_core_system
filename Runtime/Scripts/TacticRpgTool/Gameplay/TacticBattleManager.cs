using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Extensions;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;
using ProjectCI.CoreSystem.Runtime.InputSupport;

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
    public class GameTeamEvent : UnityEvent<BattleTeam>
    { }

    [System.Serializable]
    public class GridUnitEvent : UnityEvent<GridPawnUnit>
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

    public partial class TacticBattleManager : MonoBehaviour
    {
        static TacticBattleManager sInstance = null;
        public virtual LevelGridBase LevelGrid { get; set; }

        [Space(10)]

        [SerializeField]
        private InputActionManager m_InputActionManager;

        [SerializeField]
        protected BattleGameRules m_GameRules;

        [SerializeField]
        protected FogOfWar m_FogOfWar;

        [SerializeField]
        protected CameraController m_CameraController;

        [Space(10)]

        [SerializeField]
        protected GameObject m_SelectedHoverObject;

        [Space(10)]

        [SerializeField]
        protected WinCondition[] m_WinConditions;

        [SerializeField]
        protected GameObject[] m_SpawnOnStart;

        [SerializeField]
        protected GameObject[] m_AddToSpawnedUnits;

        [SerializeField]
        protected AbilityParticle[] m_DeathParticles;

        [Space(10)]

        [SerializeField]
        public GameTeamEvent OnTeamWon;

        [HideInInspector]
        public GridUnitEvent OnUnitSelected;

        [HideInInspector]
        public GridUnitEvent OnUnitHover = new GridUnitEvent();

        List<GridObject> SpawnedCellObjects = new List<GridObject>();

        Dictionary<BattleTeam, List<GridPawnUnit>> m_Teams = new Dictionary<BattleTeam, List<GridPawnUnit>>();

        Dictionary<BattleTeam, int> m_NumberOfKilledTargets = new Dictionary<BattleTeam, int>();
        Dictionary<BattleTeam, int> m_NumberOfKilledEntities = new Dictionary<BattleTeam, int>();

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

        protected virtual void OnDestroy()
        {
            UnregisterControlActions();
        }

        void Update()
        {
            if (m_GameRules)
            {
                m_GameRules.Update();
            }
        }

        public virtual void Initialize()
        {
            SetupGrid();

            if (m_GameRules)
            {
                OnTeamWon.AddListener(m_GameRules.HandleTeamWon);
                m_GameRules.InitalizeRules();
            }

            if (m_FogOfWar)
            {
                m_FogOfWar.SpawnFogObjects();
            }

            m_InputActionManager.EnableAllActions();
            RegisterControlActions();
            foreach (GameObject SpawnObj in m_SpawnOnStart)
            {
                if (SpawnObj)
                {
                    Instantiate(SpawnObj);
                }
            }
        }

        void SetupGrid()
        {
            if (LevelGrid)
            {
                LevelGrid.SetupAllCellAdjacencies();
                LevelGrid.OnCellBeingInteracted += HandleInteractionFocused;
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

        public static BattleTeam GetTeamAffinity(BattleTeam InTeam1, BattleTeam InTeam2)
        {
            if (InTeam1 == InTeam2)
            {
                return BattleTeam.Friendly;
            }

            return BattleTeam.Hostile;
        }

        public static Dictionary<BattleTeam, List<GridPawnUnit>> GetTeamsMap()
        {
            return sInstance.m_Teams;
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

        public static int GetNumTargetsKilled(BattleTeam InTeam)
        {
            if (sInstance.m_NumberOfKilledTargets.ContainsKey(InTeam))
            {
                return sInstance.m_NumberOfKilledTargets[InTeam];
            }

            return 0;
        }

        public static int NumUnitsKilled(BattleTeam InTeam)
        {
            if (sInstance.m_NumberOfKilledEntities.ContainsKey(InTeam))
            {
                return sInstance.m_NumberOfKilledEntities[InTeam];
            }

            return 0;
        }

        public static bool AreAllUnitsOnTeamDead(BattleTeam InTeam)
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

        public static int GetNumOfTargets(BattleTeam InTeam)
        {
            return GetTeamTargets(InTeam).Count;
        }

        public static List<GridPawnUnit> GetTeamTargets(BattleTeam InTeam)
        {
            List<GridPawnUnit> units = new List<GridPawnUnit>();

            if(sInstance.m_Teams.ContainsKey(InTeam))
            {
                foreach (GridPawnUnit unit in sInstance.m_Teams[InTeam])
                {
                    if (unit.IsTarget())
                    {
                        units.Add(unit);
                    }
                }
            }

            return units;
        }

        public static bool KilledAllTargets(BattleTeam InTeam)
        {
            return GetNumTargetsKilled(InTeam) == GetNumOfTargets(InTeam);
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

        public static GridPawnUnit SpawnUnit(UnitData InUnitData, BattleTeam InTeam, Vector2 InIndex, CompassDir InStartDirection = CompassDir.S)
        {
            LevelCellBase cell = sInstance.LevelGrid[InIndex];

            if (InTeam == BattleTeam.Friendly)
            {
                cell.SetVisible(true);
            }

            GridPawnUnit SpawnedGridUnit;

            if (InUnitData.m_UnitClass == "")
            {
                SpawnedGridUnit = Instantiate(InUnitData.m_Model).AddComponent<GridPawnUnit>();
            }
            else
            {
                System.Type classType = GameUtils.FindType(InUnitData.m_UnitClass);
                SpawnedGridUnit = Instantiate(InUnitData.m_Model).AddComponent(classType) as GridPawnUnit;
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

        public static void AddUnitToTeam(GridPawnUnit InUnit, BattleTeam InTeam)
        {
            sInstance.SpawnedCellObjects.Add(InUnit);

            if (!sInstance.m_Teams.ContainsKey(InTeam))
            {
                sInstance.m_Teams.Add(InTeam, new List<GridPawnUnit>());
            }

            sInstance.m_Teams[InTeam].Add(InUnit);

            if (InTeam == BattleTeam.Friendly)
            {
                if (sInstance.m_FogOfWar)
                {
                    sInstance.m_FogOfWar.CheckPoint(InUnit.GetCell());
                }
            }
        }

        void SpawnDeathParticlesForUnit(GridPawnUnit InUnit)
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

        public static List<GridPawnUnit> GetUnitsOnTeam(BattleTeam InTeam)
        {
            if (InTeam == BattleTeam.None)
            {
                Debug.Log("([ProjectCI]::TacticBattleManager::GetUnitsOnTeam) Trying to get units for invalid team: " + InTeam.ToString());
            }

            if (sInstance.m_Teams.ContainsKey(InTeam))
            {
                return sInstance.m_Teams[InTeam];
            }

            return new List<GridPawnUnit>();
        }

        protected virtual bool IsTeamHumanByBattleTeam(BattleTeam InTeam)
        {
            switch (InTeam)
            {
                case BattleTeam.Friendly:
                    return true;
                case BattleTeam.Hostile:
                    return false;
            }
            return false;
        }
        public static bool IsTeamHuman(BattleTeam InTeam)
        {
            return sInstance.IsTeamHumanByBattleTeam(InTeam);
        }

        public static bool IsTeamAI(BattleTeam InTeam)
        {
            return !sInstance.IsTeamHumanByBattleTeam(InTeam);
        }

        public static bool IsUnitOnTeam(GridPawnUnit InUnit, BattleTeam InTeam)
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

        public static BattleTeam GetUnitTeam(GridPawnUnit InUnit)
        {
            if (IsUnitOnTeam(InUnit, BattleTeam.Friendly))
            {
                return BattleTeam.Friendly;
            }
            if (IsUnitOnTeam(InUnit, BattleTeam.Hostile))
            {
                return BattleTeam.Hostile;
            }

            return BattleTeam.None;
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

        public static bool CanCasterEffectTarget(LevelCellBase InCaster, LevelCellBase InTarget, BattleTeam InEffectedTeam, bool bAllowBlocked)
        {
            if (!InCaster || !InTarget)
            {
                return false;
            }

            if ((InCaster.IsBlocked() || InTarget.IsBlocked()) && !bAllowBlocked)
            {
                return false;
            }

            if (InEffectedTeam == BattleTeam.None)
            {
                return false;
            }

            if (InCaster.GetCellTeam() != BattleTeam.None)
            {
                BattleTeam ObjAffinity = TacticBattleManager.GetTeamAffinity(InCaster.GetCellTeam(), InTarget.GetCellTeam());
                if (ObjAffinity == BattleTeam.Friendly && InEffectedTeam == BattleTeam.Hostile)
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

            Dictionary<BattleTeam, int> TeamToWinCount = new Dictionary<BattleTeam, int>()
            {
                { BattleTeam.Friendly, 0},
                { BattleTeam.Hostile, 0}
            };

            int NumWinConditions = sInstance.m_WinConditions.Length;

            foreach (WinCondition currWinCondition in sInstance.m_WinConditions)
            {
                if (currWinCondition)
                {
                    if (currWinCondition.m_bCheckWinFirst)
                    {
                        if (DidTeamPassCondition(currWinCondition, BattleTeam.Friendly))
                        {
                            if (++TeamToWinCount[BattleTeam.Friendly] >= NumWinConditions)
                            {
                                TeamWon(BattleTeam.Friendly);
                            }
                        }

                        if (DidTeamPassCondition(currWinCondition, BattleTeam.Hostile))
                        {
                            if (++TeamToWinCount[BattleTeam.Hostile] >= NumWinConditions)
                            {
                                TeamWon(BattleTeam.Hostile);
                            }
                        }

                        CheckLost(currWinCondition, BattleTeam.Friendly);
                        CheckLost(currWinCondition, BattleTeam.Hostile);
                    }
                    else
                    {
                        CheckLost(currWinCondition, BattleTeam.Friendly);
                        CheckLost(currWinCondition, BattleTeam.Hostile);

                        if (DidTeamPassCondition(currWinCondition, BattleTeam.Friendly))
                        {
                            if (++TeamToWinCount[BattleTeam.Friendly] >= NumWinConditions)
                            {
                                TeamWon(BattleTeam.Friendly);
                            }
                        }

                        if (DidTeamPassCondition(currWinCondition, BattleTeam.Hostile))
                        {
                            if (++TeamToWinCount[BattleTeam.Hostile] >= NumWinConditions)
                            {
                                TeamWon(BattleTeam.Hostile);
                            }
                        }
                    }
                }
            }
        }

        public static void HandleUnitDeath(GridPawnUnit InUnit)
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

        public static void HandleUnitActivated(GridPawnUnit InUnit)
        {

        }

        #endregion

        #region Conditions
        static bool DidTeamPassCondition(WinCondition InCondition, BattleTeam InTeam)
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

        static void CheckLost(WinCondition InCondition, BattleTeam InTeam)
        {
            if (InCondition)
            {
                if (InCondition.CheckTeamLost(InTeam))
                {
                    TeamLost(InTeam);
                }
            }
        }

        static void TeamWon(BattleTeam InTeam)
        {
            sInstance.OnTeamWon.Invoke(InTeam);
            sInstance.HandleGameComplete();
        }

        static void TeamLost(BattleTeam InTeam)
        {
            BattleTeam WinningTeam = InTeam == BattleTeam.Friendly ? BattleTeam.Hostile : BattleTeam.Friendly;
            TeamWon(WinningTeam);
        }

        #endregion
    }
}