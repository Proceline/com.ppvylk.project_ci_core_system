using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    using Editor = UnityEditor.Editor;
    
    [CustomEditor(typeof(LevelCellTools))]
    [CanEditMultipleObjects]
    public class LevelCellToolsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LevelCellTools levelCell = (LevelCellTools)target;

            if(!levelCell)
            {
                return;
            }

            CellPalette cellPalette = levelCell.GetLevelCell().GetGrid().GetCellPalette();

            if(cellPalette && cellPalette.m_CellPieces.Length > 0)
            {
                foreach (CellPalettePiece tilePiece in cellPalette.m_CellPieces)
                {
                    if (GUILayout.Button("Set to: " + tilePiece.m_Name))
                    {
                        foreach (GameObject objs in Selection.gameObjects)
                        {
                            ILevelCell objCell = objs.GetComponent<ILevelCell>();
                            if (objCell)
                            {
                                ReplaceTileWith(objCell, GetRandomTile(tilePiece));
                            }
                        }
                    }
                }
            }
        }

        GameObject GetRandomTile(CellPalettePiece InTileListPiece)
        {
            int TotalPieces = InTileListPiece.m_Cells.Length;
            int SelectedIndex = Random.Range(0, TotalPieces);

            return InTileListPiece.m_Cells[SelectedIndex];
        }

        void ReplaceTileWith(ILevelCell InCell, GameObject InTileObj)
        {
            if(InCell)
            {
                InCell.GetGrid().ReplaceTileWith(InCell.GetIndex(), InTileObj);
            }
        }
    }
}
