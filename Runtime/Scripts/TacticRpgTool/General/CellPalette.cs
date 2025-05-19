using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    [System.Serializable]
    public struct CellPalettePiece
    {
        [SerializeField]
        public string m_Name;

        [SerializeField]
        public GameObject[] m_Cells;
    }

    [CreateAssetMenu(fileName = "NewCellPalette", menuName = "ProjectCI Tools/Create CellPalette", order = 1)]
    public class CellPalette : ScriptableObject
    {
        [SerializeField]
        public CellPalettePiece[] m_CellPieces;

        [SerializeField] private List<GameObject> m_CellPrefabs;

        public GameObject GetCellPrefab(int InIndex)
        {
            if(InIndex >= 0 && InIndex < m_CellPrefabs.Count)
            {
                return m_CellPrefabs[InIndex];
            }

            return null;
        }

        public int GetCellPrefabCount()
        {
            return m_CellPrefabs.Count;
        }
    }
}
