using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.Maps;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids
{
    public enum CellInteractionState
    {
        eLeftClick,
        eRightClick,
        eMiddleClick,
        eBeginHover,
        eEndHover
    }

    [System.Serializable]
    public class TileReplacedEvent : UnityEvent<ILevelCell>
    { }

    [System.Serializable]
    public class CellInteractionEvent : UnityEvent<ILevelCell, CellInteractionState>
    { }

    [ExecuteInEditMode]
    public class ILevelGrid : MonoBehaviour
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

        public CellInteractionEvent OnCellInteraction;

        public ILevelCell this[Vector2 InIndex]
        {
            get
            {
                int xIndex = (int)InIndex.x;
                int yIndex = (int)InIndex.y;

                return this[xIndex, yIndex];
            }
        }
        public ILevelCell this[int InX, int InY]
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

        public virtual ILevelCell AddLevelCellToObject(GameObject InObj)
        {
            if (!InObj)
            {
                return null;
            }
            
            return InObj.AddComponent<LevelCell>();
        }

        public List<ILevelCell> GetAllCells()
        {
            List<ILevelCell> AllCells = new List<ILevelCell>();
            foreach (var pair in m_CellMap.Pairs)
            {
                AllCells.Add(pair._Value);
            }
            return AllCells;
        }
        public List<ILevelCell> GetTeamStartPoints(GameTeam InTeam)
        {
            List<ILevelCell> outCells = new List<ILevelCell>();

            if(InTeam == GameTeam.None)
            {
                Debug.Log("([TurnBasedTools]::LevelGrid::GetTeamStartPoint) Trying to get start points for invalid team. Start cells don't exist for: " + InTeam.ToString());
                return outCells;
            }

            foreach (var CellPair in m_CellMap.Pairs)
            {
                ILevelCell CurrCell = CellPair._Value;
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

        public ILevelCell ReplaceTileWith(Vector2 InIndex, GameObject InObject)
        {
            m_CellObjCursor = InObject;

            ILevelCell OldCell = this[InIndex];
            Vector3 pos = OldCell.gameObject.transform.position;

            RemoveCell(InIndex, true);
            ILevelCell NewCell = GenerateCell(pos, InIndex);
            SetupAllCellAdjacencies();

            OnTileReplaced.Invoke(NewCell);

            return NewCell;
        }
        public ILevelCell GenerateCellAdjacentTo(Vector2 InOriginalIndex, CompassDir InDirection)
        {
            ILevelCell referenceCell = this[InOriginalIndex];
            ILevelCell newCell = this[GetIndex(InOriginalIndex, InDirection)];
            if (referenceCell && !newCell)
            {
                Vector2 pos = GetPosition(InOriginalIndex, InDirection);
                float height = referenceCell.gameObject.transform.position.y;

                ILevelCell generatedCell = GenerateCell(new Vector3(pos.x, height, pos.y), GetIndex(InOriginalIndex, InDirection));
                SetupAllCellAdjacencies();

                return generatedCell;
            }

            return null;
        }
        public ILevelCell GenerateCell(Vector3 InPos, Vector2 InIndex)
        {
            GameObject generatedCell = Instantiate(m_CellObjCursor, InPos, m_CellObjCursor.transform.rotation, gameObject.transform);
            generatedCell.name = "CELL: " + InIndex.x + ", " + InIndex.y;

            ILevelCell newCell = AddLevelCellToObject(generatedCell);
            newCell.Setup();
            newCell.SetIndex(InIndex);

            AddCell(newCell);

            if( Application.isEditor && !Application.isPlaying )
            {
                EditorUtility.SetDirty(generatedCell);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            return newCell;
        }

        public void HandleCellInteraction(ILevelCell InCell, CellInteractionState InteractionState)
        {
            OnCellInteraction.Invoke(InCell, InteractionState);
        }

        public void RemoveCell(Vector2 InIndex, bool bInDestroyObject)
        {
            ILevelCell CellToRemove = m_CellMap[InIndex];
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

        public List<ILevelCell> GetCellsById(string InCellId)
        {
            List<ILevelCell> outCells = new List<ILevelCell>();

            List<ILevelCell> allCells = GetAllCells();
            foreach (ILevelCell cell in allCells)
            {
                if(cell.GetCellId() == InCellId)
                {
                    outCells.Add(cell);
                }
            }

            return outCells;
        }

        #region Virtual

        protected virtual void SetupAdjacencies(ILevelCell InCell)
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
        protected virtual Dictionary<CompassDir, Vector2> GetRelativeIndicesMap(ILevelCell InCell)
        {
            Debug.Log("([TurnBasedTools]::ILevelGrid) Cannot use ILevelGrid as itself, you must use the HexagonGrid, or SquareGrid");
            return new Dictionary<CompassDir, Vector2>();
        }

        #endregion

        #region Private

        void AddCell(ILevelCell InCell)
        {
            m_CellMap.Add(InCell.GetIndex(), InCell);
        }
        void DestroyDeletedObjects()
        {
            StartCoroutine(DeleteObjects());
        }
        void RefreshIndexMap()
        {
            ILevelCell[] cells = GetComponentsInChildren<ILevelCell>();

            m_CellMap.Clear();
            foreach (ILevelCell currCell in cells)
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
