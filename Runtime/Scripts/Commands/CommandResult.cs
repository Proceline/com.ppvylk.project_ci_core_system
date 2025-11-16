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
        public string ResultId;
        public string AbilityId;
        public string OwnerId;
        public Vector2Int TargetCellIndex;
        public string ExtraInfo; // Optional, for custom info

        public virtual void ApplyCommand(GridPawnUnit fromUnit, GridPawnUnit toUnit)
        {
            // Empty
        }

        public virtual void ApplyCommand(GridPawnUnit fromUnit, LevelCellBase targetCell)
        {
            // Empty
        }
    }
} 