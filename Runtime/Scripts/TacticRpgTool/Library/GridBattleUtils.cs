using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;

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
        /// <param name="gridWidth">Number of cells in width</param>
        /// <param name="gridHeight">Number of cells in height</param>
        /// <param name="maxDistance">Maximum raycast distance</param>
        /// <param name="layerMask">Layer mask for raycast</param>
        /// <returns>Dictionary with grid positions as keys and hit points as values</returns>
        public static Dictionary<Vector2, Vector3> ScanHexagonGroundGrid(
            Vector3 center,
            float hexWidth,
            float hexHeight,
            int gridWidth,
            int gridHeight,
            float maxDistance = 100f,
            LayerMask layerMask = default)
        {
            Dictionary<Vector2, Vector3> hitPoints = new Dictionary<Vector2, Vector3>();
            
            // Calculate start position (top-left corner)
            Vector3 startPos = center + new Vector3(
                -hexWidth * gridWidth * 0.5f,
                0,
                -hexHeight * gridHeight * 0.5f
            );

            for (int y = 0; y < gridHeight; y++)
            {
                // Offset for even rows
                float xOffset = (y % 2 == 0) ? hexWidth * 0.5f : 0;
                
                // Y position with hexagon spacing
                float yPos = y * hexHeight * 0.75f;

                for (int x = 0; x < gridWidth; x++)
                {
                    // Calculate ray start position
                    Vector3 rayStart = startPos + new Vector3(
                        x * hexWidth + xOffset,
                        0,
                        -yPos
                    );

                    Ray ray = new Ray(rayStart, Vector3.down);
#if UNITY_EDITOR
                    Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.cyan, 2f);
#endif

                    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
                    {
                        Vector2 gridPos = new Vector2(x, y);
                        hitPoints[gridPos] = hit.point;
                    }
                }
            }

            return hitPoints;
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
    }
} 