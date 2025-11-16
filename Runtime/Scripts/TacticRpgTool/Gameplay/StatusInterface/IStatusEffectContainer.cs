using System.Collections.Generic;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay.Status
{
    public interface IBattleStatus
    {
        string StatusTag { get; }
        int Duration  { get; }
        int Layer  { get; }
        bool IsBeingDisposed();
    }

    public interface IStatusEffectContainer
    {
        List<IBattleStatus> GetStatusList();
        void AddStatus(IBattleStatus status);
        void RemoveStatusDirectly(IBattleStatus status);
        void RemoveStatusByIndex(int index);
        void MarkDeductStatus(IBattleStatus status);
    }
}
