using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Data;
using ProjectCI.CoreSystem.Runtime.Enums;

namespace ProjectCI.CoreSystem.Runtime.Levels.Components
{
    /// <summary>
    /// Component for managing level cell visual states
    /// </summary>
    public class BattleLevelCell : BattleCell
    {
        public override void SetMaterial(BattleCellState cellState)
        {
            Renderer renderer = GetRenderer();
            List<IndexToMaterial> cellMatState = GetStyleInfo().GetCellMaterialState(cellState);

            if(cellMatState != null)
            {
                Material[] meshMaterials = renderer.materials;

                foreach (IndexToMaterial matState in cellMatState)
                {
                    if (meshMaterials.Length > matState.Index)
                    {
                        meshMaterials[matState.Index] = matState.Material;
                    }
                }

                renderer.materials = meshMaterials;

                if (meshMaterials.Length == 0)
                {
                    Debug.Log("[ProjectCI] BattleLevelCell::SetMaterial) " + name + " is missing materials in its mesh renderer");
                }
            }
            else
            {
                Debug.Log("[ProjectCI] BattleLevelCell::SetMaterial) " + name + " doesn't have a BattleCellStyleInfo. It needs one to change visual states");
            }
        }

        BattleCellStyleInfo GetStyleInfo()
        {
            BattleCellStyleInfo cellStyleInfo = gameObject.GetComponent<BattleCellStyleInfo>();
            if(cellStyleInfo == null)
            {
                cellStyleInfo = gameObject.AddComponent<BattleCellStyleInfo>();
            }

            return cellStyleInfo;
        }
        
        Material GetMaterial(int materialSlot)
        {
            Material[] materials = GetComponent<MeshRenderer>().materials;

            if (materials.Length >= materialSlot)
            {
                return materials[materialSlot];
            }
            else
            {
                Debug.Log("[ProjectCI] BattleLevelCell::GetMaterial) " + name + " is missing a material in the mesh renderer, or the index(" + materialSlot + ") is set wrong in the GridStyle");
            }

            return null;
        }

        Material[] GetMaterials()
        {
            return GetComponent<MeshRenderer>().materials;
        }
    }
} 