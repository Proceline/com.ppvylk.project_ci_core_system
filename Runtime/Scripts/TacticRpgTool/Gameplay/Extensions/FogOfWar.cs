using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Extensions
{
    [CreateAssetMenu(fileName = "NewFogOfWar", menuName = "ProjectCI Tools/Create FogOfWar", order = 1)]
    public class FogOfWar : ScriptableObject
    {
        [SerializeField]
        int m_DiscoverRange;

        [SerializeField]
        GameObject m_FogObject;

        [SerializeField]
        int m_FogHeight;

        Dictionary<LevelCellBase, List<GameObject>> CellToFogObject = new Dictionary<LevelCellBase, List<GameObject>>();

        public int NumFogCells()
        {
            return CellToFogObject.Keys.Count;
        }

        public void CheckPoint(LevelCellBase InCell)
        {
            AIRadiusInfo radiusInfo = new AIRadiusInfo( InCell, m_DiscoverRange );

            List<LevelCellBase> DiscoverCells = AStarAlgorithmUtils.GetRadius( radiusInfo );
            foreach ( LevelCellBase levelCell in DiscoverCells )
            {
                if( levelCell )
                {
                    if( !levelCell.IsVisible() )
                    {
                        levelCell.SetVisible( true );
                        if( CellToFogObject.ContainsKey( levelCell ) )
                        {
                            foreach ( GameObject obj in CellToFogObject[ levelCell ] )
                            {
                                Destroy( obj );
                            }
                            CellToFogObject.Remove( levelCell );
                        }
                    }
                }
            }

            TacticBattleManager.CheckWinConditions();
        }

        public void SpawnFogObjects()
        {
            if(m_FogObject == null)
            {
                return;
            }

            List<LevelCellBase> AllCells = TacticBattleManager.GetGrid().GetAllCells();

            foreach (LevelCellBase levelCell in AllCells)
            {
                if (levelCell)
                {
                    if ( !levelCell.IsVisible() )
                    {
                        List<GameObject> SpawnedObjs = new List<GameObject>();

                        Vector3 SpawnBounds = TacticBattleManager.GetBoundsOfObject(m_FogObject);

                        for (int i = 0; i < m_FogHeight; i++)
                        {
                            float extraHeight = SpawnBounds.y * i;
                            Vector3 location = levelCell.transform.position + new Vector3(0, extraHeight, 0);
                            GameObject SpawnedObj = Instantiate(m_FogObject, location, m_FogObject.transform.rotation);
                            SpawnedObjs.Add(SpawnedObj);
                        }

                        CellToFogObject.Add(levelCell, SpawnedObjs);
                    }
                }
            }
        }
    }
}
