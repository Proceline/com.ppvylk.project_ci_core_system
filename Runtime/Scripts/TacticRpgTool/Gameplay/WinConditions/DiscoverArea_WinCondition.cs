using System.Collections.Generic;
using UnityEngine;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Extensions;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.WinConditions
{
    [CreateAssetMenu(fileName = "DiscoverArea", menuName = "ProjectCI Tools/WinCondition/Create DiscoverArea", order = 1)]
    public class DiscoverArea_WinCondition : WinCondition
    {
        protected override bool DidTeamWin(BattleTeam InTeam)
        {
            List<LevelCellBase> allCells = TacticBattleManager.GetGrid().GetAllCells();
            foreach (LevelCellBase cell in allCells)
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
