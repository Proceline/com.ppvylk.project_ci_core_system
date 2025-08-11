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

    [Serializable]
    public class TileReplacedEvent : UnityEvent<LevelCellBase>
    { }

    public abstract class LevelGridBase : MonoBehaviour
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

        public LevelCellBase this[Vector2Int InIndex] => this[InIndex.x, InIndex.y];

        public LevelCellBase this[int InX, int InY]
        {
            get
            {
                Vector2Int index = new Vector2Int(InX, InY);
                if (m_CellMap.ContainsKey(index))
                {
                    return m_CellMap[index];
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
            List<LevelCellBase> allCells = new List<LevelCellBase>();
            foreach (var pair in m_CellMap.Pairs)
            {
                allCells.Add(pair._Value);
            }
            return allCells;
        }
        public List<LevelCellBase> GetTeamStartPoints(BattleTeam InTeam)
        {
            List<LevelCellBase> outCells = new List<LevelCellBase>();

            if(InTeam == BattleTeam.None)
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
                        case BattleTeam.Friendly:
                            if (CurrCell.IsFriendlySpawnPoint())
                            {
                                outCells.Add(CurrCell);
                            }
                            break;
                        case BattleTeam.Hostile:
                            if (CurrCell.IsHostileSpawnPoint())
                            {
                                outCells.Add(CurrCell);
                            }
                            break;
                        case BattleTeam.All:
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

        public LevelCellBase GenerateCellAdjacentTo(Vector2Int InOriginalIndex, CompassDir InDirection)
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

        public LevelCellBase GenerateCell(Vector3 InPos, Vector2Int InIndex)
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

        protected abstract void SetupAdjacencies(LevelCellBase InCell);

        protected abstract Vector2Int GetIndex(Vector2Int InOriginalIndex, CompassDir InDirection);

        protected abstract Vector2 GetPosition(Vector2Int originalIndex, CompassDir dir);

        protected abstract Vector2Int GetOffsetFromDirection(CompassDir dir);

        protected abstract Dictionary<CompassDir, Vector2Int> GetRelativeIndicesMap(LevelCellBase InCell);

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
