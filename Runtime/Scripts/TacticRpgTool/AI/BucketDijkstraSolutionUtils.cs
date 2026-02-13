using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.AI
{
    public sealed class RadiusField
    {
        // 每个格子的最小代价（不可达为 int.MaxValue）
        public readonly Dictionary<LevelCellBase, int> Dist = new();

        // 可达格集合：Dist[cell] <= M
        public readonly List<LevelCellBase> Reach = new();

        // 按总代价分层（下标=总代价）。layers[d] 是所有 Dist==d 的格。
        // 便于“从近到远”遍历（比如先用低代价格生成候选）
        public readonly List<List<LevelCellBase>> Layers = new();

        // 可选：回溯父指针（若需要重构路径）
        public readonly Dictionary<LevelCellBase, LevelCellBase> Parent = new();

        public void AddCellToLayerOnIndex(int layerValue, LevelCellBase cell)
        {
            var cellsList = GetCellsInLayer(layerValue);
            cellsList.Add(cell);
        }

        public List<LevelCellBase> GetCellsInLayer(int layerValue)
        {
            while (layerValue >= Layers.Count)
            {
                Layers.Add(new List<LevelCellBase>());
            }

            return Layers[layerValue];
        }
    }
    
    public sealed class AttackField
    {
        public readonly HashSet<LevelCellBase> AllVictims = new();
        public readonly Dictionary<LevelCellBase, List<LevelCellBase>> VictimsFromCells = new();
        // FromWhich[t] = 可以从哪些站位格打到 t
    }
    
    public sealed class BucketQueue<T>
    {
        private readonly List<T>[] _buckets;  // 每个桶存一组元素
        private readonly int _maxBucketLength;              // 桶数组长度 = Cmax * M + 1
        private int _curr;              // 当前扫描的距离（绝对距离）

        public BucketQueue(int inputL, int startDist = 0)
        {
            _maxBucketLength = Mathf.Max(1, inputL);
            _buckets = new List<T>[_maxBucketLength];
            for (int i = 0; i < _maxBucketLength; i++)
            {
                _buckets[i] = new List<T>(4);
            }
            _curr = Mathf.Max(0, startDist);
        }

        /// <summary>把 item 放入“距离=key”的桶。</summary>
        public void Enqueue(int key, T item)
        {
            // key 必须是非负；调用者需保证 key 不超过关注范围（例如 <= Cmax*M）
            var index = key % _maxBucketLength;
            _buckets[index].Add(item);
        }

        /// <summary>
        /// 尝试从“当前距离”的桶中弹出一个元素；如果当前桶空则推进到下一个非空桶。
        /// max 限制推进上限（例如 M），超过则返回 false。
        /// </summary>
        public bool TryDequeueUpTo(int max, out int currDist, out T item)
        {
            while (_curr <= max)
            {
                var list = _buckets[_curr % _maxBucketLength];
                int last = list.Count - 1;
                if (last >= 0)
                {
                    item = list[last];
                    list.RemoveAt(last);
                    currDist = _curr;
                    return true;
                }

                _curr++;
            }

            currDist = -1;
            item = default;
            return false;
        }

        /// <summary>把扫描指针推进一步（通常不需要手动调用）。</summary>
        public void Advance() => _curr++;
    }
    
    public static class BucketDijkstraSolutionUtils
    {
        public static RadiusField CalculateBucket(AIRadiusInfo radiusInfo, bool useWeight, int maxTotalWeight)
        {
            var startCell = radiusInfo.StartCell;
            if (!startCell)
            {
                throw new NullReferenceException(
                    $"([{nameof(BucketDijkstraSolutionUtils)}] Invalid Start, or Target");
            }
            
            var radius = radiusInfo.Radius;

            var field = new RadiusField();
            var visited = new HashSet<LevelCellBase>();
            BucketQueue<LevelCellBase> bucketQueue = new BucketQueue<LevelCellBase>(radiusInfo.Radius);
            
            field.Dist[startCell] = 0;
            bucketQueue.Enqueue(0, startCell);
            field.AddCellToLayerOnIndex(0, startCell);
            field.Reach.Add(startCell);
            
            while (bucketQueue.TryDequeueUpTo(maxTotalWeight, out int currG, out LevelCellBase cell))
            {
                if (!visited.Add(cell))
                {
                    continue;
                }

                // 标记可达, No need to check duplicated because correct Cell only browse once
                if (currG <= radius && !cell.GetUnitOnCell())
                {
                    field.Reach.Add(cell);
                }

                foreach (var vertex in cell.GetAllAdjacentCells())
                {
                    // 遇阻即停：不把不允许的格扩展进搜索
                    if (radiusInfo.bStopAtBlockedCell && !AStarAlgorithmUtils.AllowCellInRadius(vertex, radiusInfo))
                    {
                        continue;
                    }

                    var weight = useWeight? vertex.GetWeightInfo().weight : 1;
                    if (weight < 0 || weight > maxTotalWeight)
                    {
                        continue;
                    }

                    int newG = currG + weight;
                    if (newG > maxTotalWeight) 
                    {
                        continue;
                    }

                    // If field doesn't contain VERTEX, or newG is less than oldG
                    if (!field.Dist.TryGetValue(vertex, out var oldG) || newG < oldG)
                    {
                        field.Dist[vertex] = newG;
                        field.Parent[vertex] = cell;
                        bucketQueue.Enqueue(newG, vertex);

                        // 分层：记录“总代价为 newG 的格”
                        field.AddCellToLayerOnIndex(newG, vertex);
                    }
                }
            }

            return field;
        }

        public static AttackField ComputeAttackField(RadiusField moveField,
            Func<LevelCellBase, List<LevelCellBase>> startToAoe)
        {
            var field = new AttackField();
            foreach (var reachableCell in moveField.Reach)
            {
                var victims = startToAoe.Invoke(reachableCell);
                foreach (var victim in victims)
                {
                    field.AllVictims.Add(victim);
                    if (!field.VictimsFromCells.TryGetValue(victim, out var fromCells))
                    {
                        fromCells = new List<LevelCellBase>();
                        field.VictimsFromCells[victim] = fromCells;
                    }

                    fromCells.Add(reachableCell);
                }
            }

            return field;
        }
    }
}