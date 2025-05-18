using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.Levels;

namespace ProjectCI.CoreSystem.Runtime.Levels.Pathfinding
{
    /// <summary>
    /// Contains information needed for pathfinding calculations
    /// </summary>
    public struct BattlePathInfo
    {
        public BattleCell StartCell;
        public BattleCell TargetCell;
        public bool bIgnoreUnits;
        public bool bAllowBlocked;
        public bool bTakeWeightIntoAccount;
        public List<BattleCell> AllowedCells;

        public BattlePathInfo(BattleCell InStart, BattleCell InTarget)
        {
            StartCell = InStart;
            TargetCell = InTarget;
            bIgnoreUnits = true;
            AllowedCells = null;
            bAllowBlocked = false;
            bTakeWeightIntoAccount = false;
        }
    }
} 