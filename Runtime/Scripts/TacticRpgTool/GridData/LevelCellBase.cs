using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.Maps;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData
{
    [System.Serializable]
    public struct CellInfo
    {
        public bool bFriendlySpawnPoint;
        public bool bHostileSpawnPoint;
        public bool bIsVisible;

        public static CellInfo Default()
        {
            return new CellInfo
            {
                bIsVisible = true,
                bFriendlySpawnPoint = false,
                bHostileSpawnPoint = false
            };
        }
    }

    public abstract class LevelCellBase : MonoBehaviour
    {
        [SerializeField] 
        private CellInfo cellInfo;

        private Renderer _hintRenderer;

        private GridObject _objectOnCell;
        private Vector2Int _presetIndex;

        private LevelCellMap _adjacentCellsMap;

        private CellState _cellState;

        private bool _bTileNaturallyBlocked;
        private bool _bMouseIsOver;
        private bool _bIsHovering;

        private LevelGridBase _grid;
        private readonly List<ObjectWeightInfo> _weightInfosCol = new();

        public void Reset()
        {
            cellInfo = CellInfo.Default();
        }

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _cellState = CellState.eNormal;
            
            var objWeightInfo = GetComponent<ObjectWeightInfo>();
            if (objWeightInfo)
            {
                _weightInfosCol.Add(objWeightInfo);
            }

            var weightInfos = GetComponentsInChildren<ObjectWeightInfo>();
            foreach (var currWeightInfo in weightInfos)
            {
                _weightInfosCol.Add(currWeightInfo);
            }
            
            Renderer obtainedRenderer = GetComponent<Renderer>();
            if (!obtainedRenderer)
            {
                throw new NullReferenceException("Renderer ERROR: Add Renderer to Cell!");
            }

            _hintRenderer = obtainedRenderer;
        }

        protected virtual void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _bTileNaturallyBlocked = GetWeightInfo().bBlocked;
            HandleVisibilityChanged();
        }

        internal void Setup(LevelGridBase gridBase)
        {
            _adjacentCellsMap = new LevelCellMap();
            _grid = gridBase;
        }

        public void ClearAdjacencyList()
        {
            _adjacentCellsMap.Pairs.Clear();
        }

        public void AddAdjacentCell(CompassDir InDirection, LevelCellBase InLevelCell)
        {
            if (!_adjacentCellsMap.ContainsKey(InDirection))
            {
                _adjacentCellsMap.Add(InDirection, InLevelCell);
            }
            else
            {
                Debug.Log("[TurnBasedTools]::LevelCell::AddAdjacentCell) " + gameObject.name + " already has an adjacent cell in the " + InDirection.ToString() + " direction");
            }
        }

        private void RemoveCell(bool bInRemoveObj)
        {
            if (_grid)
            {
                _grid.RemoveCell(GetIndex(), bInRemoveObj);
            }
        }
        
        #region Getters

        public bool IsMouseOver()
        {
            return _bMouseIsOver;
        }

        public bool IsBlocked()
        {
            WeightInfo objWeightInfo = GetWeightInfo();
            return (objWeightInfo.bBlocked || _bTileNaturallyBlocked) && IsVisible();
        }

        public bool IsFriendlySpawnPoint()
        {
            return cellInfo.bFriendlySpawnPoint;
        }

        public bool IsHostileSpawnPoint()
        {
            return cellInfo.bHostileSpawnPoint;
        }

        public bool IsObjectOnCell()
        {
            return _objectOnCell;
        }

        public bool IsCellAccessible()
        {
            return !IsBlocked() && !IsObjectOnCell();
        }

        public bool IsVisible()
        {
            return GetInfo().bIsVisible;
        }

        public bool HasAdjacentCell(CompassDir InDirection)
        {
            return _adjacentCellsMap.ContainsKey(InDirection) && _adjacentCellsMap[InDirection] != null;
        }

        public LevelCellBase GetAdjacentCell(CompassDir InDirection)
        {
            if (_adjacentCellsMap.ContainsKey(InDirection))
            {
                return _adjacentCellsMap[InDirection];
            }
            else
            {
                return null;
            }
        }

        public List<LevelCellBase> GetAllAdjacentCells()
        {
            List<LevelCellBase> outCells = new List<LevelCellBase>();

            foreach (var pair in _adjacentCellsMap.Pairs)
            {
                outCells.Add(pair._Value);
            }

            return outCells;
        }

        public CellInfo GetInfo()
        {
            return cellInfo;
        }

        public CellState GetNormalState()
        {
            return CellState.eNormal;
        }

        public CellState GetCellState()
        {
            return _cellState;
        }

        public WeightInfo GetWeightInfo()
        {
            WeightInfo totalWeightInfo = new WeightInfo();
            foreach (var weightInfoComp in _weightInfosCol)
            {
                totalWeightInfo += weightInfoComp.weightInfo;
            }

            // List<StatusEffect> ailments = GetAilmentContainer().GetStatusEffectList();
            return totalWeightInfo;
        }

        public Vector3 GetAlignPos(GridObject InObject)
        {
            float objectHeightOffset = 0.0f;

            GridPawnUnit gridUnit = InObject as GridPawnUnit;
            if (gridUnit)
            {
                objectHeightOffset += gridUnit.GetUnitData().m_HeightOffset;
            }

            Renderer cellRenderer = GetRenderer();
            if (cellRenderer)
            {
                float cellHeight = cellRenderer.bounds.size.y;
                objectHeightOffset += ( cellHeight * 0.5f );

                Vector3 cellPosition = transform.position;
                return cellPosition + new Vector3(0, objectHeightOffset, 0); ;
            }

            return transform.position;
        }

        public Vector3 GetAlignPos(GameObject InObject)
        {
            Renderer cellRenderer = GetRenderer();
            if (cellRenderer)
            {
                Vector3 cellPosition = transform.position;
                Vector3 objectBounds = TacticBattleManager.GetBoundsOfObject(InObject);
                Vector3 levelCellBounds = cellRenderer.bounds.size;

                float heightOffset = levelCellBounds.y + objectBounds.y * 0.5f;
                Vector3 alignPos = cellPosition + new Vector3(0, heightOffset, 0);

                return alignPos;
            }

            return transform.position;
        }

        public GridObject GetObjectOnCell()
        {
            return _objectOnCell;
        }

        public GridPawnUnit GetUnitOnCell()
        {
            if (_objectOnCell)
            {
                return _objectOnCell as GridPawnUnit;
            }

            return null;
        }

        public BattleTeam GetCellTeam()
        {
            return _objectOnCell ? _objectOnCell.GetTeam() : BattleTeam.None;
        }

        public Vector2Int GetIndex()
        {
            return _presetIndex;
        }

        public CompassDir GetDirectionToAdjacentCell(LevelCellBase inTarget)
        {
            foreach (var pair in _adjacentCellsMap.Pairs)
            {
                if (pair._Value == inTarget)
                {
                    return pair._Key;
                }
            }

            return CompassDir.N;
        }

        public Renderer GetRenderer() => _hintRenderer;

        public List<Collider> GetColliders()
        {
            List<Collider> colliderList = new List<Collider>();

            Collider obtainedCollider = GetComponent<Collider>();
            if(obtainedCollider)
            {
                colliderList.Add(obtainedCollider);
            }

            Collider childCollider = gameObject.GetComponentInChildren<Collider>();
            if (childCollider)
            {
                colliderList.Add(childCollider);
            }

            Collider parentCollider = gameObject.GetComponentInParent<Collider>();
            if (parentCollider)
            {
                colliderList.Add(parentCollider);
            }

            return colliderList;
        }

        #endregion

        #region Setters

        public abstract void SetMaterial(CellState InCellState);

        public void SetVisible(bool bInVisible)
        {
            cellInfo.bIsVisible = bInVisible;
            TacticBattleManager.ResetCellState(this);
            HandleVisibilityChanged();
        }

        public void SetObjectOnCell(GridObject InObject)
        {
            _objectOnCell = InObject;
        }

        public void SetCellState(CellState InCellState)
        {
            _cellState = InCellState;
        }

        public void SetIndex(Vector2Int InIndex)
        {
            _presetIndex = InIndex;
        }

        #endregion

        #region EventListeners

        public void HandleMouseOver()
        {
            _bMouseIsOver = true;
        }

        public void HandleMouseExit()
        {
            _bMouseIsOver = false;
        }

        public void HandleVisibilityChanged()
        {
            GetRenderer().enabled = IsVisible();

            List<Collider> colliders = GetColliders();
            foreach ( Collider currCollider in colliders )
            {
                if(currCollider)
                {
                    currCollider.enabled = IsVisible();
                }
            }
        }

        protected virtual void OnDestroy()
        {
            RemoveCell(false);
        }

        public void OnMouseOver()
        {
            if(EventSystem.current)
            {

                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (_grid)
                    {
                        if (!_bIsHovering)
                        {
                            _bIsHovering = true;
                            _grid.OnCellBeingInteracted?.Invoke(this, CellInteractionState.BeginFocused);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("[ProjectCI] You need to add an EventSystem to your scene.");
            }
        }

        public void OnMouseExit()
        {
            if (_grid)
            {
                if (_bIsHovering)
                {
                    _bIsHovering = false;
                    _grid.OnCellBeingInteracted.Invoke(this, CellInteractionState.EndFocused);
                }
            }
        }

        #endregion
    }
}
