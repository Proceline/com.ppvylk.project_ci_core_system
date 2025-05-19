using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem
{
    [CreateAssetMenu(fileName = "NewCellAilment", menuName = "TurnBasedTools/Create New Cell Ailment", order = 1)]
    public class CellAilment : Ailment
    {
        public AilmentExecutionInfo m_ExecuteOnUnitOver;

        public WeightInfo m_WeightInfo;

        public GameObject m_SpawnOnCell;
    }
}
