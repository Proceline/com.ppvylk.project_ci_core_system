using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

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
                    Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.cyan, 2f);

                    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
                    {
                        Vector2 gridPos = new Vector2(x, y);
                        hitPoints[gridPos] = hit.point;
                    }
                }
            }

            return hitPoints;
        }
    }
} 