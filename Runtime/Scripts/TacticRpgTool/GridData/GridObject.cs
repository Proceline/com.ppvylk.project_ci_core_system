using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData
{
    public enum GameTeam
    {
        None,
        Friendly,
        Hostile,
        All
    }

    public class GridObject : MonoBehaviour
    {
        GameTeam m_Team;

        Renderer m_ObjectRenderer;

        public UnityEvent OnLeftClick = new UnityEvent();
        public UnityEvent OnRightClick = new UnityEvent();
        public UnityEvent OnMiddleClick = new UnityEvent();
        public UnityEvent OnHoverBegin = new UnityEvent();
        public UnityEvent OnHoverEnd = new UnityEvent();

        protected LevelGridBase m_AssociatedGrid;
        protected LevelCellBase m_CurrentCell;

        bool m_bVisible = true;

        public virtual void Initalize()
        {
            DestroyColliders();
        }

        public virtual void PostInitalize()
        {
            
        }

        public void AlignToGrid()
        {
            if (m_CurrentCell)
            {
                transform.position = m_CurrentCell.GetAllignPos(this);
            }
        }

        void DestroyColliders()//Prevents the object from blocking the cell RayCast.
        {
            foreach (Transform child in transform)
            {
                Collider[] colliders = child.GetComponents<Collider>();
                foreach (var collider in colliders)
                {
                    DestroyImmediate(collider);
                }
            }

            Collider MainObjCollider = GetComponent<Collider>();
            if (MainObjCollider)
            {
                DestroyImmediate(MainObjCollider);
            }
        }

        #region Setters

        public void SetGrid(LevelGridBase InGrid)
        {
            m_AssociatedGrid = InGrid;
        }

        public void SetTeam(GameTeam InTeam)
        {
            m_Team = InTeam;
        }

        public void SetCurrentCell(LevelCellBase InCell)
        {
            if(m_CurrentCell)
            {
                m_CurrentCell.SetObjectOnCell(null);
            }
            m_CurrentCell = InCell;
            if(m_CurrentCell)
            {
                m_CurrentCell.SetObjectOnCell(this);
                HandleOwnerCellChanged(m_CurrentCell);
            }
        }

        public void SetVisible(bool bVisible)
        {
            if(bVisible != m_bVisible)
            {
                List<Renderer> Renderers = GetAllRenderers();
                foreach (Renderer currRenderer in Renderers)
                {
                    if(currRenderer)
                    {
                        currRenderer.enabled = bVisible;
                    }
                }
            }
            m_bVisible = bVisible;
        }

        #endregion

        #region Getters

        public bool IsVisible()
        {
            return m_bVisible;
        }

        public LevelCellBase GetCell()
        {
            return m_CurrentCell;
        }

        public LevelGridBase GetGrid()
        {
            return m_AssociatedGrid;
        }

        public GameTeam GetTeam()
        {
            return m_Team;
        }

        public Vector3 GetBounds()
        {
            Vector3 bounds = new Vector3();

            List<Renderer> Renderers = GetAllRenderers();
            foreach (Renderer currRenderer in Renderers)
            {
                if (currRenderer)
                {
                    Vector3 rendererBound = currRenderer.bounds.size;
                    bounds.x += rendererBound.x;
                    bounds.y += rendererBound.y;
                    bounds.z += rendererBound.z;
                }
            }

            return bounds;
        }

        List<Renderer> GetAllRenderers()
        {
            List<Renderer> Renderers = new List<Renderer>();

            Renderers.Add(gameObject.GetComponent<Renderer>());
            Renderers.AddRange(gameObject.GetComponentsInChildren<Renderer>());

            return Renderers;
        }

        #endregion

        #region EventListeners

        public void HandleInteraction(CellInteractionState InInteractionState)
        {
            switch ( InInteractionState )
            {
                case CellInteractionState.eLeftClick:
                    OnLeftClick.Invoke();
                    HandleLeftClick();
                    break;
                case CellInteractionState.eRightClick:
                    OnRightClick.Invoke();
                    HandleRightClick();
                    break;
                case CellInteractionState.eMiddleClick:
                    OnMiddleClick.Invoke();
                    HandleMiddleClick();
                    break;
                case CellInteractionState.eBeginHover:
                    OnHoverBegin.Invoke();
                    HandleHoverBegin();
                    break;
                case CellInteractionState.eEndHover:
                    OnHoverEnd.Invoke();
                    HandleHoverEnd();
                    break;
            }
        }

        public virtual void HandleLeftClick()
        {
            
        }

        public virtual void HandleRightClick()
        {

        }

        public virtual void HandleMiddleClick()
        {
            
        }

        public virtual void HandleHoverBegin()
        {

        }

        public virtual void HandleHoverEnd()
        {

        }

        public virtual void HandleOwnerCellChanged(LevelCellBase NewCell)
        {
            
        }

        public virtual void HandleCellStateChanged(CellState InCellState)
        {
            
        }

        #endregion
    }
}
