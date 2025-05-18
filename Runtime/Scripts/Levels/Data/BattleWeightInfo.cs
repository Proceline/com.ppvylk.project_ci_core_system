using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Levels.Data
{
    /// <summary>
    /// Represents weight and blocking information for a battle entity
    /// </summary>
    [System.Serializable]
    public struct BattleWeightInfo
    {
        /// <summary>
        /// Whether the entity blocks movement
        /// </summary>
        public bool bBlocked;

        /// <summary>
        /// The weight value of the entity
        /// </summary>
        public int Weight;

        /// <summary>
        /// Combines two weight infos by adding their weights and using OR for blocked state
        /// </summary>
        public static BattleWeightInfo operator+ (BattleWeightInfo InWeightLeft, BattleWeightInfo InWeightRight)
        {
            BattleWeightInfo NewWeightInfo = InWeightLeft;

            NewWeightInfo.Weight = InWeightLeft.Weight + InWeightRight.Weight;
            NewWeightInfo.bBlocked = (InWeightLeft.bBlocked || InWeightRight.bBlocked);

            return NewWeightInfo;
        }
    }
} 