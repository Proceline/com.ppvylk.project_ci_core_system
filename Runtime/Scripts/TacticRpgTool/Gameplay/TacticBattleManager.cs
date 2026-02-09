using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Extensions;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay
{
    [Serializable]
    public enum CellState
    {
        eNormal,
        eHover,
        ePositive,
        eNegative,
        eMovement,
        eSpecial,
        eReadOnlyAggro,
        eReadOnlyMove
    }

    [Serializable]
    public class GameTeamEvent : UnityEvent<BattleTeam>
    { }

    [Serializable]
    public class GridUnitEvent : UnityEvent<GridPawnUnit>
    { }

    [Serializable]
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
        private static TacticBattleManager _instance;
        public virtual LevelGridBase LevelGrid { get; set; }

        [Space(10)]

        [SerializeField]
        protected BattleGameRules m_GameRules;

        [SerializeField]
        protected FogOfWar m_FogOfWar;

        [Space(10)]

        [SerializeField]
        protected GameObject m_SelectedHoverObject;

        [Space(10)] 
        private readonly Dictionary<BattleTeam, List<GridPawnUnit>> _mTeams = new();

        private bool _isPlaying = false;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
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
                m_GameRules.InitializeRules();
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
                LevelGrid.OnCellBeingInteracted += HandleInteractionFocused;
            }
            SetupMaterials();
        }

        void SetupMaterials()
        {
            if (LevelGrid)
            {
                LevelCellBase testCell = null;

                List<LevelCellBase> levelCells = LevelGrid.GetAllCells();
                if (levelCells.Count > 0)
                {
                    testCell = levelCells[0];
                }
                if (testCell != null)
                {
                    foreach (LevelCellBase currCell in levelCells)
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
            return _instance;
        }

        public static LevelGridBase GetGrid()
        {
            return _instance.LevelGrid;
        }

        public static BattleGameRules GetRules()
        {
            return _instance.m_GameRules;
        }
        
        public static FogOfWar GetFogOfWar()
        {
            return _instance.m_FogOfWar;
        }

        public static GameObject GetSelectedHoverPrefab()
        {
            return _instance.m_SelectedHoverObject;
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
            return _instance._mTeams;
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

        public static GridPawnUnit SpawnUnit<T>(GameObject prefab, SoUnitData unitData, BattleTeam InTeam, Vector2Int InIndex, CompassDir InStartDirection = CompassDir.S)
            where T : GridPawnUnit
        {
            LevelCellBase cell = _instance.LevelGrid[InIndex];

            if (InTeam == BattleTeam.Friendly)
            {
                cell.SetVisible(true);
            }

            GridPawnUnit spawnedGridUnit = Instantiate(prefab).AddComponent<T>();

            spawnedGridUnit.Initialize();
            spawnedGridUnit.SetUnitData(unitData);
            spawnedGridUnit.SetTeam(InTeam);
            spawnedGridUnit.SetGrid(_instance.LevelGrid);
            spawnedGridUnit.SetCurrentCell(cell);
            spawnedGridUnit.AlignToGrid();
            spawnedGridUnit.PostInitialize();

            LevelCellBase dirCell = spawnedGridUnit.GetCell().GetAdjacentCell(InStartDirection);
            if (dirCell)
            {
                spawnedGridUnit.LookAtCell(dirCell);
            }

            AddUnitToTeam(spawnedGridUnit, InTeam);

            return spawnedGridUnit;
        }

        public static void AddUnitToTeam(GridPawnUnit InUnit, BattleTeam InTeam)
        {
            if (!_instance._mTeams.ContainsKey(InTeam))
            {
                _instance._mTeams.Add(InTeam, new List<GridPawnUnit>());
            }

            _instance._mTeams[InTeam].Add(InUnit);

            if (InTeam == BattleTeam.Friendly)
            {
                if (_instance.m_FogOfWar)
                {
                    _instance.m_FogOfWar.CheckPoint(InUnit.GetCell());
                }
            }
        }

        private static List<Renderer> GetAllRenderersOfObject(GameObject InObject)
        {
            List<Renderer> renderers = new() { InObject.GetComponent<Renderer>() };

            renderers.AddRange(InObject.GetComponentsInChildren<Renderer>());

            return renderers;
        }

        public static Vector3 GetBoundsOfObject(GameObject InObject)
        {
            if (InObject)
            {
                Vector3 bounds = new Vector3();

                List<Renderer> renderers = GetAllRenderersOfObject(InObject);
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

        public static List<GridPawnUnit> GetUnitsOnTeam(BattleTeam inTeam)
        {
            if (inTeam == BattleTeam.None)
            {
                throw new Exception("([ProjectCI]::TacticBattleManager::GetUnitsOnTeam) Trying to get units for invalid team: " + inTeam);
            }

            if (_instance._mTeams.TryGetValue(inTeam, out var team))
            {
                return team;
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
            return _instance.IsTeamHumanByBattleTeam(InTeam);
        }

        public static bool IsTeamAI(BattleTeam InTeam)
        {
            return !_instance.IsTeamHumanByBattleTeam(InTeam);
        }

        public static bool IsUnitOnTeam(GridPawnUnit InUnit, BattleTeam InTeam)
        {
            if(_instance._mTeams.ContainsKey(InTeam))
            {
                return _instance._mTeams[InTeam].Contains(InUnit);
            }
            return false;
        }

        public static bool IsPlaying()
        {
            return _instance._isPlaying;
        }

        public static void HandleGameStarted()
        {
            _instance._isPlaying = true;
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

        public static void SetCellState(LevelCellBase inCell, CellState inCellState)
        {
            if (_instance.LevelGrid)
            {
                inCell.SetMaterial(inCellState);
                inCell.SetCellState(inCellState);
                if (inCell.IsMouseOver())
                {
                    _instance.onCellPointed?.Invoke(inCell);
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
                BattleTeam objAffinity = GetTeamAffinity(InCaster.GetCellTeam(), InTarget.GetCellTeam());
                if (objAffinity == BattleTeam.Friendly && InEffectedTeam == BattleTeam.Hostile)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}