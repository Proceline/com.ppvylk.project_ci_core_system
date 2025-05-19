using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using UnityEditorInternal;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    public class HexagonGridScannerWindow : EditorWindow
    {
        private Vector3 centerPosition = Vector3.zero;
        private float hexWidth = 2f;
        private float hexHeight = 2f;
        private int gridWidth = 5;
        private int gridHeight = 5;
        private float maxDistance = 100f;
        private LayerMask layerMask = 0;
        [SerializeField]
        private CellPalette cellPalette;

        [MenuItem("ProjectCI Tools/Debug/Hexagon Grid Scanner")]
        public static void ShowWindow()
        {
            GetWindow<HexagonGridScannerWindow>("Hexagon Grid Scanner");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Hexagon Grid Scanner", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 扫描参数
            EditorGUILayout.LabelField("Scan Parameters", EditorStyles.boldLabel);
            centerPosition = EditorGUILayout.Vector3Field("Center Position", centerPosition);
            hexWidth = EditorGUILayout.FloatField("Hexagon Width", hexWidth);
            hexHeight = EditorGUILayout.FloatField("Hexagon Height", hexHeight);
            gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
            gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
            maxDistance = EditorGUILayout.FloatField("Max Distance", maxDistance);
            string[] layerNames = InternalEditorUtility.layers;
            int maskValue = layerMask.value;
            maskValue = EditorGUILayout.MaskField("Layer Mask", maskValue, layerNames);
            layerMask.value = maskValue;

            EditorGUILayout.Space();

            // 网格生成参数
            EditorGUILayout.LabelField("Grid Generation Parameters", EditorStyles.boldLabel);
            cellPalette = EditorGUILayout.ObjectField("Cell Palette", cellPalette, typeof(CellPalette), true) as CellPalette;

            EditorGUILayout.Space();

            // 按钮
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Scan and Generate Grid"))
            {
                ScanAndGenerateGrid();
            }
            GUI.enabled = true;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Scan button only works in Play Mode", MessageType.Warning);
            }
        }

        private void ScanAndGenerateGrid()
        {
            if (cellPalette == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Cell Palette", "OK");
                return;
            }

            // 使用 GridBattleUtils 生成网格
            var levelGrid = GridBattleUtils.GenerateLevelGridFromGround<HexagonGrid>(
                centerPosition,
                hexWidth,
                hexHeight,
                new Vector2Int(gridWidth, gridHeight),
                layerMask,
                cellPalette
            );

            if (levelGrid != null)
            {
                Debug.Log($"Successfully generated grid with {gridWidth * gridHeight} cells");
            }
        }
    }
} 