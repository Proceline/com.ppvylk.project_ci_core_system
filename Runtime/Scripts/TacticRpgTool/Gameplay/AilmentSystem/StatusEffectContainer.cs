using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine.Serialization;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem
{
    [System.Serializable]
    public struct StatusEffectContainedData
    {
        [FormerlySerializedAs("m_ailment")] public StatusEffect mStatusEffect;
        public int m_NumTurns;
        public List<GameObject> m_SpawnedObjectList;
        public LevelCellBase m_AssociatedCell;
        public GridPawnUnit m_CastedBy;

        public StatusEffectContainedData(StatusEffect inStatusEffect, int InNumTurns = 0)
        {
            mStatusEffect = inStatusEffect;
            m_NumTurns = InNumTurns;
            m_SpawnedObjectList = new List<GameObject>();
            m_AssociatedCell = null;
            m_CastedBy = null;
        }

        public bool IsEqual(StatusEffectContainedData other)
        {
            return mStatusEffect == other.mStatusEffect;
        }

        public bool IsEqual(StatusEffect other)
        {
            return mStatusEffect == other;
        }
    }

    public class StatusEffectContainer : MonoBehaviour
    {
        private readonly List<StatusEffectContainedData> _statusEffectDataList = new();

        public List<StatusEffect> GetStatusEffectList()
        {
            List<StatusEffect> outStatusEffects = new List<StatusEffect>();

            foreach (StatusEffectContainedData statusData in _statusEffectDataList)
            {
                outStatusEffects.Add(statusData.mStatusEffect);
            }

            return outStatusEffects;
        }

        public List<StatusEffectContainedData> GetAllAilmentContainerData()
        {
            return _statusEffectDataList;
        }

        public void AddStatusEffect(GridPawnUnit InCaster, CellStatusEffect inStatusEffect, LevelCellBase InCell)
        {
            if (inStatusEffect)
            {
                foreach (StatusEffectContainedData ailmentData in _statusEffectDataList)
                {
                    if (ailmentData.IsEqual(inStatusEffect))
                    {
                        RemoveStatusEffect(ailmentData);
                        break;
                    }
                }
                StatusEffectContainedData containedData = new StatusEffectContainedData(inStatusEffect);
                containedData.m_AssociatedCell = InCell;
                containedData.m_CastedBy = InCaster;

                GameObject spawnObj = inStatusEffect.m_SpawnOnCell;
                if (spawnObj)
                {
                    Vector3 allignPos = InCell.GetAllignPos(spawnObj);
                    containedData.m_SpawnedObjectList.Add( Instantiate(spawnObj, allignPos, spawnObj.gameObject.transform.rotation) );
                }


                _statusEffectDataList.Add(containedData);
            }
        }

        private void RemoveStatusEffect(StatusEffectContainedData inStatusEffectData)
        {
            if ( _statusEffectDataList.Contains( inStatusEffectData ) )
            {
                foreach ( GameObject item in inStatusEffectData.m_SpawnedObjectList )
                {
                    Destroy( item );
                }
                _statusEffectDataList.Remove( inStatusEffectData );
            }
        }

        public void AddStatusEffect(GridPawnUnit InCaster, StatusEffect inStatusEffect)
        {
            if (inStatusEffect)
            {
                foreach (StatusEffectContainedData ailmentData in _statusEffectDataList)
                {
                    if (ailmentData.IsEqual(inStatusEffect))
                    {
                        _statusEffectDataList.Remove(ailmentData);
                        break;
                    }
                }

                StatusEffectContainedData containedData = new StatusEffectContainedData(inStatusEffect);
                containedData.m_CastedBy = InCaster;

                _statusEffectDataList.Add(containedData);
            }
        }

        public void CheckStatusEffects()
        {
            List<StatusEffectContainedData> effectsToRemove = new List<StatusEffectContainedData>();

            foreach (StatusEffectContainedData effectContainedData in _statusEffectDataList)
            {
                if (effectContainedData.m_NumTurns >= effectContainedData.mStatusEffect.numEffectedTurns)
                {
                    effectsToRemove.Add(effectContainedData);
                }
            }

            foreach (StatusEffectContainedData effectContainedData in effectsToRemove)
            {
                RemoveStatusEffect(effectContainedData);
            }
        }

        public void IncrementAllStatusEffects()
        {
            int numAilments = _statusEffectDataList.Count;
            for (int i = 0; i < numAilments; i++)
            {
                StatusEffectContainedData statusEffectData = _statusEffectDataList[i];
                InternalIncrementStatusEffect(ref statusEffectData);
                _statusEffectDataList[i] = statusEffectData;
            }
        }

        public void IncrementStatusEffect(StatusEffectContainedData inStatusEffectData)
        {
            int numAilments = _statusEffectDataList.Count;
            for (int i = 0; i < numAilments; i++)
            {
                StatusEffectContainedData statusEffectData = _statusEffectDataList[i];
                if(inStatusEffectData.IsEqual(statusEffectData))
                {
                    InternalIncrementStatusEffect(ref statusEffectData);
                    _statusEffectDataList[i] = statusEffectData;
                    break;
                }
            }
        }

        private void InternalIncrementStatusEffect(ref StatusEffectContainedData statusEffectData)
            => ++statusEffectData.m_NumTurns;
    }
}
