using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.Maps
{
    public struct PositionToLevelCellPair
    {
        public Vector2 Key;
        public readonly LevelCellBase Value;

        public PositionToLevelCellPair(Vector2 InKey, LevelCellBase InValue) : this()
        {
            Key = InKey;
            Value = InValue;
        }
    }

    public class GridCellMap
    {
        public readonly List<PositionToLevelCellPair> Pairs = new();

        public void Add(Vector2 InKey, LevelCellBase InValue)
        {
            foreach (PositionToLevelCellPair item in Pairs)
            {
                if (item.Key == InKey)
                {
                    return;
                }
            }

            Pairs.Add(new PositionToLevelCellPair(InKey, InValue));
        }

        public void Remove(Vector2 InIndex)
        {
            foreach (var item in Pairs)
            {
                if(item.Key == InIndex)
                {
                    Pairs.Remove(item);
                    break;
                }
            }
        }

        public bool ContainsKey(Vector2 InKey)
        {
            foreach (var item in Pairs)
            {
                if (item.Key == InKey)
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            Pairs.Clear();
        }

        public LevelCellBase this[Vector2 InKey]
        {
            get
            {
                foreach (var item in Pairs)
                {
                    if (item.Key == InKey)
                    {
                        return item.Value;
                    }
                }

                return null;
            }
        }
    }
}
