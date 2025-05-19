using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit
{
    public enum EffectedUnitType
    {
        All,
        Ground,
        Flying,
    }

    [CreateAssetMenu(fileName = "NewAbility", menuName = "TurnBasedTools/Ability/Create New Ability", order = 1)]
    public class UnitAbility : ScriptableObject
    {
        [SerializeField]
        string m_AbilityName;

        [SerializeField]
        Texture2D m_Icon;

        [SerializeField]
        int m_Radius;

        [SerializeField]
        int m_ActionPointCost;

        [SerializeField]
        bool m_bAllowBlocked;

        [SerializeField]
        GameTeam m_EffectedTeam;

        [SerializeField]
        EffectedUnitType m_EffectedType;

        [SerializeField]
        AbilityShape m_AbilityShape;

        [SerializeField]
        AbilityShape m_EffectShape;

        [SerializeField]
        int m_EffectRadius;

        [SerializeField]
        AbilityParticle[] m_SpawnOnCaster;

        [SerializeField]
        AbilityParticle[] m_SpawnOnTarget;

        [SerializeField]
        AbilityParam[] m_Params;

        [SerializeField]
        Ailment[] m_Ailments;

        void Reset()
        {
            m_bAllowBlocked = false;
        }

        #region Getters

        public string GetAbilityName()
        {
            return m_AbilityName;
        }

        public Texture2D GetIcon()
        {
            return m_Icon;
        }

        public int GetActionPointCost()
        {
            return m_ActionPointCost;
        }

        public bool DoesAllowBlocked()
        {
            return m_bAllowBlocked;
        }

        public GameTeam GetEffectedTeam()
        {
            return m_EffectedTeam;
        }

        public EffectedUnitType GetEffectedUnitType()
        {
            return m_EffectedType;
        }


        public AbilityShape GetShape()
        {
            return m_AbilityShape;
        }

        public int GetRadius()
        {
            return m_Radius;
        }

        public AbilityShape GetEffectShape()
        {
            return m_EffectShape;
        }

        public int GetEffectRadius()
        {
            return m_EffectRadius;
        }

        public List<AbilityParticle> GetCasterParticles()
        {
            return new List<AbilityParticle>(m_SpawnOnCaster);
        }

        public List<AbilityParticle> GetTargetParticles()
        {
            return new List<AbilityParticle>(m_SpawnOnTarget);
        }

        public List<AbilityParam> GetParameters()
        {
            return new List<AbilityParam>(m_Params);
        }

        public List<Ailment> GetAilments()
        {
            return new List<Ailment>(m_Ailments);
        }

        #endregion

        public List<ILevelCell> Setup(GridUnit InCasterUnit)
        {
            if (!GetShape())
            {
                return new List<ILevelCell>();
            }

            List<ILevelCell> abilityCells = GetAbilityCells(InCasterUnit);

            CellState AbilityState = (GetEffectedTeam() == GameTeam.Hostile) ? (CellState.eNegative) : (CellState.ePositive);

            foreach (ILevelCell cell in abilityCells)
            {
                if (cell)
                {
                    TacticBattleManager.SetCellState(cell, AbilityState);
                }
            }

            return abilityCells;
        }

        public List<ILevelCell> GetEffectedCells(GridUnit InCasterUnit, ILevelCell InTarget)
        {
            List<ILevelCell> EffectCellList = new List<ILevelCell>();

            if (GetEffectShape() != null)
            {
                List<ILevelCell> EffectCells = GetEffectShape().GetCellList(InCasterUnit, InTarget, GetEffectRadius(), DoesAllowBlocked(), GameTeam.All);
                EffectCellList.AddRange(EffectCells);
            }
            else
            {
                bool bEffectsTarget = TacticBattleManager.CanCasterEffectTarget(InCasterUnit.GetCell(), InTarget, GetEffectedTeam(), DoesAllowBlocked());
                if(bEffectsTarget)
                {
                    EffectCellList.Add(InTarget);
                }
            }

            return EffectCellList;
        }

        public List<ILevelCell> GetAbilityCells(GridUnit InUnit)
        {
            if(!InUnit)
            {
                return new List<ILevelCell>();
            }

            List<ILevelCell> AbilityCells = GetShape().GetCellList(InUnit, InUnit.GetCell(), GetRadius(), DoesAllowBlocked(), GetEffectedTeam());

            if(GetEffectedUnitType() != EffectedUnitType.All)
            {
                List<ILevelCell> RemoveList = new List<ILevelCell>();
                foreach (ILevelCell cell in AbilityCells)
                {
                    if(cell)
                    {
                        GridUnit unitOnCell = cell.GetUnitOnCell();
                        if(unitOnCell)
                        {
                            bool bIsFlying = unitOnCell.IsFlying();

                            if(GetEffectedUnitType() == EffectedUnitType.Flying)
                            {
                                if( !bIsFlying )
                                {
                                    RemoveList.Add(cell);
                                }
                            }
                            else if(GetEffectedUnitType() == EffectedUnitType.Ground)
                            {
                                if( bIsFlying )
                                {
                                    RemoveList.Add(cell);
                                }
                            }
                        }
                        else
                        {
                            RemoveList.Add(cell);
                        }
                    }
                }

                foreach (ILevelCell cell in RemoveList)
                {
                    AbilityCells.Remove(cell);
                }
            }

            return AbilityCells;
        }

        public IEnumerator Execute(GridUnit InCasterUnit, ILevelCell InTarget, UnityEvent OnComplete = null)
        {
            if( GetShape() )
            {
                List<ILevelCell> abilityCells = GetAbilityCells(InCasterUnit);
                if (abilityCells.Contains(InTarget))
                {
                    InCasterUnit.LookAtCell(InTarget);

                    List<ILevelCell> EffectCellList = new List<ILevelCell>();
                    EffectCellList.AddRange( GetEffectedCells( InCasterUnit, InTarget ) );

                    if(!EffectCellList.Contains(InTarget))
                    {
                        EffectCellList.Add( InTarget );
                    }

                    TacticBattleManager.AddActionBeingPerformed();

                    InCasterUnit.RemoveAbilityPoints(m_ActionPointCost);

                    UnitAbilityPlayerData SelectedPlayerData = InCasterUnit.GetUnitAbilityPlayerData(this);

                    AnimationClip AbilityAnimation = SelectedPlayerData.AssociatedAnimation;
                    if ( AbilityAnimation )
                    {
                        InCasterUnit.PlayAnimation( AbilityAnimation, true );
                    }

                    AudioClip startAudioClip = SelectedPlayerData.AudioOnStart;
                    if( startAudioClip )
                    {
                        AudioPlayData audioData = new AudioPlayData(startAudioClip);
                        AudioHandler.PlayAudio(audioData, InCasterUnit.gameObject.transform.position);
                    }

                    yield return new WaitForSeconds( SelectedPlayerData.ExecuteAfterTime );

                    AudioClip executeAudioClip = SelectedPlayerData.AudioOnExecute;
                    if ( executeAudioClip )
                    {
                        AudioPlayData audioData = new AudioPlayData(executeAudioClip);
                        AudioHandler.PlayAudio(audioData, InCasterUnit.gameObject.transform.position);
                    }

                    foreach ( ILevelCell EffectCell in EffectCellList )
                    {
                        InternalHandleEffectedCell(InCasterUnit, EffectCell);
                    }

                    if ( AbilityAnimation )
                    {
                        float timeRemaining = ( AbilityAnimation.length - SelectedPlayerData.ExecuteAfterTime );
                        timeRemaining = Mathf.Max( 0, timeRemaining );

                        yield return new WaitForSeconds( timeRemaining );
                    }

                    TacticBattleManager.RemoveActionBeingPerformed();
                }
            }

            if( OnComplete != null )
            {
                OnComplete.Invoke();
            }
        }

        void InternalHandleEffectedCell(GridUnit InCasterUnit, ILevelCell InEffectCell)
        {
            GridObject targetObj = InEffectCell.GetObjectOnCell();
            GridUnit targetExecuteUnit = InEffectCell.GetUnitOnCell();

            if (targetExecuteUnit)
            {
                targetExecuteUnit.LookAtCell(InCasterUnit.GetCell());
            }

            foreach (AbilityParticle abilityParticle in m_SpawnOnCaster)
            {
                Vector3 pos = InCasterUnit.GetCell().GetAllignPos(InCasterUnit);
                AbilityParticle CreatedAbilityParticle = Instantiate(abilityParticle.gameObject, pos, InCasterUnit.transform.rotation).GetComponent<AbilityParticle>();
                CreatedAbilityParticle.Setup(this, InCasterUnit, InEffectCell);
            }

            foreach (AbilityParticle abilityParticle in m_SpawnOnTarget)
            {
                Vector3 pos = InEffectCell.gameObject.transform.position;

                if (targetObj)
                {
                    pos = InEffectCell.GetAllignPos(targetObj);
                }

                AbilityParticle CreatedAbilityParticle = Instantiate(abilityParticle.gameObject, pos, InEffectCell.transform.rotation).GetComponent<AbilityParticle>();
                CreatedAbilityParticle.Setup(this, InCasterUnit, InEffectCell);
            }

            foreach (AbilityParam param in m_Params)
            {
                if (targetObj)
                {
                    param.ApplyTo(InCasterUnit, targetObj);
                }

                param.ApplyTo(InCasterUnit, InEffectCell);
            }

            foreach (Ailment ailment in m_Ailments)
            {
                if (ailment)
                {
                    if (targetExecuteUnit)
                    {
                        targetExecuteUnit.GetAilmentContainer().AddAilment(InCasterUnit, ailment);
                    }

                    CellAilment cellAilment = ailment as CellAilment;
                    if (cellAilment)
                    {
                        InEffectCell.GetAilmentContainer().AddAilment(InCasterUnit, cellAilment, InEffectCell);
                    }
                }
            }
        }

        public float CalculateAbilityTime(GridUnit InUnit)
        {
            UnitAbilityPlayerData SelectedPlayerData = InUnit.GetUnitAbilityPlayerData(this);
            return SelectedPlayerData.AssociatedAnimation ? SelectedPlayerData.AssociatedAnimation.length : 0;
        }
    }
}
