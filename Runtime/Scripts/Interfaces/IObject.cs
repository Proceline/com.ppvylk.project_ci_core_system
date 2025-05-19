using UnityEngine;
using UnityEngine.Events;

namespace ProjectCI.CoreSystem.Runtime.Interfaces
{
    /// <summary>
    /// Base interface for all objects that can be placed on the grid
    /// </summary>
    public interface IObject
    {
        // Properties
        bool IsVisible { get; }
        Vector3 Bounds { get; }
        Vector3 Position { get; }

        // Methods
        void Initialize();
        void PostInitialize();
        void SetVisible(bool visible);
    }
} 