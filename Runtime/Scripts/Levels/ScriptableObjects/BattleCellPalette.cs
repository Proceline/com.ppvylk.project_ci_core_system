using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Levels.ScriptableObjects
{
    [System.Serializable]
    public struct BattleCellPalettePiece
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public GameObject[] Cells;
    }

    [CreateAssetMenu(fileName = "NewBattleCellPalette", menuName = "ProjectCI/Battle/Create Cell Palette", order = 1)]
    public class BattleCellPalette : ScriptableObject
    {
        [SerializeField]
        public BattleCellPalettePiece[] CellPieces;
    }
} 