using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.Maps;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData
{
    [System.Serializable]
    public struct CellInfo
    {
        //Used for referencing, primarily used for placing enemies.
        [SerializeField]
        public string m_CellId;

        [SerializeField]
        public bool m_bFriendlySpawnPoint;

        [SerializeField]
        [Tooltip("Only used if team2 is human.")]
        public bool m_bHostileSpawnPoint;

        [SerializeField]
        public bool m_bIsVisible;

        public static CellInfo Default()
        {
            return new CellInfo()
            {
                m_bIsVisible = true,
                m_bFriendlySpawnPoint = false,
                m_bHostileSpawnPoint = false
            };
        }
    }

    [ExecuteInEditMode]
    public class LevelCellBase : MonoBehaviour
    {
        [SerializeField]
        CellInfo m_Info;

        GridObject m_ObjectOnCell;

        [SerializeField]
        [HideInInspector]
        Vector2 m_Index;

        [SerializeField]
        LevelCellMap m_AdjacentCellsMap;

        CellState m_CellState;

        bool m_bTileNaturallyBlocked = false;
        bool m_bMouseIsOver = false;
        bool bIsHovering = false;

        public UnityEvent OnCellDestroyed = new UnityEvent();

        private LevelGridBase m_Grid;

        public LevelGridBase Grid
        {
            get
            {
                if (m_Grid == null)
                {
                    m_Grid = GetComponentInParent<LevelGridBase>();
                }
                return m_Grid;
            }
        }

        public void Reset()
        {
            m_Info = CellInfo.Default();
        }

        void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            m_CellState = CellState.eNormal;
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
            m_AdjacentCellsMap = new LevelCellMap();
        }

        public void ClearAdjacencyList()
        {
            m_AdjacentCellsMap.Pairs.Clear();
        }

        public void AddAdjacentCell(CompassDir InDirection, LevelCellBase InLevelCell)
        {
            if (!m_AdjacentCellsMap.ContainsKey(InDirection))
            {
                m_AdjacentCellsMap.Add(InDirection, InLevelCell);
            }
            else
            {
                Debug.Log("[TurnBasedTools]::LevelCell::AddAdjacentCell) " + gameObject.name + " already has an adjacent cell in the " + InDirection.ToString() + " direction");
            }
        }

        public LevelCellBase AddCellTo(CompassDir InDirection)
        {
            if (!HasAdjacentCell(InDirection))
            {
                LevelCellBase generatedCell = Grid.GenerateCellAdjacentTo(GetIndex(), InDirection);
                return generatedCell;
            }

            return null;
        }

        public void RemoveCell(bool bInRemoveObj)
        {
            if (Grid)
            {
                Grid.RemoveCell(GetIndex(), bInRemoveObj);
            }
        }
        
        #region Getters

        public bool IsMouseOver()
        {
            return m_bMouseIsOver;
        }

        public bool IsBlocked()
        {
            WeightInfo ObjWeightInfo = GetWeightInfo();
            return (ObjWeightInfo.bBlocked || m_bTileNaturallyBlocked) && IsVisible();
        }

        public bool IsFriendlySpawnPoint()
        {
            return m_Info.m_bFriendlySpawnPoint;
        }

        public bool IsHostileSpawnPoint()
        {
            return m_Info.m_bHostileSpawnPoint;
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
            return GetInfo().m_bIsVisible;
        }

        public bool HasAdjacentCell(CompassDir InDirection)
        {
            return m_AdjacentCellsMap.ContainsKey(InDirection) && m_AdjacentCellsMap[InDirection] != null;
        }

        public LevelCellBase GetAdjacentCell(CompassDir InDirection)
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

        public List<LevelCellBase> GetAllAdjacentCells()
        {
            List<LevelCellBase> outCells = new List<LevelCellBase>();

            foreach (var pair in m_AdjacentCellsMap.Pairs)
            {
                outCells.Add(pair._Value);
            }

            return outCells;
        }

        public StatusEffectContainer GetAilmentContainer()
        {
            StatusEffectContainer statusEffectHandler = GetComponent<StatusEffectContainer>();
            if (!statusEffectHandler)
            {
                statusEffectHandler = gameObject.AddComponent<StatusEffectContainer>();
            }

            return statusEffectHandler;
        }

        public CellInfo GetInfo()
        {
            return m_Info;
        }

        public CellState GetNormalState()
        {
            return CellState.eNormal;
        }

        public CellState GetCellState()
        {
            return m_CellState;
        }

        public WeightInfo GetWeightInfo()
        {
            WeightInfo TotalWeightInfo = new WeightInfo();

            ObjectWeightInfo ObjWeightInfo = GetComponent<ObjectWeightInfo>();
            if (ObjWeightInfo)
            {
                TotalWeightInfo += ObjWeightInfo.m_WeightInfo;
            }

            ObjectWeightInfo[] WeightInfos = GetComponentsInChildren<ObjectWeightInfo>();
            foreach (ObjectWeightInfo currWeightInfo in WeightInfos)
            {
                TotalWeightInfo += currWeightInfo.m_WeightInfo;
            }

            List<StatusEffect> ailments = GetAilmentContainer().GetStatusEffectList();
            foreach (StatusEffect currAilment in ailments)
            {
                if(currAilment)
                {
                    CellStatusEffect cellStatusEffect = currAilment as CellStatusEffect;
                    if(cellStatusEffect)
                    {
                        TotalWeightInfo += cellStatusEffect.m_WeightInfo;
                    }
                }
            }

            return TotalWeightInfo;
        }

        public Vector3 GetAllignPos(GridObject InObject)
        {
            float objectHeightOffset = 0.0f;

            GridPawnUnit gridUnit = InObject as GridPawnUnit;
            if (gridUnit)
            {
                objectHeightOffset += gridUnit.GetUnitData().m_HeightOffset;
            }

            Renderer CellRenderer = GetRenderer();
            if (CellRenderer)
            {
                float cellHeight = CellRenderer.bounds.size.y;
                objectHeightOffset += ( cellHeight * 0.5f );

                Vector3 CellPosition = transform.position;
                return CellPosition + new Vector3(0, objectHeightOffset, 0); ;
            }

            return transform.position;
        }

        public Vector3 GetAllignPos(GameObject InObject)
        {
            Renderer CellRenderer = GetRenderer();
            if (CellRenderer)
            {
                Vector3 CellPosition = transform.position;
                Vector3 ObjectBounds = TacticBattleManager.GetBoundsOfObject(InObject);
                Vector3 LevelCellBounds = CellRenderer.bounds.size;

                float heightOffset = (LevelCellBounds.y + (ObjectBounds.y * 0.5f));
                Vector3 AlignPos = CellPosition + new Vector3(0, heightOffset, 0);

                return AlignPos;
            }

            return transform.position;
        }

        public GridObject GetObjectOnCell()
        {
            return m_ObjectOnCell;
        }

        public GridPawnUnit GetUnitOnCell()
        {
            if (m_ObjectOnCell)
            {
                return m_ObjectOnCell as GridPawnUnit;
            }

            return null;
        }

        public BattleTeam GetCellTeam()
        {
            return m_ObjectOnCell ? m_ObjectOnCell.GetTeam() : BattleTeam.None;
        }

        public string GetCellId()
        {
            return GetInfo().m_CellId;
        }

        public Vector2 GetIndex()
        {
            return m_Index;
        }

        public CompassDir GetDirectionToAdjacentCell(LevelCellBase InTarget)
        {
            foreach (var pair in m_AdjacentCellsMap.Pairs)
            {
                if (pair._Value == InTarget)
                {
                    return pair._Key;
                }
            }

            return CompassDir.N;
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

        public virtual void SetMaterial(CellState InCellState)
        {
            //Cannot use base ILevelCell
        }

        public void SetVisible(bool bInVisible)
        {
            m_Info.m_bIsVisible = bInVisible;
            TacticBattleManager.ResetCellState(this);
            HandleVisibilityChanged();
        }

        public void SetObjectOnCell(GridObject InObject)
        {
            m_ObjectOnCell = InObject;
        }

        public void SetCellState(CellState InCellState)
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
            GridPawnUnit unit = GetUnitOnCell();
            if (unit)
            {
                unit.CheckCellVisibility();
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
                    if (Grid)
                    {
                        if (!bIsHovering)
                        {
                            bIsHovering = true;
                            Grid.OnCellBeingInteracted?.Invoke(this, CellInteractionState.eBeginFocused);
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
            if (Grid)
            {
                if (bIsHovering)
                {
                    bIsHovering = false;
                    Grid.OnCellBeingInteracted.Invoke(this, CellInteractionState.eEndFocused);
                }
            }
        }

        #endregion
    }
}
