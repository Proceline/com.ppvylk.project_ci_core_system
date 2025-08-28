using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData.LevelGrids;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.GameRules;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay
{
    public partial class TacticBattleManager
    {
        #region EventStuff

        [SerializeField] private UnityEvent<LevelCellBase> onCellPointed;
        [SerializeField] private UnityEvent<LevelCellBase> onCellUnPointed;

        private void HandleInteractionFocused(LevelCellBase InCell, CellInteractionState InInteractionState)
        {
            switch (InInteractionState)
            {
                case CellInteractionState.BeginFocused:
                    onCellPointed?.Invoke(InCell);
                    break;
                case CellInteractionState.EndFocused:
                    onCellUnPointed?.Invoke(InCell);
                    break;
            }
        }

        #endregion
    }
} 