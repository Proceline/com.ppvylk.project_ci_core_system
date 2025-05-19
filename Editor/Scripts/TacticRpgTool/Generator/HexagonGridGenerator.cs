using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool.Generator
{
    public class HexagonGridGenerator : IGridGenerator<HexagonGrid>
    {
        [MenuItem("ProjectCI Tools/HexagonGrid Generator")]
        static void Init()
        {
            HexagonGridGenerator window = (HexagonGridGenerator)EditorWindow.GetWindow(typeof(HexagonGridGenerator));
            window.Show();
        }

        protected override void OnGUI()
        {
            base.OnGUI();
        }

        protected override void DrawCells(HexagonGrid levelGrid, Vector3 CellBounds)
        {
            for (int y = 0; y < m_GridSize.y; y++)
            {
                float offset = 0;
                if (y % 2 == 0)
                {
                    offset = CellBounds.x * 0.5f;
                }

                float finalY = y * CellBounds.z * 0.75f;

                for (int x = 0; x < m_GridSize.x; x++)
                {
                    float finalX = x * CellBounds.x + offset;

                    levelGrid.GenerateCell(new Vector3(finalX, 0.0f, -finalY), new Vector2(x, y));
                }
            }
        }
    }
}
