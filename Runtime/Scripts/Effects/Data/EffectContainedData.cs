using UnityEngine;
using System.Collections.Generic;
using ProjectCI_CoreSystem.Runtime.Scripts.Effects.ScriptableObjects;
using ProjectCI_CoreSystem.Runtime.Scripts.Units.Interfaces;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Effects.Data
{
    /// <summary>
    /// Data for an effect that is contained by a unit
    /// </summary>
    public struct EffectContainedData
    {
        public BattleEffectSO m_Ailment;
        public int m_NumTurns;
        public List<GameObject> m_SpawnedObjectList;

        // TODO: Add associated cell
        // public ILevelCell m_AssociatedCell;
        public IUnit m_CastedBy;

        public EffectContainedData(BattleEffectSO InAilment, int InNumTurns = 0)
        {
            m_Ailment = InAilment;
            m_NumTurns = InNumTurns;
            m_SpawnedObjectList = new List<GameObject>();
            // m_AssociatedCell = null;
            m_CastedBy = null;
        }

        public bool IsEqual(EffectContainedData other)
        {
            return m_Ailment == other.m_Ailment;
        }

        public bool IsEqual(BattleEffectSO other)
        {
            return m_Ailment == other;
        }
    }
} 