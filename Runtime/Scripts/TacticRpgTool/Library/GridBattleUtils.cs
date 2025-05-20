using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.PlayerData;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library
{
    public static class GridBattleUtils
    {
        /// <summary>
        /// Scan ground with raycasts in a hexagon grid pattern
        /// </summary>
        /// <param name="center">Center position of the scan area</param>
        /// <param name="hexWidth">Width of each hexagon cell</param>
        /// <param name="hexHeight">Height of each hexagon cell</param>
        /// <param name="radiusWidth">Width of the hexagon grid</param>
        /// <param name="radiusHeight">Height of the hexagon grid</param>
        /// <param name="maxDistance">Maximum raycast distance</param>
        /// <param name="layerMask">Layer mask for raycast</param>
        /// <param name="useEllipseBoundary">Use ellipse boundary instead of square boundary</param>
        /// <returns>Dictionary with grid positions as keys and hit points as values</returns>
        public static Dictionary<Vector2, Vector3> ScanHexagonGroundGrid(
            Vector3 center,
            float hexWidth,
            float hexHeight,
            int radiusWidth,
            int radiusHeight,
            float maxDistance = 100f,
            LayerMask layerMask = default,
            bool useEllipseBoundary = false)
        {
            Dictionary<Vector2, Vector3> hitPoints = new Dictionary<Vector2, Vector3>();

            for (int q = -radiusWidth; q <= radiusWidth; q++)
            {
                for (int r = -radiusHeight; r <= radiusHeight; r++)
                {
                    // 椭圆形边界判断（默认不用）
                    if (useEllipseBoundary)
                    {
                        float normQ = (float)q / radiusWidth;
                        float normR = (float)r / radiusHeight;
                        if (normQ * normQ + normR * normR > 1f)
                            continue;
                    }

                    Vector3 pos = HexToWorld(center, q, r, hexWidth, hexHeight);
                    Ray ray = new Ray(pos, Vector3.down);
#if UNITY_EDITOR
                    Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.magenta, 2f);
#endif
                    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
                    {
                        hitPoints[new Vector2(q, r)] = hit.point;
                    }
                }
            }
            return hitPoints;
        }

        /// <summary>
        /// 六边形轴坐标转世界坐标
        /// </summary>
        private static Vector3 HexToWorld(Vector3 center, int q, int r, float hexWidth, float hexHeight)
        {
            float x = hexWidth * (q + r / 2f);
            float z = hexHeight * r * 0.75f;
            return center + new Vector3(x, 0, z);
        }

        /// <summary>
        /// Creates hexagon cell objects based on the hit points returned by ScanHexagonGroundGrid.
        /// </summary>
        /// <param name="hitPoints">Dictionary with grid positions as keys and hit points as values</param>
        /// <param name="hexWidth">Width of each hexagon cell</param>
        /// <param name="hexHeight">Height of each hexagon cell</param>
        /// <param name="parent">Optional parent transform for the created cells</param>
        /// <param name="prefab">Optional prefab to instantiate for each cell</param>
        /// <returns>List of created hexagon cell GameObjects</returns>
        public static List<GameObject> CreateHexagonCellsFromHits(Dictionary<Vector2, Vector3> hitPoints, float hexWidth, float hexHeight, Transform parent = null, GameObject prefab = null)
        {
            List<GameObject> cells = new List<GameObject>();
            foreach (var hit in hitPoints)
            {
                GameObject cell;
                if (prefab != null)
                {
                    cell = Object.Instantiate(prefab, hit.Value, Quaternion.identity);
                }
                else
                {
                    cell = new GameObject($"HexCell_{hit.Key.x}_{hit.Key.y}");
                    cell.transform.position = hit.Value;
                    cell.transform.localScale = new Vector3(hexWidth, 0.1f, hexHeight);
                }
                if (parent != null)
                {
                    cell.transform.SetParent(parent);
                }
                cells.Add(cell);
            }
            return cells;
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

            // 2. 扫描地形获取碰撞点
            var hitPoints = ScanHexagonGroundGrid(
                scanCenter,
                hexWidth,
                hexHeight,
                gridSize.x,
                gridSize.y,
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
                var cell =levelGrid.GenerateCell(hit.Value, hit.Key);
                cell.Reset();
            }

            // 6. 设置网格相邻关系
            levelGrid.SetupAllCellAdjacencies();

            return levelGrid;
        }

        private static GridUnit SetupBattleUnit<T>(
            GameObject originPawn,
            LevelGridBase InGrid,
            UnitData InUnitData,
            GameTeam InTeam,
            LevelCellBase cell) where T : GridUnit
        {
            GridUnit SpawnedGridUnit = originPawn.AddComponent<T>();
            SpawnedGridUnit.Initalize();
            SpawnedGridUnit.SetUnitData(InUnitData);
            SpawnedGridUnit.SetTeam(InTeam);
            SpawnedGridUnit.SetGrid(InGrid);
            SpawnedGridUnit.SetCurrentCell(cell);
            SpawnedGridUnit.AlignToGrid();
            SpawnedGridUnit.PostInitalize();
            return SpawnedGridUnit;
        }

        public static GridUnit ChangeUnitToBattleUnit<T>(GameObject originPawn,
            LevelGridBase InGrid, UnitData InUnitData,
            GameTeam InTeam, Vector2 InIndex, CompassDir InStartDirection = CompassDir.S)
            where T : GridUnit
        {
            LevelCellBase cell = InGrid[InIndex];

            if (InTeam == GameTeam.Friendly)
            {
                cell.SetVisible(true);
            }

            GridUnit SpawnedGridUnit = SetupBattleUnit<T>(originPawn, InGrid, InUnitData, InTeam, cell);

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
        /// <param name="humanTeamData">人类队伍数据</param>
        /// <param name="teamData">队伍数据</param>
        /// <returns>创建并初始化好的TacticBattleManager实例</returns>
        public static TacticBattleManager CreateBattleManager(
            TacticBattleManager prefab,
            LevelGridBase levelGrid,
            HumanTeamData humanTeamData,
            TeamData teamData)
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

            if (humanTeamData == null)
            {
                Debug.LogError("[GridBattleUtils]::CreateBattleManager) Missing HumanTeamData");
                return null;
            }

            if (teamData == null)
            {
                Debug.LogError("[GridBattleUtils]::CreateBattleManager) Missing TeamData");
                return null;
            }

            TacticBattleManager instance = Object.Instantiate(prefab);
            instance.name = "TacticBattleManager";
            instance.LevelGrid = levelGrid;
            instance.FriendlyTeamData = humanTeamData;
            instance.HostileTeamData = teamData;
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
    }
} 