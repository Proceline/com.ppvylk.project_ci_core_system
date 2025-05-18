using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Levels.Data;

namespace ProjectCI.CoreSystem.Runtime.Levels.Components
{
    /// <summary>
    /// Component that provides weight and blocking information for a battle object
    /// </summary>
    public class BattleObjectWeightInfo : MonoBehaviour
    {
        /// <summary>
        /// The weight and blocking information for this object
        /// </summary>
        public BattleWeightInfo m_WeightInfo;
    }
} 