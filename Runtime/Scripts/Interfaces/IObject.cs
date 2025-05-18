using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI_CoreSystem.Runtime.Scripts.Interfaces
{
    /// <summary>
    /// Base interface for all objects that can be placed on the grid
    /// </summary>
    public interface IObject
    {
        // Events
        UnityEvent OnLeftClick { get; }
        UnityEvent OnRightClick { get; }
        UnityEvent OnMiddleClick { get; }
        UnityEvent OnHoverBegin { get; }
        UnityEvent OnHoverEnd { get; }

        // Properties
        bool IsVisible { get; }
        Vector3 Bounds { get; }
        Vector3 Position { get; }

        // Methods
        void Initialize();
        void PostInitialize();
        void AlignToGrid();
        void SetVisible(bool visible);

        // TODO: Implement these methods after CellInteractionState is defined
        // void HandleInteraction(CellInteractionState interactionState);
        void HandleLeftClick();
        void HandleRightClick();
        void HandleMiddleClick();
        void HandleHoverBegin();
        void HandleHoverEnd();

        // TODO: Implement these methods after ILevelCell and CellState are defined
        // void HandleOwnerCellChanged(ILevelCell newCell);
        // void HandleCellStateChanged(CellState cellState);
    }
} 