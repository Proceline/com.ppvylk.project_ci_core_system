using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using ProjectCI.CoreSystem.Runtime.Enums;
using ProjectCI.CoreSystem.Runtime.Levels.Data;
using ProjectCI.CoreSystem.Runtime.Levels.Components;
using ProjectCI.CoreSystem.Runtime.Levels.Enums;
using ProjectCI.CoreSystem.Runtime.Interfaces;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Effects.Components;
using ProjectCI.CoreSystem.Runtime.Effects.ScriptableObjects;
using ProjectCI.CoreSystem.Runtime.Levels;

namespace ProjectCI.CoreSystem.Runtime.Levels
{
    [ExecuteInEditMode]
    public abstract class BattleCell : MonoBehaviour
    {
        [SerializeField]
        BattleCellInfo m_Info;

        IObject m_ObjectOnCell;

        [SerializeField]
        [HideInInspector]
        Vector2 m_Index;

        [SerializeField]
        BattleCellDirectionMap m_AdjacentCellsMap;

        [SerializeField]
        BattleCellState m_CellState;

        bool m_bTileNaturallyBlocked = false;
        bool m_bMouseIsOver = false;
        bool bIsHovering = false;

        public UnityEvent OnCellDestroyed = new UnityEvent();

        void Reset()
        {
            m_Info = BattleCellInfo.Default();
        }

        void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            m_CellState = BattleCellState.Normal;
        }

