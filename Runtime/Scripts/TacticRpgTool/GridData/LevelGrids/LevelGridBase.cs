using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.Maps;
using System;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids
{
    public enum CellInteractionState
    {
        eBeginFocused,
        eEndFocused
    }

    [System.Serializable]
    public class TileReplacedEvent : UnityEvent<LevelCellBase>
    { }

    public class LevelGridBase : MonoBehaviour
    {
        [SerializeField]
        protected GridCellMap m_CellMap;

        [SerializeField]
        protected GameObject m_CellObjCursor;

        [SerializeField]
        public CellPalette m_CellPalette;

        [SerializeField]
        public TileReplacedEvent OnTileReplaced;

        public List<GameObject> m_ObjectsToDestroy = new List<GameObject>();

        public Action<LevelCellBase, CellInteractionState> OnCellBeingInteracted;

        public LevelCellBase this[Vector2 InIndex]
        {
            get
            {
                int xIndex = (int)InIndex.x;
                int yIndex = (int)InIndex.y;

                return this[xIndex, yIndex];
            }
        }
        
        public LevelCellBase this[int InX, int InY]
        {
            get
            {
                Vector2 Index = new Vector2(InX, InY);
                if (m_CellMap.ContainsKey(Index))
                {
                    return m_CellMap[Index];
                }
                else
                {
                    return null;
                }
            }
        }

        public void Setup()
        {
            m_CellMap = new GridCellMap();
            OnTileReplaced = new TileReplacedEvent();
        }

        public void SetPrefabCursor(GameObject InCellObj)
        {
            m_CellObjCursor = InCellObj;
        }

        public void SetTileList(CellPalette InTileList)
        {
            m_CellPalette = InTileList;
        }

        public virtual LevelCellBase AddLevelCellToObject(GameObject InObj)
        {
            if (!InObj)
            {
                return null;
            }
            
            return InObj.AddComponent<LevelCellImp>();
        }

        public List<LevelCellBase> GetAllCells()
        {
            List<LevelCellBase> AllCells = new List<LevelCellBase>();
            foreach (var pair in m_CellMap.Pairs)
            {
                AllCells.Add(pair._Value);
            }
            return AllCells;
        }
        public List<LevelCellBase> GetTeamStartPoints(GameTeam InTeam)
        {
            List<LevelCellBase> outCells = new List<LevelCellBase>();

            if(InTeam == GameTeam.None)
            {
                Debug.Log("([TurnBasedTools]::LevelGrid::GetTeamStartPoint) Trying to get start points for invalid team. Start cells don't exist for: " + InTeam.ToString());
                return outCells;
            }

            foreach (var CellPair in m_CellMap.Pairs)
            {
                LevelCellBase CurrCell = CellPair._Value;
                if (CurrCell)
                {
                    switch (InTeam)
                    {
                        case GameTeam.Friendly:
                            if (CurrCell.IsFriendlySpawnPoint())
                            {
                                outCells.Add(CurrCell);
                            }
                            break;
                        case GameTeam.Hostile:
                            if (CurrCell.IsHostileSpawnPoint())
                            {
                                outCells.Add(CurrCell);
                            }
                            break;
                        case GameTeam.All:
                            {
                                outCells.Add(CurrCell);
                            }
                            break;
                        default:
                            break;
                    }

                }
            }

            return outCells;
        }
        public CellPalette GetCellPalette()
        {
            return m_CellPalette;
        }

        public LevelCellBase ReplaceTileWith(Vector2 InIndex, GameObject InObject)
        {
            m_CellObjCursor = InObject;

            LevelCellBase OldCell = this[InIndex];
            Vector3 pos = OldCell.gameObject.transform.position;

            RemoveCell(InIndex, true);
            LevelCellBase NewCell = GenerateCell(pos, InIndex);
            SetupAllCellAdjacencies();

            OnTileReplaced.Invoke(NewCell);

            return NewCell;
        }
        public LevelCellBase GenerateCellAdjacentTo(Vector2 InOriginalIndex, CompassDir InDirection)
        {
            LevelCellBase referenceCell = this[InOriginalIndex];
            LevelCellBase newCell = this[GetIndex(InOriginalIndex, InDirection)];
            if (referenceCell && !newCell)
            {
                Vector2 pos = GetPosition(InOriginalIndex, InDirection);
                float height = referenceCell.gameObject.transform.position.y;

                LevelCellBase generatedCell = GenerateCell(new Vector3(pos.x, height, pos.y), GetIndex(InOriginalIndex, InDirection));
                SetupAllCellAdjacencies();

                return generatedCell;
            }

            return null;
        }

        public LevelCellBase GenerateCell(Vector3 InPos, Vector2 InIndex)
        {
            GameObject generatedCell = Instantiate(m_CellObjCursor, InPos, m_CellObjCursor.transform.rotation, gameObject.transform);
            generatedCell.name = "CELL: " + InIndex.x + ", " + InIndex.y;

            LevelCellBase newCell = AddLevelCellToObject(generatedCell);
            newCell.Setup();
            newCell.SetIndex(InIndex);

            AddCell(newCell);
            return newCell;
        }

        public void RemoveCell(Vector2 InIndex, bool bInDestroyObject)
        {
            LevelCellBase CellToRemove = m_CellMap[InIndex];
            if (CellToRemove)
            {
                m_CellMap.Remove(InIndex);
                if (bInDestroyObject)
                {
                    m_ObjectsToDestroy.Add(CellToRemove.gameObject);
                    DestroyDeletedObjects();
                }

                SetupAllCellAdjacencies();
            }
        } 
        public void SetupAllCellAdjacencies()
        {
            foreach (var item in m_CellMap.Pairs)
            {
                item._Value.ClearAdjacencyList();
                SetupAdjacencies(item._Value);
            }
        }

        public List<LevelCellBase> GetCellsById(string InCellId)
        {
            List<LevelCellBase> outCells = new List<LevelCellBase>();

            List<LevelCellBase> allCells = GetAllCells();
            foreach (LevelCellBase cell in allCells)
            {
                if(cell.GetCellId() == InCellId)
                {
                    outCells.Add(cell);
                }
            }

            return outCells;
        }

        #region Virtual

        protected virtual void SetupAdjacencies(LevelCellBase InCell)
        {
            Debug.Log("([TurnBasedTools]::ILevelGrid) Cannot use ILevelGrid as itself, you must use the HexagonGrid, or SquareGrid");
        }
        protected virtual Vector2 GetIndex(Vector2 InOriginalIndex, CompassDir InDirection)
        {
            Debug.Log("([TurnBasedTools]::ILevelGrid) Cannot use ILevelGrid as itself, you must use the HexagonGrid, or SquareGrid");
            return Vector2.zero;
        }
        protected virtual Vector2 GetPosition(Vector2 OriginalIndex, CompassDir dir)
        {
            Debug.Log("([TurnBasedTools]::ILevelGrid) Cannot use ILevelGrid as itself, you must use the HexagonGrid, or SquareGrid");
            return Vector2.zero;
        }
        protected virtual Vector2 GetOffsetFromDirection(CompassDir dir)
        {
            Debug.Log("([TurnBasedTools]::ILevelGrid) Cannot use ILevelGrid as itself, you must use the HexagonGrid, or SquareGrid");
            return Vector2.zero;
        }
        protected virtual Dictionary<CompassDir, Vector2> GetRelativeIndicesMap(LevelCellBase InCell)
        {
            Debug.Log("([TurnBasedTools]::ILevelGrid) Cannot use ILevelGrid as itself, you must use the HexagonGrid, or SquareGrid");
            return new Dictionary<CompassDir, Vector2>();
        }

        #endregion

        #region Private

        void AddCell(LevelCellBase InCell)
        {
            m_CellMap.Add(InCell.GetIndex(), InCell);
        }
        void DestroyDeletedObjects()
        {
            StartCoroutine(DeleteObjects());
        }
        void RefreshIndexMap()
        {
            LevelCellBase[] cells = GetComponentsInChildren<LevelCellBase>();

            m_CellMap.Clear();
            foreach (LevelCellBase currCell in cells)
            {
                m_CellMap.Add(currCell.GetIndex(), currCell);
            }
        }

        IEnumerator DeleteObjects()
        {
            GameObject[] objsToDestroy = new GameObject[m_ObjectsToDestroy.Count];
            m_ObjectsToDestroy.CopyTo(objsToDestroy);
            m_ObjectsToDestroy.Clear();

            yield return new WaitForSeconds(0.1f);

            foreach (GameObject obj in objsToDestroy)
            {
                if (Application.isPlaying)
                {
                    Destroy(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
            }

            RefreshIndexMap();
        }

        #endregion
    }
}
