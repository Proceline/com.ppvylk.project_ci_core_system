using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem
{
    [System.Serializable]
    public struct AilmentContainedData
    {
        public Ailment m_ailment;
        public int m_NumTurns;
        public List<GameObject> m_SpawnedObjectList;
        public LevelCellBase m_AssociatedCell;
        public GridUnit m_CastedBy;

        public AilmentContainedData(Ailment InAilment, int InNumTurns = 0)
        {
            m_ailment = InAilment;
            m_NumTurns = InNumTurns;
            m_SpawnedObjectList = new List<GameObject>();
            m_AssociatedCell = null;
            m_CastedBy = null;
        }

        public bool IsEqual(AilmentContainedData other)
        {
            return m_ailment == other.m_ailment;
        }

        public bool IsEqual(Ailment other)
        {
            return m_ailment == other;
        }
    }

    public class AilmentContainer : MonoBehaviour
    {
        List<AilmentContainedData> m_Ailments = new List<AilmentContainedData>();

        public List<Ailment> GetAilments()
        {
            List<Ailment> outAilments = new List<Ailment>();

            foreach (AilmentContainedData ailmentData in m_Ailments)
            {
                outAilments.Add(ailmentData.m_ailment);
            }

            return outAilments;
        }

        public List<AilmentContainedData> GetAllAilmentContainerData()
        {
            return m_Ailments;
        }

        public void AddAilment(GridUnit InCaster, CellAilment InAilment, LevelCellBase InCell)
        {
            if (InAilment)
            {
                foreach (AilmentContainedData ailmentData in m_Ailments)
                {
                    if (ailmentData.IsEqual(InAilment))
                    {
                        RemoveAilment(ailmentData);
                        break;
                    }
                }
                AilmentContainedData containedData = new AilmentContainedData(InAilment);
                containedData.m_AssociatedCell = InCell;
                containedData.m_CastedBy = InCaster;

                GameObject spawnObj = InAilment.m_SpawnOnCell;
                if (spawnObj)
                {
                    Vector3 allignPos = InCell.GetAllignPos(spawnObj);
                    containedData.m_SpawnedObjectList.Add( Instantiate(spawnObj, allignPos, spawnObj.gameObject.transform.rotation) );
                }


                m_Ailments.Add(containedData);
            }
        }

        void RemoveAilment(AilmentContainedData InAilmentData)
        {
            if ( m_Ailments.Contains( InAilmentData ) )
            {
                foreach ( GameObject item in InAilmentData.m_SpawnedObjectList )
                {
                    Destroy( item );
                }
                m_Ailments.Remove( InAilmentData );
            }
        }

        public void AddAilment(GridUnit InCaster, Ailment InAilment)
        {
            if (InAilment)
            {
                foreach (AilmentContainedData ailmentData in m_Ailments)
                {
                    if (ailmentData.IsEqual(InAilment))
                    {
                        m_Ailments.Remove(ailmentData);
                        break;
                    }
                }

                AilmentContainedData containedData = new AilmentContainedData(InAilment);
                containedData.m_CastedBy = InCaster;

                m_Ailments.Add(containedData);
            }
        }

        public void CheckAilments()
        {
            List<AilmentContainedData> AilmentstoRemove = new List<AilmentContainedData>();

            foreach (AilmentContainedData ailmentData in m_Ailments)
            {
                if (ailmentData.m_NumTurns >= ailmentData.m_ailment.m_NumEffectedTurns)
                {
                    AilmentstoRemove.Add(ailmentData);
                }
            }

            foreach (AilmentContainedData ailmentData in AilmentstoRemove)
            {
                RemoveAilment( ailmentData );
            }
        }

        public void IncrementAllAilments()
        {
            int numAilments = m_Ailments.Count;
            for (int i = 0; i < numAilments; i++)
            {
                AilmentContainedData ailmentData = m_Ailments[i];
                InternalIncrementAilement(ref ailmentData);
                m_Ailments[i] = ailmentData;
            }
        }

        public void IncrementAilment(AilmentContainedData InAilmentData)
        {
            int numAilments = m_Ailments.Count;
            for (int i = 0; i < numAilments; i++)
            {
                AilmentContainedData ailmentData = m_Ailments[i];
                if(InAilmentData.IsEqual(ailmentData))
                {
                    InternalIncrementAilement(ref ailmentData);
                    m_Ailments[i] = ailmentData;
                    break;
                }
            }
        }

        void InternalIncrementAilement(ref AilmentContainedData ailmentData)
        {
            ++ailmentData.m_NumTurns;
        }
    }
}
