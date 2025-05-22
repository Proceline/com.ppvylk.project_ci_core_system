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
    public enum EffectedVictimType
    {
        All,
        Ground,
        Flying,
    }

    [CreateAssetMenu(fileName = "NewAbility", menuName = "ProjectCI Tools/Ability/Create New Ability", order = 1)]
    public class BasicUnitAbility : ScriptableObject
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
        BattleTeam m_EffectedTeam;

        [SerializeField]
        EffectedVictimType m_EffectedType;

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

        [SerializeField]
        private UnitAbilityAnimation m_Animation;

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

        public BattleTeam GetEffectedTeam()
        {
            return m_EffectedTeam;
        }

        public EffectedVictimType GetEffectedUnitType()
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

        public List<LevelCellBase> Setup(GridPawnUnit InCasterUnit)
        {
            if (!GetShape())
            {
                return new List<LevelCellBase>();
            }

            List<LevelCellBase> abilityCells = GetAbilityCells(InCasterUnit);

            CellState AbilityState = (GetEffectedTeam() == BattleTeam.Hostile) ? (CellState.eNegative) : (CellState.ePositive);

            foreach (LevelCellBase cell in abilityCells)
            {
                if (cell)
                {
                    TacticBattleManager.SetCellState(cell, AbilityState);
                }
            }

            return abilityCells;
        }

        public List<LevelCellBase> GetEffectedCells(GridPawnUnit InCasterUnit, LevelCellBase InTarget)
        {
            List<LevelCellBase> EffectCellList = new List<LevelCellBase>();

            if (GetEffectShape() != null)
            {
                List<LevelCellBase> EffectCells = GetEffectShape().GetCellList(InCasterUnit, InTarget, GetEffectRadius(), DoesAllowBlocked(), BattleTeam.All);
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

        public List<LevelCellBase> GetAbilityCells(GridPawnUnit InUnit)
        {
            if(!InUnit)
            {
                return new List<LevelCellBase>();
            }

            List<LevelCellBase> AbilityCells = GetShape().GetCellList(InUnit, InUnit.GetCell(), GetRadius(), DoesAllowBlocked(), GetEffectedTeam());

            if(GetEffectedUnitType() != EffectedVictimType.All)
            {
                List<LevelCellBase> RemoveList = new List<LevelCellBase>();
                foreach (LevelCellBase cell in AbilityCells)
                {
                    if(cell)
                    {
                        GridPawnUnit unitOnCell = cell.GetUnitOnCell();
                        if(unitOnCell)
                        {
                            bool bIsFlying = unitOnCell.IsFlying();

                            if(GetEffectedUnitType() == EffectedVictimType.Flying)
                            {
                                if( !bIsFlying )
                                {
                                    RemoveList.Add(cell);
                                }
                            }
                            else if(GetEffectedUnitType() == EffectedVictimType.Ground)
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

                foreach (LevelCellBase cell in RemoveList)
                {
                    AbilityCells.Remove(cell);
                }
            }

            return AbilityCells;
        }

        public virtual IEnumerator Execute(GridPawnUnit InCasterUnit, LevelCellBase InTarget, UnityEvent OnComplete = null)
        {
            if( GetShape() )
            {
                List<LevelCellBase> abilityCells = GetAbilityCells(InCasterUnit);
                if (abilityCells.Contains(InTarget))
                {
                    InCasterUnit.LookAtCell(InTarget);

                    List<LevelCellBase> EffectCellList = new List<LevelCellBase>();
                    EffectCellList.AddRange( GetEffectedCells( InCasterUnit, InTarget ) );

                    if(!EffectCellList.Contains(InTarget))
                    {
                        EffectCellList.Add( InTarget );
                    }

                    TacticBattleManager.AddActionBeingPerformed();

                    InCasterUnit.RemoveAbilityPoints(m_ActionPointCost);

                    UnitAbilityPlayerData SelectedPlayerData = InCasterUnit.GetUnitAbilityPlayerData(this);

                    m_Animation?.PlayAnimation(InCasterUnit);

                    AudioClip startAudioClip = SelectedPlayerData.AudioOnStart;
                    if( startAudioClip )
                    {
                        AudioPlayData audioData = new AudioPlayData(startAudioClip);
                        AudioHandler.PlayAudio(audioData, InCasterUnit.gameObject.transform.position);
                    }

                    float firstExecuteTime = m_Animation.ExecuteAfterTime(0);
                    yield return Awaitable.WaitForSecondsAsync(firstExecuteTime);

                    AudioClip executeAudioClip = SelectedPlayerData.AudioOnExecute;
                    if ( executeAudioClip )
                    {
                        AudioPlayData audioData = new AudioPlayData(executeAudioClip);
                        AudioHandler.PlayAudio(audioData, InCasterUnit.gameObject.transform.position);
                    }

                    foreach ( LevelCellBase EffectCell in EffectCellList )
                    {
                        InternalHandleEffectedCell(InCasterUnit, EffectCell);
                    }

                    if (m_Animation)
                    {
                        float timeRemaining = m_Animation.GetAnimationLength() - firstExecuteTime;
                        timeRemaining = Mathf.Max(0, timeRemaining);

                        yield return Awaitable.WaitForSecondsAsync(timeRemaining);
                    }

                    TacticBattleManager.RemoveActionBeingPerformed();
                }
            }

            OnComplete?.Invoke();
        }

        void InternalHandleEffectedCell(GridPawnUnit InCasterUnit, LevelCellBase InEffectCell)
        {
            GridObject targetObj = InEffectCell.GetObjectOnCell();
            GridPawnUnit targetExecuteUnit = InEffectCell.GetUnitOnCell();

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

        public float CalculateAbilityTime(GridPawnUnit InUnit)
        {
            UnitAbilityPlayerData SelectedPlayerData = InUnit.GetUnitAbilityPlayerData(this);
            return SelectedPlayerData.AssociatedAnimation ? SelectedPlayerData.AssociatedAnimation.length : 0;
        }
    }
}
