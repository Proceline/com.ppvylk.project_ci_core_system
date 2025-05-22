using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    public class TargetIndicator : MonoBehaviour
    {
        GridPawnUnit m_AssocaitedUnit = null;
        
        void Update()
        {
            if( m_AssocaitedUnit == null )
            {
                m_AssocaitedUnit = GetComponentInParent<GridPawnUnit>();
                if( m_AssocaitedUnit )
                {
                    bool bIsTarget = TacticBattleManager.GetTeamTargets(m_AssocaitedUnit.GetTeam()).Contains(m_AssocaitedUnit);
                    if(bIsTarget)
                    {
                        transform.localPosition = new Vector3(0, m_AssocaitedUnit.GetBounds().y, 0);
                        GetComponentInChildren<Renderer>().enabled = m_AssocaitedUnit.IsVisible();
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
