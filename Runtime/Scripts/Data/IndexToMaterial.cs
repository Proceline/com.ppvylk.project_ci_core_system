using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Data
{
    /// <summary>
    /// Struct to map an index to a material
    /// </summary>
    [System.Serializable]
    public struct IndexToMaterial
    {
        public int Index;
        public Material Material;

        public IndexToMaterial(int index, Material material)
        {
            Index = index;
            Material = material;
        }
    }
} 