using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    [ExecuteInEditMode]
    public class DirectionalCellSpawner : MonoBehaviour
    {
        public LevelCellBase ReferenceCell;

        List<CompassDir> m_AllowedDirections = new List<CompassDir>();

        public UnityEvent OnReferenceCellDestroyed = new UnityEvent();

        void Start()
        {

        }

        void Update()
        {

        }

        public void SetCurrentTile(LevelCellBase InCell)
        {
            if ( ReferenceCell )
            {
                ReferenceCell.OnCellDestroyed.RemoveListener( HandleRefCellDestroyed );
            }

            ReferenceCell = InCell;

            if ( ReferenceCell )
            {
                ReferenceCell.OnCellDestroyed.AddListener( HandleRefCellDestroyed );
            }

            UpdateArrows();
        }

        void HandleRefCellDestroyed()
        {
            OnReferenceCellDestroyed.Invoke();
        }

        void UpdateAllowedDirections()
        {
            m_AllowedDirections.Clear();

            if (ReferenceCell)
            {
                if (ReferenceCell.GetGrid() as HexagonGrid)
                {
                    CompassDir[] dirs = { CompassDir.E, CompassDir.NE, CompassDir.NW, CompassDir.SE, CompassDir.SW, CompassDir.W };
                    m_AllowedDirections.AddRange(dirs);
                }
            }
        }

        void UpdateArrows()
        {
            UpdateAllowedDirections();

            if (ReferenceCell)
            {
                ArrowSpawner[] ArrowSpawners = GetComponentsInChildren<ArrowSpawner>();
                foreach (ArrowSpawner arrow in ArrowSpawners)
                {
                    if (arrow)
                    {
                        if (ReferenceCell.HasAdjacentCell(arrow.direction) || !m_AllowedDirections.Contains(arrow.direction))
                        {
                            DestroyImmediate(arrow.gameObject);
                        }
                    }
                }
            }
        }

        public LevelCellBase SpawnTile(CompassDir Direction, bool bSelectNewTile)
        {
            if(ReferenceCell)
            {
                LevelCellBase generatedCell = ReferenceCell.AddCellTo(Direction);
                UpdateArrows();

                return generatedCell;
            }

            return null;
        }
    }
}
