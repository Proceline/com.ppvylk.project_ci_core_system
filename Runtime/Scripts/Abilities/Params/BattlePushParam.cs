using UnityEngine;
using ProjectCI.CoreSystem.Runtime.Units.Interfaces;
using ProjectCI.CoreSystem.Runtime.Interfaces;

namespace ProjectCI.CoreSystem.Runtime.Abilities.Params
{
    [CreateAssetMenu(fileName = "NewBattlePushParam", menuName = "ProjectCI/Ability/Parameters/Create BattlePushParam", order = 1)]
    public class BattlePushParam : BattleAbilityParam
    {
        public int m_Distance;

        public override void ApplyTo(IUnit InCaster, IObject InTarget)
        {
            // TODO: Implement direct push logic
        }

        public override void ApplyTo(IUnit InCaster, Vector2Int InCell)
        {
            // TODO: Implement cell-based push logic
            // IUnit TargetUnit = GetUnitOnCell(InCell);
            // if (TargetUnit && InCaster)
            // {
            //     Vector2Int PushDirection = GetDirectionToCell(InCaster.GetPosition(), InCell);
            //     Vector2Int targetPos = TargetUnit.GetPosition();
            //     for (int i = 0; i < m_Distance; i++)
            //     {
            //         Vector2Int nextPos = targetPos + PushDirection;
            //         if(IsCellAccessible(nextPos))
            //         {
            //             targetPos = nextPos;
            //         }
            //     }
            //     TargetUnit.MoveTo(targetPos);
            // }
        }

        public override string GetAbilityInfo()
        {
            return "Push Back: " + m_Distance.ToString() + " Space" + ((m_Distance > 1) ? "s" : "");
        }
    }
} 