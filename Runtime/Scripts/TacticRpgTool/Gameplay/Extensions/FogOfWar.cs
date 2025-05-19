using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Extensions
{
    [CreateAssetMenu(fileName = "NewFogOfWar", menuName = "TurnBasedTools/Create FogOfWar", order = 1)]
    public class FogOfWar : ScriptableObject
    {
        [SerializeField]
        int m_DiscoverRange;

        [SerializeField]
        GameObject m_FogObject;

        [SerializeField]
        int m_FogHeight;

        Dictionary<ILevelCell, List<GameObject>> CellToFogObject = new Dictionary<ILevelCell, List<GameObject>>();

        public int NumFogCells()
        {
            return CellToFogObject.Keys.Count;
        }

        public void CheckPoint(ILevelCell InCell)
        {
            AIRadiusInfo radiusInfo = new AIRadiusInfo( InCell, m_DiscoverRange );

            List<ILevelCell> DiscoverCells = AIManager.GetRadius( radiusInfo );
            foreach ( ILevelCell levelCell in DiscoverCells )
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

            GameManager.CheckWinConditions();
        }

        public void SpawnFogObjects()
        {
            if(m_FogObject == null)
            {
                return;
            }

            List<ILevelCell> AllCells = GameManager.GetGrid().GetAllCells();

            foreach (ILevelCell levelCell in AllCells)
            {
                if (levelCell)
                {
                    if ( !levelCell.IsVisible() )
                    {
                        List<GameObject> SpawnedObjs = new List<GameObject>();

                        Vector3 SpawnBounds = GameManager.GetBoundsOfObject(m_FogObject);

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
