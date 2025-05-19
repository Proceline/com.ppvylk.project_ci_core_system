using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.Interfaces;
using ProjectCI.CoreSystem.Runtime.Levels;
using ProjectCI.CoreSystem.Runtime.Enums;

namespace ProjectCI.CoreSystem.Runtime.Units.Components
{
    /// <summary>
    /// Base class for all objects that can be placed on the battle grid
    /// </summary>
    public class BattleObject : MonoBehaviour, IObject
    {
        [SerializeField]
        protected bool m_IsVisible = true;

        [SerializeField]
        protected Vector3 m_Bounds = Vector3.one;

        [SerializeField]
        protected BattleTeam m_Team = BattleTeam.None;

        // Events
        public UnityEvent OnLeftClick { get; } = new UnityEvent();
        public UnityEvent OnRightClick { get; } = new UnityEvent();
        public UnityEvent OnMiddleClick { get; } = new UnityEvent();
        public UnityEvent OnHoverBegin { get; } = new UnityEvent();
        public UnityEvent OnHoverEnd { get; } = new UnityEvent();

        // Properties
        public bool IsVisible => m_IsVisible;
        public Vector3 Bounds => m_Bounds;
        public Vector3 Position => transform.position;
        public BattleTeam Team => m_Team;

        protected BattleCell m_OwnerCell;
        protected BattleGrid m_Grid;

        protected virtual void Awake()
        {
            // Base Awake logic
        }

        public virtual void Initialize()
        {
            // Base initialization logic
        }

        public virtual void PostInitialize()
        {
            // Post initialization logic
        }

        public virtual void AlignToGrid()
        {
            if (m_OwnerCell != null)
            {
                transform.position = m_OwnerCell.GetAllignPos(gameObject);
            }
        }

        public virtual void SetVisible(bool visible)
        {
            m_IsVisible = visible;
            
            // Update renderer visibility
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = visible;
            }

            // Update collider visibility
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = visible;
            }
        }

        public virtual void SetGrid(BattleGrid grid)
        {
            m_Grid = grid;
        }

        public virtual void SetCurrentCell(BattleCell cell)
        {
            if (m_OwnerCell != null)
            {
                m_OwnerCell.SetObjectOnCell(null);
            }
            m_OwnerCell = cell;
            if (m_OwnerCell != null)
            {
                m_OwnerCell.SetObjectOnCell(this);
                HandleOwnerCellChanged(m_OwnerCell);
            }
        }

        protected virtual void HandleOwnerCellChanged(BattleCell newCell)
        {
            // Base implementation - can be overridden by derived classes
        }

        public virtual void HandleLeftClick()
        {
            OnLeftClick?.Invoke();
        }

        public virtual void HandleRightClick()
        {
            OnRightClick?.Invoke();
        }

        public virtual void HandleMiddleClick()
        {
            OnMiddleClick?.Invoke();
        }

        public virtual void HandleHoverBegin()
        {
            OnHoverBegin?.Invoke();
        }

        public virtual void HandleHoverEnd()
        {
            OnHoverEnd?.Invoke();
        }

        public virtual void SetOwnerCell(BattleCell cell)
        {
            if (m_OwnerCell != cell)
            {
                m_OwnerCell = cell;
                AlignToGrid();
            }
        }

        public virtual BattleCell GetOwnerCell()
        {
            return m_OwnerCell;
        }

        public virtual BattleCell GetCell()
        {
            return m_OwnerCell;
        }

        public virtual void SetTeam(BattleTeam team)
        {
            m_Team = team;
        }

        protected virtual void OnDestroy()
        {
            // Clean up events
            OnLeftClick.RemoveAllListeners();
            OnRightClick.RemoveAllListeners();
            OnMiddleClick.RemoveAllListeners();
            OnHoverBegin.RemoveAllListeners();
            OnHoverEnd.RemoveAllListeners();
        }
    }
} 