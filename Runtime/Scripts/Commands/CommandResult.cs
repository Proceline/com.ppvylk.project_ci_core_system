using System;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Commands
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public abstract class CommandResult
    {
        public const string TakeDamage = "TakeDamage";
        public const string Heal = "Heal";
        public const string TakeDefense = "TakeDefense";
        public const string Dodge = "Dodge";
        public const string TakeCritical = "TakeCritical";

        public string ResultId;
        public string AbilityId;
        public string OwnerId;
        public Vector2 TargetCellIndex;
        public string CommandType; // e.g. "Damage", "Heal"
        public int Value; // e.g. damage or heal amount
        public string ExtraInfo; // Optional, for custom info

        public abstract Awaitable AnalyzeResult(GridPawnUnit owner, UnitAbilityCore ability,
            LevelCellBase target,
            List<Action<GridPawnUnit, LevelCellBase>> reactions);
        public abstract void AddReaction(UnitAbilityCore ability, List<Action<GridPawnUnit, LevelCellBase>> reactions);
    }
} 