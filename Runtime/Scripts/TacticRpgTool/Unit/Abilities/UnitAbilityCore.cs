using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.AbilityParams;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit.Abilities;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.AilmentSystem;
using System;
using ProjectCI.CoreSystem.Runtime.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit
{
    public enum EffectedVictimType
    {
        All,
        Ground,
        Flying,
    }

    public abstract class UnitAbilityCore : ScriptableObject, IIdentifier
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
        protected List<GameObject> spawnOnCaster = new();

        [SerializeField] 
        protected List<GameObject> spawnOnTarget = new();

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

        public List<GameObject> GetCasterParticles() => spawnOnCaster;

        public List<GameObject> GetTargetParticles() => spawnOnTarget;

        public List<AbilityParamBase> GetParameters()
        {
            return new List<AbilityParamBase>(m_Params);
        }

        public List<StatusEffect> GetAilments()
        {
            return new List<StatusEffect>(m_Ailments);
        }

        #endregion

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

        public List<LevelCellBase> GetAbilityCells(GridPawnUnit inUnit)
        {
            if (!inUnit)
            {
                return new List<LevelCellBase>();
            }

            List<LevelCellBase> abilityCells = GetShape().GetCellList(inUnit, inUnit.GetCell(), GetRadius(),
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

        public abstract void ApplyVisualEffects(GridPawnUnit casterUnit, LevelCellBase effectCell);
    }
}
