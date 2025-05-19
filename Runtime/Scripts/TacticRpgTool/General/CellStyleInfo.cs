using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    [System.Serializable]
    public struct IndexToMaterial
    {
        public int m_Index;
        public Material m_Material;

        public IndexToMaterial(int InIndex, Material InMaterial)
        {
            m_Index = InIndex;
            m_Material = InMaterial;
        }
    }

    public class CellStyleInfo : MonoBehaviour
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
            Material[] Mats = GetComponent<MeshRenderer>().materials;

            List<IndexToMaterial> OutMatIndexMap = new List<IndexToMaterial>();
            for (int i = 0; i < Mats.Length; i++)
            {
                if (Mats.Length > i)
                {
                    OutMatIndexMap.Add(new IndexToMaterial(i, Mats[i]));
                }
            }
            return OutMatIndexMap;
        }

        public List<IndexToMaterial> GetCellMaterialState(CellState InCellState)
        {
            switch (InCellState)
            {
                case CellState.eNormal:
                    return m_NormalMatStates;
                case CellState.eHover:
                    return m_HoverMatStates;
                case CellState.ePositive:
                    return m_PositiveMatStates;
                case CellState.eNegative:
                    return m_NegativeMatStates;
                case CellState.eMovement:
                    return m_MovementMatStates;
            }

            return m_NormalMatStates;
        }
    }
}
