using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    [System.Serializable]
    public struct WeightInfo
    {
        public bool bBlocked;
        
        [FormerlySerializedAs("Weight")] 
        public int weight;

        public static WeightInfo operator+ (WeightInfo weightLeftSide, WeightInfo weightRightSide)
        {
            WeightInfo newWeightInfo = weightLeftSide;

            newWeightInfo.weight = weightLeftSide.weight + weightRightSide.weight;
            newWeightInfo.bBlocked = weightLeftSide.bBlocked || weightRightSide.bBlocked;

            return newWeightInfo;
        }
    }

    public class ObjectWeightInfo : MonoBehaviour
    {
        [FormerlySerializedAs("m_WeightInfo")] 
        public WeightInfo weightInfo;
    }
}