        void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            m_bTileNaturallyBlocked = GetWeightInfo().bBlocked;
            HandleVisibilityChanged();
        }

        public void Setup()
        {
            m_AdjacentCellsMap = new BattleCellDirectionMap();
        }

        public void ClearAdjacencyList()
        {
            m_AdjacentCellsMap.Pairs.Clear();
        }

        public void AddAdjacentCell(GridDirection InDirection, BattleCell InLevelCell)
        {
            if (!m_AdjacentCellsMap.ContainsKey(InDirection))
            {
                m_AdjacentCellsMap.Add(InDirection, InLevelCell);
            }
            else
            {
                Debug.Log("[ProjectCI]::BattleCell::AddAdjacentCell) " + gameObject.name + " already has an adjacent cell in the " + InDirection.ToString() + " direction");
            }
        }

        public BattleCell AddCellTo(GridDirection InDirection)
        {
            if (!HasAdjacentCell(InDirection))
            {
                BattleCell generatedCell = GetGrid().GenerateCellAdjacentTo(GetIndex(), InDirection);
                return generatedCell;
            }

            return null;
        }

        public void RemoveCell(bool bInRemoveObj)
        {
            if (GetGrid())
            {
                GetGrid().RemoveCell(GetIndex(), bInRemoveObj);
            }
        }
        
        #region Getters

        public bool IsMouseOver()
        {
            return m_bMouseIsOver;
        }

        public bool IsBlocked()
        {
            BattleWeightInfo ObjWeightInfo = GetWeightInfo();
            return (ObjWeightInfo.bBlocked || m_bTileNaturallyBlocked) && IsVisible();
        }

        public bool IsFriendlySpawnPoint()
        {
            return m_Info.IsFriendlySpawnPoint;
        }

        public bool IsHostileSpawnPoint()
        {
            return m_Info.IsHostileSpawnPoint;
        }

        public bool IsObjectOnCell()
        {
            return (m_ObjectOnCell != null);
        }

        public bool IsCellAccesible()
        {
            return !IsBlocked() && !IsObjectOnCell();
        }

        public bool IsVisible()
        {
            return m_Info.IsVisible;
        }

        public bool HasAdjacentCell(GridDirection InDirection)
        {
            return m_AdjacentCellsMap.ContainsKey(InDirection) && m_AdjacentCellsMap[InDirection] != null;
        }

        public BattleCell GetAdjacentCell(GridDirection InDirection)
        {
            if (m_AdjacentCellsMap.ContainsKey(InDirection))
            {
                return m_AdjacentCellsMap[InDirection];
            }
            else
            {
                return null;
            }
        }

        public List<BattleCell> GetAllAdjacentCells()
        {
            List<BattleCell> outCells = new List<BattleCell>();

            foreach (var pair in m_AdjacentCellsMap.Pairs)
            {
                outCells.Add(pair.Cell);
            }

            return outCells;
        }

        public BattleEffectContainer GetEffectContainer()
        {
            BattleEffectContainer effectHandler = GetComponent<BattleEffectContainer>();
            if (!effectHandler)
            {
                effectHandler = gameObject.AddComponent<BattleEffectContainer>();
            }

            return effectHandler;
        }

        public BattleCellInfo GetInfo()
        {
            return m_Info;
        }

        public BattleCellState GetNormalState()
        {
            return BattleCellState.Normal;
        }

        public BattleCellState GetCellState()
        {
            return m_CellState;
        }

        public BattleWeightInfo GetWeightInfo()
        {
            BattleWeightInfo TotalWeightInfo = new BattleWeightInfo();

            BattleObjectWeightInfo ObjWeightInfo = GetComponent<BattleObjectWeightInfo>();
            if (ObjWeightInfo)
            {
                TotalWeightInfo += ObjWeightInfo.m_WeightInfo;
            }

            BattleObjectWeightInfo[] WeightInfos = GetComponentsInChildren<BattleObjectWeightInfo>();
            foreach (BattleObjectWeightInfo currWeightInfo in WeightInfos)
            {
                TotalWeightInfo += currWeightInfo.m_WeightInfo;
            }

            List<BattleEffectSO> effects = GetEffectContainer().GetEffects();
            foreach (BattleEffectSO currEffect in effects)
            {
                if(currEffect)
                {
                    // TODO: Implement cell effect weight info
                    // CellEffect cellEffect = currEffect as CellEffect;
                    // if(cellEffect)
                    // {
                    //     TotalWeightInfo += cellEffect.m_WeightInfo;
                    // }
                }
            }

            return TotalWeightInfo;
        }

        public Vector3 GetAllignPos(IObject InObject)
        {
            float objectHeightOffset = 0.0f;

            IUnit unit = (InObject as IUnit);
            if (unit != null)
            {
                // TODO: Get height offset from unit data
                // objectHeightOffset += unit.UnitData.HeightOffset;
            }

            Renderer CellRenderer = GetRenderer();
            if (CellRenderer)
            {
                float cellHeight = CellRenderer.bounds.size.y;
                objectHeightOffset += ( cellHeight * 0.5f );

                Vector3 CellPosition = transform.position;
                return CellPosition + new Vector3(0, objectHeightOffset, 0);
            }

            return transform.position;
        }

        public Vector3 GetAllignPos(GameObject InObject)
        {
            Renderer CellRenderer = GetRenderer();
            if (CellRenderer)
            {
                Vector3 CellPosition = transform.position;
                // TODO: Implement GameManager
                // Vector3 ObjectBounds = GameManager.GetBoundsOfObject(InObject);
                Vector3 ObjectBounds = Vector3.one;
                Vector3 LevelCellBounds = CellRenderer.bounds.size;

                float heightOffset = (LevelCellBounds.y + (ObjectBounds.y * 0.5f));
                Vector3 AlignPos = CellPosition + new Vector3(0, heightOffset, 0);

                return AlignPos;
            }

            return transform.position;
        }

        public IObject GetObjectOnCell()
        {
            return m_ObjectOnCell;
        }

        public IUnit GetUnitOnCell()
        {
            if (m_ObjectOnCell != null)
            {
                return m_ObjectOnCell as IUnit;
            }

            return null;
        }

        public BattleTeam GetCellTeam()
        {
            return m_ObjectOnCell != null ? m_ObjectOnCell.Team : BattleTeam.None;
        }

        public string GetCellId()
        {
            return m_Info.CellId;
        }

        public Vector2 GetIndex()
        {
            return m_Index;
        }

        public BattleGrid GetGrid()
        {
            return GetComponentInParent<BattleGrid>();
        }

        public GridDirection GetDirectionToAdjacentCell(BattleCell InTarget)
        {
            foreach (var pair in m_AdjacentCellsMap.Pairs)
            {
                if (pair.Cell == InTarget)
                {
                    return pair.Direction;
                }
            }

            return GridDirection.North;
        }

        public T GetRenderer<T>() where T : Renderer
        {
            Renderer renderer = GetComponent<Renderer>();
            if (!renderer)
            {
                renderer = gameObject.GetComponentInChildren<Renderer>();
            }
            if (!renderer)
            {
                renderer = gameObject.GetComponentInParent<Renderer>();
            }

            return renderer as T;
        }

        public Renderer GetRenderer()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (!renderer)
            {
                renderer = gameObject.GetComponentInChildren<Renderer>();
            }
            if (!renderer)
            {
                renderer = gameObject.GetComponentInParent<Renderer>();
            }

            return renderer;
        }

        public List<Collider> GetColliders()
        {
            List<Collider> colliderList = new List<Collider>();

            Collider collider = GetComponent<Collider>();
            if(collider)
            {
                colliderList.Add(collider);
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

        public virtual void SetMaterial(BattleCellState InCellState)
        {
            // Cannot use base BattleCell
        }

        public void SetVisible(bool bInVisible)
        {
            m_Info.IsVisible = bInVisible;
            // TODO: Implement GameManager
            // GameManager.ResetCellState(this);
            HandleVisibilityChanged();
        }

        public void SetObjectOnCell(IObject InObject)
        {
            m_ObjectOnCell = InObject;
        }

        public void SetCellState(BattleCellState InCellState)
        {
            m_CellState = InCellState;
        }

        public void SetIndex(Vector2 InIndex)
        {
            m_Index = InIndex;
        }

        #endregion

        #region EventListeners

        public void HandleMouseOver()
        {
            m_bMouseIsOver = true;
        }

        public void HandleMouseExit()
        {
            m_bMouseIsOver = false;
        }

        public void HandleVisibilityChanged()
        {
            IUnit unit = GetUnitOnCell();
            if (unit != null)
            {
                // TODO: Implement unit visibility check
                // unit.CheckCellVisibility();
            }

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

        private void OnDestroy()
        {
            OnCellDestroyed.Invoke();
            RemoveCell(false);
        }

        public void OnMouseOver()
        {
            if(EventSystem.current)
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (GetGrid())
                    {
                        if (!bIsHovering)
                        {
                            bIsHovering = true;
                            OnInteraction(CellInteractionState.BeginHover);
                        }

                        if (Input.GetMouseButtonDown(0))//Left click
                        {
                            OnInteraction(CellInteractionState.LeftClick);
                        }

                        if (Input.GetMouseButtonDown(1))//Right click
                        {
                            OnInteraction(CellInteractionState.RightClick);
                        }

                        if (Input.GetMouseButtonDown(2))//Middle click
                        {
                            OnInteraction(CellInteractionState.MiddleClick);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("[ProjectCI] You need to add an EventSystem to your scene. It's under UI in the right-click menu.");
            }
        }

        public void OnMouseExit()
        {
            if (GetGrid())
            {
                if (bIsHovering)
                {
                    bIsHovering = false;
                    OnInteraction(CellInteractionState.EndHover);
                }
            }
        }

        void OnInteraction(CellInteractionState InInteractionState)
        {
            GetGrid().HandleCellInteraction(this, InInteractionState);
        }

        #endregion
    }
} 