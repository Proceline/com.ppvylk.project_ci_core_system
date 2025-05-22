using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData
{
    public enum BattleTeam
    {
        None,
        Friendly,
        Hostile,
        All
    }

    public class GridObject : MonoBehaviour
    {
        BattleTeam m_Team;

        Renderer m_ObjectRenderer;

        public UnityEvent OnGetConfirmed = new UnityEvent();
        public UnityEvent OnGetCanceled = new UnityEvent();
        public UnityEvent OnBegingFocused = new UnityEvent();
        public UnityEvent OnEndFocused = new UnityEvent();

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

        public void SetTeam(BattleTeam InTeam)
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

        public BattleTeam GetTeam()
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

        public virtual void HandleBeingConfirmed()
        {
            OnGetConfirmed.Invoke();
        }

        public virtual void HandleBeingCanceled()
        {
            OnGetCanceled.Invoke();
        }

        public virtual void HandleBegingFocused()
        {
            OnBegingFocused.Invoke();
        }

        public virtual void HandleEndFocused()
        {
            OnEndFocused.Invoke();
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
