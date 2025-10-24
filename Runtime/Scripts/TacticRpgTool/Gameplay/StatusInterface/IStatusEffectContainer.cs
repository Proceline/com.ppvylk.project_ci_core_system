using System.Collections.Generic;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status
{
    public interface IBattleStatus
    {
        string StatusTag { get; }
        int Duration  { get; }
        int Layer  { get; }
    }

    public interface IStatusEffectContainer
    {
        List<IBattleStatus> GetStatusList();
        void AddStatus(IBattleStatus status);
        void RemoveStatus(IBattleStatus status);
    }
}
