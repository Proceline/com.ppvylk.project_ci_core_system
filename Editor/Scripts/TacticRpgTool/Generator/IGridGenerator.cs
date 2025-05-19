using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool.Generator
{
    public class IGridGenerator<T> : EditorWindow where T : ILevelGrid
    {
        public CellPalette m_CellPalette;
        public Vector2 m_GridSize;

        protected virtual void OnGUI()
        {
            m_GridSize = EditorGUILayout.Vector2Field("Grid Size:", m_GridSize);

            EditorGUILayout.BeginHorizontal();
            m_CellPalette = EditorGUILayout.ObjectField("Cell Palette:", m_CellPalette, typeof(CellPalette), true) as CellPalette;
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate"))
            {
                Generate();
            }
        }

        void Generate()
        {
            if (m_CellPalette == null)
            {
                Debug.LogError("([TurnBasedTools]::IGridGenerator::GenerateHexagonGrid) Generator is missing a TileList");
                return;
            }

            if (m_CellPalette.m_CellPieces.Length < 0)
            {
                Debug.LogError("([TurnBasedTools]::IGridGenerator::GenerateHexagonGrid) Generator's TileList is missing any tiles.");
                return;
            }

            foreach (CellPalettePiece tileListPiece in m_CellPalette.m_CellPieces)
            {
                foreach (GameObject tile in tileListPiece.m_Cells)
                {
                    if (tile == null)
                    {
                        Debug.LogError(m_CellPalette.name + " has an invalid tile.");
                        return;
                    }
                }
            }

            GameObject gridObject = new GameObject("Level Grid");

            T levelGrid = gridObject.AddComponent<T>();

            GameObject StartingTile = m_CellPalette.m_CellPieces[0].m_Cells[0];

            levelGrid.Setup();
            levelGrid.SetPrefabCursor(StartingTile);
            levelGrid.SetTileList(m_CellPalette);

            Vector3 bounds = StartingTile.GetComponent<Renderer>().bounds.size;

            DrawCells(levelGrid, bounds);

            levelGrid.SetupAllCellAdjacencies();
        }

        protected virtual void DrawCells(T levelGrid, Vector3 CellBounds)
        {
            Debug.Log("([TurnBasedTools]::IGridGenerator) Cannot use ILevelGrid as itself, you must use the HexagonGrid, or SquareGrid");
        }
    }
}
