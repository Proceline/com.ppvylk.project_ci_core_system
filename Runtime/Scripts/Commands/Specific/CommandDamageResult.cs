using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Components;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Commands
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public class CommandDamageResult : CommandResult
    {
        public int BeforeValue;
        public int AfterValue;

        [NonSerialized]
        private UnitAbilityCore _runtimeAbility;

        public override async Awaitable AnalyzeResult(GridPawnUnit owner, UnitAbilityCore ability,
            LevelCellBase target,
            List<Action<GridPawnUnit, LevelCellBase>> reactions)
        {
            await ability.ApplyResult(owner, target, reactions);
        }

        public override void AddReaction(UnitAbilityCore ability, List<Action<GridPawnUnit, LevelCellBase>> reactions)
        {
            if (reactions == null)
            {
                return;
            }
            _runtimeAbility = ability;
            reactions.Add(ApplyVisualEffects);
        }

        private void ApplyVisualEffects(GridPawnUnit owner, LevelCellBase target)
        {
            if (!_runtimeAbility)
            {
                return;
            }
            
            _runtimeAbility.ApplyVisualEffects(owner, target);
            
            GridObject targetObj = target.GetObjectOnCell();
            if (targetObj)
            {
                BattleHealth healthComp = targetObj.GetComponent<BattleHealth>();
                if (healthComp)
                {
                    healthComp.ReceiveHitDamage();
                    healthComp.SetHealth(AfterValue);
                    healthComp.ReceiveHealthDamage(Value);
                }
            }
        }
    }
} 