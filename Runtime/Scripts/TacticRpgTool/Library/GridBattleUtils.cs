using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library
{
    public static class GridBattleUtils
    {
        /// <summary>
        /// 生成六边形格点的RaycastHit信息（不生成Cell，仅采集地面碰撞点）
        /// </summary>
        /// <param name="center">扫描中心点</param>
        /// <param name="cellSize">单个六边形格子的尺寸（x=宽，z=高）</param>
        /// <param name="gridSize">网格尺寸（x=列数，y=行数）</param>
        /// <param name="maxDistance">射线最大距离</param>
        /// <param name="layerMask">射线检测层</param>
        /// <returns>字典，key为格子坐标(x, y)，value为RaycastHit</returns>
        public static Dictionary<Vector2Int, RaycastHit> ScanHexGridRaycastHits(
            Vector3 center,
            Vector3 cellSize,
            Vector2Int gridSize,
            float maxDistance = 100f,
            LayerMask layerMask = default)
        {
            Dictionary<Vector2Int, RaycastHit> hitResults = new Dictionary<Vector2Int, RaycastHit>();

            for (int y = 0; y < gridSize.y; y++)
            {
                float offset = 0;
                if (y % 2 == 0)
                {
                    offset = cellSize.x * 0.5f;
                }

                float finalY = y * cellSize.z * 0.75f;

                for (int x = 0; x < gridSize.x; x++)
                {
                    float finalX = x * cellSize.x + offset;
                    Vector3 worldPos = center + new Vector3(finalX, 0.0f, -finalY);

                    Ray ray = new Ray(worldPos, Vector3.down);
                    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
                    {
                        hitResults[new Vector2Int(x, y)] = hit;
                    }
                }
            }

            return hitResults;
        }

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
        /// <returns>生成的LevelGrid对象</returns>
        public static T GenerateLevelGridFromGround<T>(
            Vector3 scanCenter,
            float hexWidth,
            float hexHeight,
            Vector2Int gridSize,
            LayerMask groundLayer,
            CellPalette cellPalette) where T : LevelGridBase
        {
            // 1. 验证参数
            if (cellPalette == null)
            {
                Debug.LogError("([GridBattleUtils]::GenerateLevelGridFromGround) Missing CellPalette");
                return null;
            }

            if (cellPalette.m_CellPieces.Length == 0 || cellPalette.m_CellPieces[0].m_Cells.Length == 0)
            {
                Debug.LogError("([GridBattleUtils]::GenerateLevelGridFromGround) CellPalette is missing tiles");
                return null;
            }

            var hitPoints = ScanHexGridRaycastHits(
                scanCenter,
                new Vector3(hexWidth, 0, hexHeight),
                gridSize,
                100f,
                groundLayer
            );

            // 3. 创建网格容器
            GameObject gridObject = new GameObject("Ground Based Grid");
            T levelGrid = gridObject.AddComponent<T>();

            // 4. 设置LevelGrid
            levelGrid.Setup();
            levelGrid.SetPrefabCursor(cellPalette.m_CellPieces[0].m_Cells[0]);
            levelGrid.SetTileList(cellPalette);

            // 5. 为每个碰撞点创建网格
            foreach (var hit in hitPoints)
            {
                var cell = levelGrid.GenerateCell(hit.Value.point, hit.Key);
                cell.Reset();
            }

            // 6. 设置网格相邻关系
            levelGrid.SetupAllCellAdjacencies();

            return levelGrid;
        }

        private static T SetupBattleUnit<T>(
            GameObject originPawn,
            LevelGridBase InGrid,
            SoUnitData InUnitData,
            BattleTeam InTeam,
            List<UnitAbilityCore> InAbilities,
            LevelCellBase cell) where T : GridPawnUnit
        {
            T SpawnedGridUnit = originPawn.AddComponent<T>();
            SpawnedGridUnit.Initialize();
            SpawnedGridUnit.SetUnitData(InUnitData);
            SpawnedGridUnit.SetAbilities(InAbilities);
            SpawnedGridUnit.SetTeam(InTeam);
            SpawnedGridUnit.SetGrid(InGrid);
            SpawnedGridUnit.SetCurrentCell(cell);
            SpawnedGridUnit.AlignToGrid();
            SpawnedGridUnit.PostInitialize();
            return SpawnedGridUnit;
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
            List<UnitAbilityCore> InAbilities,
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

            T SpawnedGridUnit = SetupBattleUnit<T>(originPawn, InGrid, InUnitData, InTeam, InAbilities, cell);

            LevelCellBase DirCell = SpawnedGridUnit.GetCell().GetAdjacentCell(InStartDirection);
            if (DirCell)
            {
                SpawnedGridUnit.LookAtCell(DirCell);
            }

            TacticBattleManager.AddUnitToTeam(SpawnedGridUnit, InTeam);

            return SpawnedGridUnit;
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