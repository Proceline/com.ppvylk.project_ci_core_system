using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.PlayerData;
using UnityEditorInternal;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    public class HexagonGridScannerWindow : EditorWindow
    {
        private Vector3 centerPosition = new Vector3(0, 10, 0);
        private float hexWidth = 2f;
        private float hexHeight = 2f;
        private int gridWidth = 5;
        private int gridHeight = 5;
        private float maxDistance = 100f;
        private LayerMask layerMask = 0;
        [SerializeField]
        private CellPalette cellPalette;

        [Header("Battle Manager Settings")]
        [SerializeField]
        private TacticBattleManager battleManagerPrefab;
        [SerializeField]
        private HumanTeamData friendlyTeamData;
        [SerializeField]
        private TeamData hostileTeamData;

        // Test Area fields
        private Vector3 testAreaCenter = Vector3.zero;
        private float testAreaRadius = 5f;
        private bool testAreaIsCircle = true;
        private LayerMask testAreaLayerMask = 0;
        private int testAreaMaxResults = 10;

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

            // Battle Manager 参数
            EditorGUILayout.LabelField("Battle Manager Parameters", EditorStyles.boldLabel);
            battleManagerPrefab = EditorGUILayout.ObjectField("Battle Manager Prefab", battleManagerPrefab, typeof(TacticBattleManager), true) as TacticBattleManager;
            friendlyTeamData = EditorGUILayout.ObjectField("Friendly Team Data", friendlyTeamData, typeof(HumanTeamData), true) as HumanTeamData;
            hostileTeamData = EditorGUILayout.ObjectField("Hostile Team Data", hostileTeamData, typeof(TeamData), true) as TeamData;

            EditorGUILayout.Space();

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

            // Test Area
            EditorGUILayout.LabelField("Test Area", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            testAreaCenter = EditorGUILayout.Vector3Field("Test Center", testAreaCenter);
            testAreaRadius = EditorGUILayout.FloatField("Test Radius", testAreaRadius);
            testAreaIsCircle = EditorGUILayout.Toggle("Is Circle", testAreaIsCircle);
            string[] allLayerNames = InternalEditorUtility.layers;
            int testMaskValue = testAreaLayerMask.value;
            testMaskValue = EditorGUILayout.MaskField("Layer Mask", testMaskValue, allLayerNames);
            testAreaLayerMask.value = testMaskValue;
            testAreaMaxResults = EditorGUILayout.IntField("Max Results", testAreaMaxResults);

            if (GUILayout.Button("Test ScanAreaForObjects<Animator>"))
            {
                TestScanAreaForAnimators();
            }
            EditorGUILayout.EndVertical();
        }

        private void ScanAndGenerateGrid()
        {
            if (cellPalette == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Cell Palette", "OK");
                return;
            }

            if (battleManagerPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Battle Manager Prefab", "OK");
                return;
            }

            if (friendlyTeamData == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign Friendly Team Data", "OK");
                return;
            }

            if (hostileTeamData == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign Hostile Team Data", "OK");
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

                // 创建 Battle Manager
                var battleManager = GridBattleUtils.CreateBattleManager(
                    battleManagerPrefab,
                    levelGrid,
                    friendlyTeamData,
                    hostileTeamData
                );
                battleManager.Initialize();

                if (battleManager != null)
                {
                    Debug.Log("Successfully created Battle Manager");
                }
            }
        }

        private void TestScanAreaForAnimators()
        {
            var animators = GridBattleUtils.ScanAreaForObjects<Animator>(
                testAreaCenter,
                testAreaRadius,
                testAreaIsCircle,
                testAreaLayerMask,
                testAreaMaxResults
            );
            Debug.Log($"Found {animators.Count} Animator(s) in area.");
            foreach (var animator in animators)
            {
                Debug.Log($"Animator: {animator.name} at {animator.transform.position}");
            }
        }
    }
} 