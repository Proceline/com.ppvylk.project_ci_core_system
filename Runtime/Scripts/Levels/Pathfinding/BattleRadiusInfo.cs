using ProjectCI.CoreSystem.Runtime.Levels;
using ProjectCI.CoreSystem.Runtime.Enums;

namespace ProjectCI.CoreSystem.Runtime.Levels.Pathfinding
{
    /// <summary>
    /// Contains information needed for radius calculations
    /// </summary>
    public struct BattleRadiusInfo
    {
        public BattleCell StartCell;
        public int Radius;
        public BattleTeam EffectedTeam;
        public bool bAllowBlocked;
        public bool bStopAtBlockedCell;

        public BattleRadiusInfo(BattleCell InStart, int InRadius)
        {
            StartCell = InStart;
            Radius = InRadius;
            EffectedTeam = BattleTeam.Neutral;
            bAllowBlocked = true;
            bStopAtBlockedCell = true;
        }
    }
} 