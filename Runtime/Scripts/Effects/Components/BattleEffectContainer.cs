using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Effects.Data;
using ProjectCI.CoreSystem.Runtime.Effects.ScriptableObjects;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Effects.Components
{
    /// <summary>
    /// Manages effects on a battle entity (unit or cell)
    /// </summary>
    public class BattleEffectContainer : MonoBehaviour
    {
        List<EffectContainedData> m_Effects = new List<EffectContainedData>();

        public List<BattleEffectSO> GetEffects()
        {
            List<BattleEffectSO> outEffects = new List<BattleEffectSO>();

            foreach (EffectContainedData effectData in m_Effects)
            {
                outEffects.Add(effectData.m_Ailment);
            }

            return outEffects;
        }

        public List<EffectContainedData> GetAllEffectContainerData()
        {
            return m_Effects;
        }

        public void AddEffect(IUnit InCaster, BattleEffectSO InEffect, Vector2Int InCell)
        {
            if (InEffect)
            {
                foreach (EffectContainedData effectData in m_Effects)
                {
                    if (effectData.IsEqual(InEffect))
                    {
                        RemoveEffect(effectData);
                        break;
                    }
                }
                EffectContainedData containedData = new EffectContainedData(InEffect);
                // TODO: Add associated cell
                // containedData.m_AssociatedCell = InCell;
                containedData.m_CastedBy = InCaster;

                // TODO: Implement spawn object logic
                // GameObject spawnObj = InEffect.m_SpawnOnCell;
                // if (spawnObj)
                // {
                //     Vector3 allignPos = InCell.GetAllignPos(spawnObj);
                //     containedData.m_SpawnedObjectList.Add(Instantiate(spawnObj, allignPos, spawnObj.gameObject.transform.rotation));
                // }

                m_Effects.Add(containedData);
            }
        }

        void RemoveEffect(EffectContainedData InEffectData)
        {
            if (m_Effects.Contains(InEffectData))
            {
                foreach (GameObject item in InEffectData.m_SpawnedObjectList)
                {
                    Destroy(item);
                }
                m_Effects.Remove(InEffectData);
            }
        }

        public void AddEffect(IUnit InCaster, BattleEffectSO InEffect)
        {
            if (InEffect)
            {
                foreach (EffectContainedData effectData in m_Effects)
                {
                    if (effectData.IsEqual(InEffect))
                    {
                        m_Effects.Remove(effectData);
                        break;
                    }
                }

                EffectContainedData containedData = new EffectContainedData(InEffect);
                containedData.m_CastedBy = InCaster;

                m_Effects.Add(containedData);
            }
        }

        public void CheckEffects()
        {
            List<EffectContainedData> EffectsToRemove = new List<EffectContainedData>();

            foreach (EffectContainedData effectData in m_Effects)
            {
                // TODO: Implement turn check logic
                // if (effectData.m_NumTurns >= effectData.m_Ailment.m_NumEffectedTurns)
                // {
                //     EffectsToRemove.Add(effectData);
                // }
            }

            foreach (EffectContainedData effectData in EffectsToRemove)
            {
                RemoveEffect(effectData);
            }
        }

        public void IncrementAllEffects()
        {
            int numEffects = m_Effects.Count;
            for (int i = 0; i < numEffects; i++)
            {
                EffectContainedData effectData = m_Effects[i];
                InternalIncrementEffect(ref effectData);
                m_Effects[i] = effectData;
            }
        }

        public void IncrementEffect(EffectContainedData InEffectData)
        {
            int numEffects = m_Effects.Count;
            for (int i = 0; i < numEffects; i++)
            {
                EffectContainedData effectData = m_Effects[i];
                if (InEffectData.IsEqual(effectData))
                {
                    InternalIncrementEffect(ref effectData);
                    m_Effects[i] = effectData;
                    break;
                }
            }
        }

        void InternalIncrementEffect(ref EffectContainedData effectData)
        {
            ++effectData.m_NumTurns;
        }
    }
} 