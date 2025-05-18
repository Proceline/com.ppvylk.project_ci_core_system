using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using ProjectCI.CoreSystem.Runtime.Enums;
using ProjectCI.CoreSystem.Runtime.Levels.Data;
using ProjectCI.CoreSystem.Runtime.Levels.Enums;
using ProjectCI.CoreSystem.Runtime.Levels.ScriptableObjects;

namespace ProjectCI.CoreSystem.Runtime.Levels
{
    [System.Serializable]
    public class TileReplacedEvent : UnityEvent<BattleCell>
    { }

    [System.Serializable]
    public class CellInteractionEvent : UnityEvent<BattleCell, CellInteractionState>
    { }

    [ExecuteInEditMode]
    public abstract class BattleGrid : MonoBehaviour
    {
        [SerializeField]
        protected BattleGridCellMap m_CellMap;

        [SerializeField]
        protected GameObject m_CellObjCursor;

        [SerializeField]
        public BattleCellPalette m_CellPalette;

        [SerializeField]
        public TileReplacedEvent OnTileReplaced;

        public List<GameObject> m_ObjectsToDestroy = new List<GameObject>();

        public CellInteractionEvent OnCellInteraction;

        public BattleCell this[Vector2 InIndex]
        {
            get
            {
                if (m_CellMap.ContainsKey(InIndex))
                {
                    return m_CellMap[InIndex];
                }
                return null;
            }
        }

        public BattleCell this[int InX, int InY]
        {
            get
            {
                return this[new Vector2(InX, InY)];
            }
        }

        public void Setup()
        {
            m_CellMap = new BattleGridCellMap();
            OnTileReplaced = new TileReplacedEvent();
        }

        public void SetPrefabCursor(GameObject InCellObj)
        {
            m_CellObjCursor = InCellObj;
        }

        public void SetTileList(BattleCellPalette InTileList)
        {
            m_CellPalette = InTileList;
        }

        public abstract BattleCell AddLevelCellToObject(GameObject InObj);

        public List<BattleCell> GetAllCells()
        {
            List<BattleCell> AllCells = new List<BattleCell>();
            foreach (var pair in m_CellMap.Pairs)
            {
                AllCells.Add(pair.Cell);
            }
            return AllCells;
        }

