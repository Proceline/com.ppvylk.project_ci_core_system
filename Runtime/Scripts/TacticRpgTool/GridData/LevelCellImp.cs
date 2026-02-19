using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData
{
    public class LevelCellImp : LevelCellBase
    {
        public override void SetMaterial(CellState InCellState)
        {
            Renderer objRenderer = GetRenderer();

            List<IndexToMaterial> cellMatState = GetStyleInfo().GetCellMaterialState(InCellState);

            if(cellMatState != null)
            {
                Material[] meshMaterials = objRenderer.materials;

                foreach (IndexToMaterial matState in cellMatState)
                {
                    if (meshMaterials.Length > matState.m_Index)
                    {
                        meshMaterials[matState.m_Index] = matState.m_Material;
                    }
                }

                objRenderer.materials = meshMaterials;

                if (meshMaterials.Length == 0)
                {
                    Debug.Log("([TurnBasedTools]::LevelCell::SetMaterial) " + name + " is missing material's in its mesh renderer");
                }
            }
            else
            {
                Debug.Log("([TurnBasedTools]::LevelCell::SetMaterial) " + name + " doesn't have a CellStyleInfo. It needs one to change visual states");
            }

        }

        private CellStyleInfo GetStyleInfo()
        {
            CellStyleInfo cellStyleInfo = gameObject.GetComponent<CellStyleInfo>();
            if(cellStyleInfo == null)
            {
                cellStyleInfo = gameObject.AddComponent<CellStyleInfo>();
            }

            return cellStyleInfo;
        }
    }
}
