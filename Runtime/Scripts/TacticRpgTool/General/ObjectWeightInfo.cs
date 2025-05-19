using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    [System.Serializable]
    public struct WeightInfo
    {
        public bool bBlocked;
        public int Weight;

        public static WeightInfo operator+ (WeightInfo InWeightLeft, WeightInfo InWeightRight)
        {
            WeightInfo NewWeightInfo = InWeightLeft;

            NewWeightInfo.Weight = InWeightLeft.Weight + InWeightRight.Weight;
            NewWeightInfo.bBlocked = (InWeightLeft.bBlocked || InWeightRight.bBlocked);

            return NewWeightInfo;
        }
    }

    public class ObjectWeightInfo : MonoBehaviour
    {
        public WeightInfo m_WeightInfo;
    }
}