        public List<BattleCell> GetTeamStartPoints(BattleTeam InTeam)
        {
            List<BattleCell> outCells = new List<BattleCell>();

            if(InTeam == BattleTeam.None)
            {
                Debug.Log("[ProjectCI] BattleGrid::GetTeamStartPoint) Trying to get start points for invalid team. Start cells don't exist for: " + InTeam.ToString());
                return outCells;
            }

            foreach (var CellPair in m_CellMap.Pairs)
            {
                BattleCell CurrCell = CellPair.Cell;
                if (CurrCell)
                {
                    switch (InTeam)
                    {
                        case BattleTeam.Player:
                            if (CurrCell.IsFriendlySpawnPoint())
                            {
                                outCells.Add(CurrCell);
                            }
                            break;
                        case BattleTeam.Enemy:
                            if (CurrCell.IsHostileSpawnPoint())
                            {
                                outCells.Add(CurrCell);
                            }
                            break;
                        case BattleTeam.Neutral:
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

        public BattleCellPalette GetCellPalette()
        {
            return m_CellPalette;
        }

        public BattleCell ReplaceTileWith(Vector2 InIndex, GameObject InObject)
        {
            m_CellObjCursor = InObject;

            BattleCell OldCell = this[InIndex];
            Vector3 pos = OldCell.gameObject.transform.position;

            RemoveCell(InIndex, true);
            BattleCell NewCell = GenerateCell(pos, InIndex);
            SetupAllCellAdjacencies();

            OnTileReplaced.Invoke(NewCell);

            return NewCell;
        }

        public BattleCell GenerateCellAdjacentTo(Vector2 InOriginalIndex, GridDirection InDirection)
        {
            BattleCell referenceCell = this[InOriginalIndex];
            BattleCell newCell = this[GetIndex(InOriginalIndex, InDirection)];
            if (referenceCell && !newCell)
            {
                Vector2 pos = GetPosition(InOriginalIndex, InDirection);
                float height = referenceCell.gameObject.transform.position.y;

                BattleCell generatedCell = GenerateCell(new Vector3(pos.x, height, pos.y), GetIndex(InOriginalIndex, InDirection));
                SetupAllCellAdjacencies();

                return generatedCell;
            }

            return null;
        }

        public BattleCell GenerateCell(Vector3 InPos, Vector2 InIndex)
        {
            GameObject generatedCell = Instantiate(m_CellObjCursor, InPos, m_CellObjCursor.transform.rotation, gameObject.transform);
            generatedCell.name = "CELL: " + InIndex.x + ", " + InIndex.y;

            BattleCell newCell = AddLevelCellToObject(generatedCell);
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

        public void HandleCellInteraction(BattleCell InCell, CellInteractionState InteractionState)
        {
            OnCellInteraction.Invoke(InCell, InteractionState);
        }

        public void RemoveCell(Vector2 InIndex, bool bInDestroyObject)
        {
            BattleCell CellToRemove = m_CellMap[InIndex];
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
                item.Cell.ClearAdjacencyList();
                SetupAdjacencies(item.Cell);
            }
        }

        public List<BattleCell> GetCellsById(string InCellId)
        {
            List<BattleCell> outCells = new List<BattleCell>();

            List<BattleCell> allCells = GetAllCells();
            foreach (BattleCell cell in allCells)
            {
                if(cell.GetCellId() == InCellId)
                {
                    outCells.Add(cell);
                }
            }

            return outCells;
        }

        #region Virtual Methods
        // TODO: Implement SetupAdjacencies
        protected virtual void SetupAdjacencies(BattleCell InCell)
        {
            Debug.Log("[ProjectCI] BattleGrid) Cannot use BattleGrid as itself, you must use the HexagonGrid, or SquareGrid");
        }

        // TODO: Implement GetIndex
        protected virtual Vector2 GetIndex(Vector2 InOriginalIndex, GridDirection InDirection)
        {
            Debug.Log("[ProjectCI] BattleGrid) Cannot use BattleGrid as itself, you must use the HexagonGrid, or SquareGrid");
            return Vector2.zero;
        }

        // TODO: Implement GetPosition
        protected virtual Vector2 GetPosition(Vector2 OriginalIndex, GridDirection dir)
        {
            Debug.Log("[ProjectCI] BattleGrid) Cannot use BattleGrid as itself, you must use the HexagonGrid, or SquareGrid");
            return Vector2.zero;
        }

        // TODO: Implement GetOffsetFromDirection
        protected virtual Vector2 GetOffsetFromDirection(GridDirection dir)
        {
            Debug.Log("[ProjectCI] BattleGrid) Cannot use BattleGrid as itself, you must use the HexagonGrid, or SquareGrid");
            return Vector2.zero;
        }

        // TODO: Implement GetRelativeIndicesMap
        protected virtual Dictionary<GridDirection, Vector2> GetRelativeIndicesMap(BattleCell InCell)
        {
            Debug.Log("[ProjectCI] BattleGrid) Cannot use BattleGrid as itself, you must use the HexagonGrid, or SquareGrid");
            return new Dictionary<GridDirection, Vector2>();
        }
        #endregion

        #region Private Methods
        private void AddCell(BattleCell InCell)
        {
            m_CellMap.Add(InCell.GetIndex(), InCell);
        }

        private void DestroyDeletedObjects()
        {
            StartCoroutine(DeleteObjects());
        }

        private void RefreshIndexMap()
        {
            BattleCell[] cells = GetComponentsInChildren<BattleCell>();

            m_CellMap.Clear();
            foreach (BattleCell currCell in cells)
            {
                m_CellMap.Add(currCell.GetIndex(), currCell);
            }
        }

        private IEnumerator DeleteObjects()
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

        void Start()
        {
            BattleCell[] cells = GetComponentsInChildren<BattleCell>();
            foreach (BattleCell currCell in cells)
            {
                m_CellMap.Add(currCell.GetIndex(), currCell);
            }
        }
    }
} 