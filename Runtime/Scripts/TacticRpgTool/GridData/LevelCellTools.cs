using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData
{
    [RequireComponent(typeof(LevelCellBase))]
    public class LevelCellTools : MonoBehaviour
    {
        [SerializeField] private List<LevelCellBase> m_Cells;

        public LevelCellBase GetLevelCell()
        {
            return gameObject.GetComponent<LevelCellBase>();
        }

        public void AddCell(LevelCellBase InCell)
        {
            if(InCell != null)
            {
                m_Cells.Add(InCell);
            }
        }

        public void RemoveCell(LevelCellBase InCell)
        {
            if(InCell != null)
            {
                m_Cells.Remove(InCell);
            }
        }

        public List<LevelCellBase> GetCells()
        {
            return m_Cells;
        }
    }
}
