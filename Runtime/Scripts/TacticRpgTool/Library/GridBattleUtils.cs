using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library
{
    public static class GridBattleUtils
    {
        /// <summary>
        /// 基于地形扫描结果生成六边形网格
        /// </summary>
        /// <typeparam name="T">LevelGrid类型</typeparam>
        /// <param name="scanCenter">扫描中心点</param>
        /// <param name="hexWidth">六边形宽度</param>
        /// <param name="hexHeight">六边形高度</param>
        /// <param name="gridSize">网格大小</param>
        /// <param name="groundLayer">地面层</param>
        /// <param name="cellPalette">网格预制体配置</param>
        /// <param name="existedGrid"></param>
        /// <param name="hitHandler"></param>
        /// <returns>生成的LevelGrid对象</returns>
        public static void GenerateLevelGridFromGround<T, TC>(
            Vector3 scanCenter,
            float hexWidth,
            float hexHeight,
            Vector2Int gridSize,
            LayerMask groundLayer,
            CellPalette cellPalette,
            ref T existedGrid,
            UnityEvent<RaycastHit, Vector2Int, LevelGridBase> hitHandler) 
                where T : LevelGridBase
                where TC : LevelCellBase
        {
            if (cellPalette == null)
            {
                throw new Exception("([GridBattleUtils]::GenerateLevelGridFromGround) Missing CellPalette");
            }

            if (cellPalette.m_CellPieces.Length == 0 || cellPalette.m_CellPieces[0].m_Cells.Length == 0)
            {
                throw new Exception("([GridBattleUtils]::GenerateLevelGridFromGround) CellPalette is missing tiles");
            }
            
            if (existedGrid == null)
            {
                GameObject gridObject = new GameObject("Ground Based Grid");
                existedGrid = gridObject.AddComponent<T>();
                existedGrid.Setup();
                existedGrid.SetPrefabCursor(cellPalette.m_CellPieces[0].m_Cells[0]);
            }
            else
            {
                existedGrid.RemoveAllCells();
                existedGrid.Setup();
            }

            var hitPoints = existedGrid.ScanGridRayHits(
                scanCenter,
                new Vector3(hexWidth, 0, hexHeight),
                gridSize,
                100f,
                groundLayer
            );

            // Apply Cell Generation on each hit Point
            foreach (var hit in hitPoints)
            {
                if (hitHandler == null)
                {
                    var cell = existedGrid.GenerateCell<TC>(hit.Value.point, hit.Key);
                    cell.Reset();
                }
                else
                {
                    hitHandler.Invoke(hit.Value, hit.Key, existedGrid);
                }
            }
            
            existedGrid.SetupAllCellAdjacencies();
        }

        private static T SetupBattleUnit<T>(
            GameObject originPawn,
            LevelGridBase InGrid,
            SoUnitData InUnitData,
            BattleTeam InTeam,
            LevelCellBase cell) where T : GridPawnUnit
        {
            T spawnedGridUnit = originPawn.AddComponent<T>();
            spawnedGridUnit.Initialize();
            spawnedGridUnit.SetUnitData(InUnitData);
            spawnedGridUnit.SetTeam(InTeam);
            spawnedGridUnit.SetGrid(InGrid);
            spawnedGridUnit.SetCurrentCell(cell);
            spawnedGridUnit.AlignToGrid();
            spawnedGridUnit.PostInitialize();
            return spawnedGridUnit;
        }

        public static T AddResourceContainerToUnit<T>(GridPawnUnit InUnit, GameObject resourceContainerPrefab)
            where T : BattleHealth
        {
            return InUnit.gameObject.AddComponent<T>();
        }

        /// <summary>
        /// 自动根据originPawn的位置查找最近未被占用的cell并生成战斗单位
        /// </summary>
        public static T ChangeUnitToBattleUnit<T>(
            GameObject originPawn,
            LevelGridBase InGrid,
            SoUnitData InUnitData,
            BattleTeam InTeam,
            Action<T> afterSpawnHandler,
            float searchRadius,
            LayerMask cellLayer,
            CompassDir InStartDirection = CompassDir.S)
            where T : GridPawnUnit
        {
            if (originPawn == null || InGrid == null)
            {
                Debug.LogWarning("originPawn or InGrid is null");
                return null;
            }
            LevelCellBase cell = FindNearestUnoccupiedCell(originPawn.transform.position, searchRadius, cellLayer);
            if (cell == null)
            {
                Debug.LogWarning("No unoccupied cell found near the given position.");
                return null;
            }

            if (InTeam == BattleTeam.Friendly)
            {
                cell.SetVisible(true);
            }

            T spawnedGridUnit = SetupBattleUnit<T>(originPawn, InGrid, InUnitData, InTeam, cell);
            afterSpawnHandler?.Invoke(spawnedGridUnit);

            LevelCellBase dirCell = spawnedGridUnit.GetCell().GetAdjacentCell(InStartDirection);
            if (dirCell)
            {
                spawnedGridUnit.LookAtCell(dirCell);
            }

            TacticBattleManager.AddUnitToTeam(spawnedGridUnit, InTeam);

            return spawnedGridUnit;
        }

        /// <summary>
        /// 根据预制体创建并初始化TacticBattleManager
        /// </summary>
        /// <param name="prefab">TacticBattleManager预制体</param>
        /// <param name="levelGrid">关卡网格</param>
        /// <returns>创建并初始化好的TacticBattleManager实例</returns>
        public static TacticBattleManager CreateBattleManager(
            TacticBattleManager prefab,
            LevelGridBase levelGrid)
        {
            if (prefab == null)
            {
                Debug.LogError("[GridBattleUtils]::CreateBattleManager) Missing TacticBattleManager prefab");
                return null;
            }

            if (levelGrid == null)
            {
                Debug.LogError("[GridBattleUtils]::CreateBattleManager) Missing LevelGrid");
                return null;
            }

            TacticBattleManager instance = Object.Instantiate(prefab);
            instance.name = "TacticBattleManager";
            instance.LevelGrid = levelGrid;
            return instance;
        }

        /// <summary>
        /// 扫描指定区域内的对象
        /// </summary>
        /// <typeparam name="T">要查找的组件类型，必须继承自Component</typeparam>
        /// <param name="center">区域中心点</param>
        /// <param name="radius">如果是圆形区域，则为半径；如果是矩形区域，则为半边长</param>
        /// <param name="isCircle">true为圆形区域，false为矩形区域</param>
        /// <param name="layerMask">要检测的层</param>
        /// <param name="maxResults">最大返回结果数</param>
        /// <returns>区域内的对象列表</returns>
        public static List<T> ScanAreaForObjects<T>(
            Vector3 center,
            float radius,
            bool isCircle = true,
            LayerMask layerMask = default,
            int maxResults = 10) where T : Component
        {
            List<T> foundObjects = new List<T>();
            
            if (isCircle)
            {
                // 圆形区域扫描
                Collider[] colliders = Physics.OverlapSphere(center, radius, layerMask);
                foreach (var collider in colliders)
                {
                    if (foundObjects.Count >= maxResults) break;

                    T component = collider.GetComponent<T>();
                    if (component != null)
                    {
                        foundObjects.Add(component);
                    }
                }
            }
            else
            {
                // 矩形区域扫描
                Vector3 halfExtents = new Vector3(radius, radius, radius);
                Collider[] colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, layerMask);
                foreach (var collider in colliders)
                {
                    if (foundObjects.Count >= maxResults) break;

                    T component = collider.GetComponent<T>();
                    if (component != null)
                    {
                        foundObjects.Add(component);
                    }
                }
            }

            return foundObjects;
        }

        /// <summary>
        /// 查找距离position最近的未被占用的LevelCellBase（通过OverlapSphere）
        /// </summary>
        /// <param name="position">目标点</param>
        /// <param name="searchRadius">搜索半径</param>
        /// <param name="cellLayer">cell所在的LayerMask</param>
        /// <returns>最近的未被占用的LevelCellBase，找不到则返回null</returns>
        public static LevelCellBase FindNearestUnoccupiedCell(Vector3 position, float searchRadius, LayerMask cellLayer)
        {
            Collider[] colliders = Physics.OverlapSphere(position, searchRadius, cellLayer);
            LevelCellBase nearestCell = null;
            float minDist = float.MaxValue;

            foreach (var col in colliders)
            {
                LevelCellBase cell = col.GetComponent<LevelCellBase>();
                if (cell != null && !cell.IsObjectOnCell())
                {
                    float dist = Vector3.Distance(position, cell.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestCell = cell;
                    }
                }
            }
            return nearestCell;
        }
    }
} 