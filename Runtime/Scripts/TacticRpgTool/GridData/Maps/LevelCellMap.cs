using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.General;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.Maps
{
    [System.Serializable]
    public struct LevelCellPair
    {
        [SerializeField]
        public CompassDir _Key;

        [SerializeField]
        public ILevelCell _Value;

        public LevelCellPair(CompassDir InKey, ILevelCell InValue) : this()
        {
            this._Key = InKey;
            this._Value = InValue;
        }
    }

    [System.Serializable]
    public class LevelCellMap : System.Object
    {
        [SerializeField]
        public List<LevelCellPair> Pairs;

        public LevelCellMap()
        {
            Pairs = new List<LevelCellPair>();
        }

        public LevelCellPair Add(CompassDir InKey, ILevelCell InValue)
        {
            foreach (LevelCellPair item in Pairs)
            {
                if (item._Key == InKey)
                {
                    return item;
                }
            }

            Pairs.Add(new LevelCellPair(InKey, InValue));
            return Pairs[Pairs.Count - 1];
        }

        public bool ContainsKey(CompassDir InKey)
        {
            foreach (var item in Pairs)
            {
                if (item._Key == InKey)
                {
                    return true;
                }
            }

            return false;
        }

        public ILevelCell this[CompassDir InKey]
        {
            get
            {
                foreach (var item in Pairs)
                {
                    if (item._Key == InKey)
                    {
                        return item._Value;
                    }
                }

                return null;
            }
        }
    }
}
