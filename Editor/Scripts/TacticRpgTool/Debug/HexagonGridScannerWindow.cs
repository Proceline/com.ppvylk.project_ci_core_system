using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;
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
        private LayerMask layerMask = 0; // Default to layer 0 (Default)

        [MenuItem("ProjectCI Tools/Debug/Hexagon Grid Scanner")]
        public static void ShowWindow()
        {
            GetWindow<HexagonGridScannerWindow>("Hexagon Grid Scanner");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Hexagon Grid Scanner", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Parameters can be edited in both editor and play mode
            centerPosition = EditorGUILayout.Vector3Field("Center Position", centerPosition);
            hexWidth = EditorGUILayout.FloatField("Hexagon Width", hexWidth);
            hexHeight = EditorGUILayout.FloatField("Hexagon Height", hexHeight);
            gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
            gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
            maxDistance = EditorGUILayout.FloatField("Max Distance", maxDistance);
            // 多选Layer（Flags风格）
            string[] layerNames = InternalEditorUtility.layers;
            int maskValue = layerMask.value;
            maskValue = EditorGUILayout.MaskField("Layer Mask", maskValue, layerNames);
            layerMask.value = maskValue;

            EditorGUILayout.Space();

            // Only disable the scan button in non-play mode
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Scan Ground"))
            {
                ScanGround();
            }
            GUI.enabled = true;

            // Show warning if not in play mode
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Scan button only works in Play Mode", MessageType.Warning);
            }
        }

        private void ScanGround()
        {
            Dictionary<Vector2, Vector3> hitPoints = GridBattleUtils.ScanHexagonGroundGrid(
                centerPosition,
                hexWidth,
                hexHeight,
                gridWidth,
                gridHeight,
                maxDistance,
                layerMask
            );

            Debug.Log($"Found {hitPoints.Count} hit points");
            foreach (var hit in hitPoints)
            {
                Debug.Log($"Grid Position: {hit.Key}, World Position: {hit.Value}");
            }
        }
    }
} 