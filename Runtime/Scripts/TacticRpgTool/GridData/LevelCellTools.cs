using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData
{
    [RequireComponent(typeof(ILevelCell))]
    public class LevelCellTools : MonoBehaviour
    {
        [SerializeField] private List<ILevelCell> m_Cells;

        public ILevelCell GetLevelCell()
        {
            return gameObject.GetComponent<ILevelCell>();
        }

        public void AddCell(ILevelCell InCell)
        {
            if(InCell != null)
            {
                m_Cells.Add(InCell);
            }
        }

        public void RemoveCell(ILevelCell InCell)
        {
            if(InCell != null)
            {
                m_Cells.Remove(InCell);
            }
        }

        public List<ILevelCell> GetCells()
        {
            return m_Cells;
        }
    }
}
