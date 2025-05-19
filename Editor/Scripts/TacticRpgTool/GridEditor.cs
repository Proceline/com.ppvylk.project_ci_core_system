using UnityEngine;
using UnityEditor;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;

namespace ProjectCI.CoreSystem.Editor.TacticRpgTool
{
    public class GridEditorWindow : EditorWindow
    {
        public static void DeleteEditorObject(GameObject InObj)
        {
            if ( InObj )
            {
                DestroyImmediate( InObj );
            }
        }

        public static GameObject InstantiateEditorObject(GameObject InObj, Vector3 InPosition, Quaternion InRotation)
        {
            return Instantiate( InObj, InPosition, InRotation );
        }
    }

    [InitializeOnLoad]
    public class GridEditor
    {
        static ILevelGrid m_CurrentGrid;

        static DirectionalCellSpawner m_CurrentTileSpawner;

        static GridEditor()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayStateChanged;
            SceneView.duringSceneGui += OnDrawScene;
        }

        static void OnPlayStateChanged(PlayModeStateChange InNewChange)
        {
            if( InNewChange == PlayModeStateChange.ExitingEditMode )
            {
                RemoveTileSpawner();
            }
            else if( InNewChange == PlayModeStateChange.EnteredEditMode )
            {
                if ( Selection.objects.Length == 1 )
                {
                    GameObject selectedObj = Selection.objects[0] as GameObject;
                    if(selectedObj)
                    {
                        ILevelCell selectedCell = selectedObj.GetComponent<ILevelCell>();
                        if (selectedCell)
                        {
                            InstantiateTileSpawner(selectedCell);
                        }
                    }
                }
            }
        }

        static void OnDrawScene(SceneView scene)
        {
            if(UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }

            bool bMouseEvent = ( Event.current.type == EventType.MouseDown );
            if ( bMouseEvent )
            {
                HandleMouseEvent( scene, Event.current );
            }
        }

        static void SetCurrentGrid(ILevelGrid InGrid)
        {
            if (m_CurrentGrid)
            {
                m_CurrentGrid.OnTileReplaced.RemoveListener(OnTileChanged);
            }
            m_CurrentGrid = InGrid;
            if (m_CurrentGrid)
            {
                m_CurrentGrid.OnTileReplaced.AddListener(OnTileChanged);
            }
        }

        static void OnTileChanged(ILevelCell InCell)
        {
            if(InCell)
            {
                Selection.objects = new GameObject[]{ InCell.gameObject };
                InstantiateTileSpawner(InCell);
            }
        }

        #region MouseEventStuff

        static ILevelCell m_RecentlyGeneratedCell;

        static Ray CalculateRay(SceneView InScene, Event InEvent)
        {
            Vector3 scrPos = InEvent.mousePosition;
            scrPos.y = InScene.camera.pixelHeight - scrPos.y;

            return InScene.camera.ScreenPointToRay( scrPos );
        }

        static void InstantiateTileSpawner(ILevelCell InCell)
        {
            DirectionalCellSpawner[] CellSpawners = GameObject.FindObjectsByType<DirectionalCellSpawner>(FindObjectsSortMode.None);
            foreach (DirectionalCellSpawner cellSpawner in CellSpawners)
            {
                GridEditorWindow.DeleteEditorObject(cellSpawner.gameObject);
            }

            if ( InCell )
            {
                GameObject ArrowSpawnerObj = Resources.Load< GameObject >( "ArrowSelector" );
                if ( ArrowSpawnerObj )
                {
                    DirectionalCellSpawner DirTileSpawner = GridEditorWindow.InstantiateEditorObject( ArrowSpawnerObj, InCell.transform.position, ArrowSpawnerObj.transform.rotation ).GetComponent< DirectionalCellSpawner >();
                    DirTileSpawner.SetCurrentTile( InCell );

                    m_CurrentTileSpawner = DirTileSpawner;

                    m_CurrentTileSpawner.OnReferenceCellDestroyed.AddListener(RemoveTileSpawner);
                }
            }
        }

        static void RemoveTileSpawner()
        {
            if ( m_CurrentTileSpawner )
            {
                GridEditorWindow.DeleteEditorObject( m_CurrentTileSpawner.gameObject );
            }
        }

        static void HandleArrowSpawnerClick(GameObject hitObj, Event InEvent)
        {
            bool bLeftClick = InEvent.button == 0;
            bool bRightClick = InEvent.button == 1;

            ArrowSpawner arrowSpawner = hitObj.GetComponent< ArrowSpawner >();
            if ( arrowSpawner )
            {
                if ( bLeftClick )
                {
                    m_RecentlyGeneratedCell = arrowSpawner.OnLeftClick();
                    InstantiateTileSpawner( m_RecentlyGeneratedCell );
                }
                else if ( bRightClick )
                {
                    arrowSpawner.OnRightClick();
                }
            }
        }

        static void UpdateTileSpawnerSelection(GameObject hitObj, Event InEvent)
        {
            bool bLeftClick = InEvent.button == 0;
            if ( bLeftClick )
            {
                RemoveTileSpawner();

                if ( hitObj )
                {
                    ILevelCell levelCell = hitObj.GetComponent<ILevelCell>();
                    if(levelCell)
                    {
                        SetCurrentGrid(levelCell.GetGrid());
                        InstantiateTileSpawner( levelCell );
                    }
                }
                else if(m_RecentlyGeneratedCell)
                {
                    InstantiateTileSpawner(m_RecentlyGeneratedCell);
                }
            }
        }

        static void OnObjectClicked(RaycastHit InHit)
        {
            GameObject hitObj = InHit.collider.gameObject;
            if ( hitObj )
            {
                HandleArrowSpawnerClick( hitObj, Event.current );
                UpdateTileSpawnerSelection( hitObj, Event.current );
            }
        }

        static void HandleMouseEvent(SceneView InScene, Event InEvent)
        {
            bool bLeftClick = InEvent.button == 0;

            RaycastHit hit;
            if ( Physics.Raycast( CalculateRay(InScene, InEvent), out hit ) )
            {
                OnObjectClicked( hit );
            }
            else if ( bLeftClick )
            {
                RemoveTileSpawner();
            }
        }
        
        #endregion

    }
}
