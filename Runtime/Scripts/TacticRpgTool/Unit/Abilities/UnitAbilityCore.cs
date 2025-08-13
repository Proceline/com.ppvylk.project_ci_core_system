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
using System;
using ProjectCI.CoreSystem.Runtime.Interfaces;
using UnityEngine.Serialization;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit
{
    public enum EffectedVictimType
    {
        All,
        Ground,
        Flying,
    }

    [CreateAssetMenu(fileName = "NewAbility", menuName = "ProjectCI Tools/Ability/Create New Ability", order = 1)]
    public class UnitAbilityCore : ScriptableObject, IIdentifier
    {
        public string ID { get; private set; } = string.Empty;

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
        AbilityParamBase[] m_Params;

        [SerializeField]
        StatusEffect[] m_Ailments;

        void Reset()
        {
            m_bAllowBlocked = false;
        }

        #region Getters

        public string GetAbilityName()
        {
            return m_AbilityName;
        }

        public void GenerateNewID()
        {
            ID = Guid.NewGuid().ToString();
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

        public List<AbilityParamBase> GetParameters()
        {
            return new List<AbilityParamBase>(m_Params);
        }

        public List<StatusEffect> GetAilments()
        {
            return new List<StatusEffect>(m_Ailments);
        }

        #endregion

        public List<LevelCellBase> Setup(GridPawnUnit InCasterUnit)
        {
            if (!GetShape())
            {
                return new List<LevelCellBase>();
            }

            List<LevelCellBase> abilityCells = GetAbilityCells(InCasterUnit);

            CellState abilityState = GetEffectedTeam() == BattleTeam.Hostile ? CellState.eNegative : CellState.ePositive;

            foreach (LevelCellBase cell in abilityCells)
            {
                if (cell)
                {
                    TacticBattleManager.SetCellState(cell, abilityState);
                }
            }

            return abilityCells;
        }

        public List<LevelCellBase> GetEffectedCells(GridPawnUnit InCasterUnit, LevelCellBase InTarget)
        {
            List<LevelCellBase> effectCellList = new List<LevelCellBase>();

            if (GetEffectShape() != null)
            {
                List<LevelCellBase> effectCells = GetEffectShape().GetCellList(InCasterUnit, InTarget, GetEffectRadius(), DoesAllowBlocked(), BattleTeam.All);
                effectCellList.AddRange(effectCells);
            }
            else
            {
                bool bEffectsTarget = TacticBattleManager.CanCasterEffectTarget(InCasterUnit.GetCell(), InTarget, GetEffectedTeam(), DoesAllowBlocked());
                if(bEffectsTarget)
                {
                    effectCellList.Add(InTarget);
                }
            }

            return effectCellList;
        }

        public List<LevelCellBase> GetAbilityCells(GridPawnUnit InUnit)
        {
            if (!InUnit)
            {
                return new List<LevelCellBase>();
            }

            List<LevelCellBase> abilityCells = GetShape().GetCellList(InUnit, InUnit.GetCell(), GetRadius(),
                DoesAllowBlocked(), GetEffectedTeam());

            if (GetEffectedUnitType() != EffectedVictimType.All)
            {
                List<LevelCellBase> removeList = new List<LevelCellBase>();
                foreach (LevelCellBase cell in abilityCells)
                {
                    if (!cell)
                    {
                        continue;
                    }

                    GridPawnUnit unitOnCell = cell.GetUnitOnCell();
                    if (unitOnCell)
                    {
                        bool bIsFlying = unitOnCell.IsFlying();

                        if (GetEffectedUnitType() == EffectedVictimType.Flying)
                        {
                            if (!bIsFlying)
                            {
                                removeList.Add(cell);
                            }
                        }
                        else if (GetEffectedUnitType() == EffectedVictimType.Ground)
                        {
                            if (bIsFlying)
                            {
                                removeList.Add(cell);
                            }
                        }
                    }
                    else
                    {
                        removeList.Add(cell);
                    }
                }

                foreach (LevelCellBase cell in removeList)
                {
                    abilityCells.Remove(cell);
                }
            }

            return abilityCells;
        }

        private void InternalHandleEffectedCell(GridPawnUnit InCasterUnit, LevelCellBase InEffectCell)
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
                AbilityParticle createdAbilityParticle = Instantiate(abilityParticle.gameObject, pos, InCasterUnit.transform.rotation).GetComponent<AbilityParticle>();
                createdAbilityParticle.Setup(this, InCasterUnit, InEffectCell);
            }

            foreach (AbilityParticle abilityParticle in m_SpawnOnTarget)
            {
                Vector3 pos = InEffectCell.gameObject.transform.position;

                if (targetObj)
                {
                    pos = InEffectCell.GetAllignPos(targetObj);
                }

                AbilityParticle createdAbilityParticle = Instantiate(abilityParticle.gameObject, pos, InEffectCell.transform.rotation).GetComponent<AbilityParticle>();
                createdAbilityParticle.Setup(this, InCasterUnit, InEffectCell);
            }

            foreach (AbilityParamBase param in m_Params)
            {
                if (targetObj)
                {
                    param.ApplyTo(InCasterUnit, targetObj);
                }

                param.ApplyTo(InCasterUnit, InEffectCell);
            }

            foreach (StatusEffect ailment in m_Ailments)
            {
                if (ailment)
                {
                    if (targetExecuteUnit)
                    {
                        targetExecuteUnit.GetAilmentContainer().AddStatusEffect(InCasterUnit, ailment);
                    }

                    CellStatusEffect cellStatusEffect = ailment as CellStatusEffect;
                    if (cellStatusEffect)
                    {
                        InEffectCell.GetAilmentContainer().AddStatusEffect(InCasterUnit, cellStatusEffect, InEffectCell);
                    }
                }
            }
        }

        public void ApplyVisualEffects(GridPawnUnit InCasterUnit, LevelCellBase InEffectCell)
        {
            GridObject targetObj = InEffectCell.GetObjectOnCell();
            GridPawnUnit targetExecuteUnit = InEffectCell.GetUnitOnCell();

            if (targetExecuteUnit)
            {
                targetExecuteUnit.LookAtCell(InCasterUnit.GetCell());
            }

            // Visual effects on caster
            foreach (AbilityParticle abilityParticle in m_SpawnOnCaster)
            {
                Vector3 pos = InCasterUnit.GetCell().GetAllignPos(InCasterUnit);
                AbilityParticle createdAbilityParticle = Instantiate(abilityParticle.gameObject, pos, InCasterUnit.transform.rotation).GetComponent<AbilityParticle>();
                createdAbilityParticle.Setup(this, InCasterUnit, InEffectCell);
            }

            // Visual effects on target
            foreach (AbilityParticle abilityParticle in m_SpawnOnTarget)
            {
                Vector3 pos = InEffectCell.gameObject.transform.position;

                if (targetObj)
                {
                    pos = InEffectCell.GetAllignPos(targetObj);
                }

                AbilityParticle createdAbilityParticle = Instantiate(abilityParticle.gameObject, pos, InEffectCell.transform.rotation).GetComponent<AbilityParticle>();
                createdAbilityParticle.Setup(this, InCasterUnit, InEffectCell);
            }

            // TODO: Should be handled as visual effects
            foreach (StatusEffect ailment in m_Ailments)
            {
                if (ailment)
                {
                    if (targetExecuteUnit)
                    {
                        targetExecuteUnit.GetAilmentContainer().AddStatusEffect(InCasterUnit, ailment);
                    }

                    CellStatusEffect cellStatusEffect = ailment as CellStatusEffect;
                    if (cellStatusEffect)
                    {
                        InEffectCell.GetAilmentContainer().AddStatusEffect(InCasterUnit, cellStatusEffect, InEffectCell);
                    }
                }
            }
        }
    }
}
