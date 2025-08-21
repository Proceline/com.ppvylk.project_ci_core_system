using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.Maps;
using System;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids
{
    public enum CellInteractionState
    {
        BeginFocused,
        EndFocused
    }

    public abstract class LevelGridBase : MonoBehaviour
    {
        private GridCellMap _cellMap;

        protected GameObject CellObjCursor;

        private readonly List<GameObject> _objectsToDestroy = new();

        public Action<LevelCellBase, CellInteractionState> OnCellBeingInteracted;

        public LevelCellBase this[Vector2Int InIndex] => this[InIndex.x, InIndex.y];

        public LevelCellBase this[int InX, int InY]
        {
            get
            {
                Vector2Int index = new Vector2Int(InX, InY);
                if (_cellMap.ContainsKey(index))
                {
                    return _cellMap[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public void Setup()
        {
            _cellMap = new GridCellMap();
        }

        public void SetPrefabCursor(GameObject InCellObj)
        {
            CellObjCursor = InCellObj;
        }

        private LevelCellBase AddLevelCellToObject(GameObject InObj)
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
            foreach (var pair in _cellMap.Pairs)
            {
                allCells.Add(pair.Value);
            }
            return allCells;
        }

        public abstract Dictionary<Vector2Int, RaycastHit> ScanGridRayHits(
            Vector3 center,
            Vector3 cellSize,
            Vector2Int gridSize,
            float maxDistance = 100f,
            LayerMask layerMask = default);
        
        public LevelCellBase GenerateCell(Vector3 InPos, Vector2Int InIndex)
        {
            GameObject generatedCell = Instantiate(CellObjCursor, InPos, CellObjCursor.transform.rotation, gameObject.transform);
            generatedCell.name = "CELL: " + InIndex.x + ", " + InIndex.y;

            LevelCellBase newCell = AddLevelCellToObject(generatedCell);
            newCell.Setup(this);
            newCell.SetIndex(InIndex);

            AddCell(newCell);
            return newCell;
        }

        public void RemoveCell(Vector2 InIndex, bool bInDestroyObject)
        {
            LevelCellBase cellToRemove = _cellMap[InIndex];
            if (cellToRemove)
            {
                _cellMap.Remove(InIndex);
                if (bInDestroyObject)
                {
                    _objectsToDestroy.Add(cellToRemove.gameObject);
                    DestroyDeletedObjects();
                }

                SetupAllCellAdjacencies();
            }
        }

        public void RemoveAllCells()
        {
            foreach (var levelCellPair in _cellMap.Pairs)
            {
                _objectsToDestroy.Add(levelCellPair.Value.gameObject);
            }
            DestroyDeletedObjects();
        }
        
        public void SetupAllCellAdjacencies()
        {
            foreach (var item in _cellMap.Pairs)
            {
                item.Value.ClearAdjacencyList();
                SetupAllNeighbors(item.Value);
            }
        }

        #region Virtual

        protected abstract void SetupAllNeighbors(LevelCellBase InCell);

        protected abstract Vector2Int GetIndex(Vector2Int InOriginalIndex, CompassDir InDirection);

        protected abstract Vector2 GetPosition(Vector2Int originalIndex, CompassDir dir);

        protected abstract Vector2Int GetOffsetFromDirection(CompassDir dir);

        protected abstract Dictionary<CompassDir, Vector2Int> GetRelativeIndicesMap(LevelCellBase InCell);

        #endregion

        #region Private

        void AddCell(LevelCellBase InCell)
        {
            _cellMap.Add(InCell.GetIndex(), InCell);
        }
        void DestroyDeletedObjects()
        {
            StartCoroutine(DeleteObjects());
        }
        void RefreshIndexMap()
        {
            LevelCellBase[] cells = GetComponentsInChildren<LevelCellBase>();

            _cellMap.Clear();
            foreach (LevelCellBase currCell in cells)
            {
                _cellMap.Add(currCell.GetIndex(), currCell);
            }
        }

        IEnumerator DeleteObjects()
        {
            GameObject[] objsToDestroy = new GameObject[_objectsToDestroy.Count];
            _objectsToDestroy.CopyTo(objsToDestroy);
            _objectsToDestroy.Clear();

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
