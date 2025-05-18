using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Data;
using ProjectCI.CoreSystem.Runtime.Enums;

namespace ProjectCI.CoreSystem.Runtime.Levels.Components
{
    /// <summary>
    /// Component to manage cell material states
    /// </summary>
    public class BattleCellStyleInfo : MonoBehaviour
    {
        [SerializeField]
        List<IndexToMaterial> m_HoverMatStates;

        [SerializeField]
        List<IndexToMaterial> m_PositiveMatStates;

        [SerializeField]
        List<IndexToMaterial> m_NegativeMatStates;

        [SerializeField]
        List<IndexToMaterial> m_MovementMatStates;

        List<IndexToMaterial> m_NormalMatStates = new List<IndexToMaterial>();

        void Start()
        {
            m_NormalMatStates = GenerateCurrentCellMatState();
        }
        
        public List<IndexToMaterial> GenerateCurrentCellMatState()
        {
            Material[] materials = GetComponent<MeshRenderer>().materials;
            List<IndexToMaterial> outMatIndexMap = new List<IndexToMaterial>();
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials.Length > i)
                {
                    outMatIndexMap.Add(new IndexToMaterial(i, materials[i]));
                }
            }
            return outMatIndexMap;
        }

        public List<IndexToMaterial> GetCellMaterialState(BattleCellState cellState)
        {
            switch (cellState)
            {
                case BattleCellState.Normal:
                    return m_NormalMatStates;
                case BattleCellState.Hover:
                    return m_HoverMatStates;
                case BattleCellState.Positive:
                    return m_PositiveMatStates;
                case BattleCellState.Negative:
                    return m_NegativeMatStates;
                case BattleCellState.Movement:
                    return m_MovementMatStates;
                default:
                    return m_NormalMatStates;
            }
        }
    }
} 