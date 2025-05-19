using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData
{
    public class LevelCell : ILevelCell
    {
        public override void SetMaterial(CellState InCellState)
        {
            Renderer renderer = GetRenderer();

            List<IndexToMaterial> CellMatState = GetStyleInfo().GetCellMaterialState(InCellState);

            if(CellMatState != null)
            {
                Material[] MeshMaterials = renderer.materials;

                foreach (IndexToMaterial MatState in CellMatState)
                {
                    if (MeshMaterials.Length > MatState.m_Index)
                    {
                        MeshMaterials[MatState.m_Index] = MatState.m_Material;
                    }
                }

                renderer.materials = MeshMaterials;

                if (MeshMaterials.Length == 0)
                {
                    Debug.Log("([TurnBasedTools]::LevelCell::SetMaterial) " + name + " is missing material's in its mesh renderer");
                }
            }
            else
            {
                Debug.Log("([TurnBasedTools]::LevelCell::SetMaterial) " + name + " doesn't have a CellStyleInfo. It needs one to change visual states");
            }

        }

        CellStyleInfo GetStyleInfo()
        {
            CellStyleInfo cellStyleInfo = gameObject.GetComponent<CellStyleInfo>();
            if(cellStyleInfo == null)
            {
                cellStyleInfo = gameObject.AddComponent<CellStyleInfo>();
            }

            return cellStyleInfo;
        }
        
        Material GetMaterial(int InMaterialSlot)
        {
            Material[] Mats = GetComponent<MeshRenderer>().materials;

            if (Mats.Length >= InMaterialSlot)
            {
                return Mats[InMaterialSlot];
            }
            else
            {
                Debug.Log("[TurnBasedTools]::LevelCell::GetMaterial) " + name + " is missing a material in the mesh renderer, or the index(" + InMaterialSlot + ") is set wrong in the GridStyle");
            }

            return null;
        }

        Material[] GetMaterials()
        {
            return GetComponent<MeshRenderer>().materials;
        }
    }
}
