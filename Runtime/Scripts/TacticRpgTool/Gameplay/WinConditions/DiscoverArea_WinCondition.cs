using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Extensions;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "DiscoverArea", menuName = "TurnBasedTools/WinCondition/Create DiscoverArea", order = 1)]
    public class DiscoverArea_WinCondition : WinCondition
    {
        protected override bool DidTeamWin(GameTeam InTeam)
        {
            List<ILevelCell> allCells = TacticBattleManager.GetGrid().GetAllCells();
            foreach (ILevelCell cell in allCells)
            {
                if (cell)
                {
                    if( !cell.IsVisible() )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override string GetConditionStateString()
        {
            int CurrentFogCells = 0;

            FogOfWar fogOfWar = TacticBattleManager.GetFogOfWar();
            if(fogOfWar)
            {
                CurrentFogCells = TacticBattleManager.GetFogOfWar().NumFogCells();
            }

            int TotalCells = TacticBattleManager.GetGrid().GetAllCells().Count;

            return "(" + (TotalCells - CurrentFogCells) + "/" + TotalCells + ")";
        }
    }
}
