using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.MapBuilder
{
    [Serializable]
    public struct MapDecoEntry
    {
        public string label;
        public GameObject prefab;
    }

    [CreateAssetMenu(
        fileName = "MapPalette",
        menuName = "ProjectCI/MapBuilder/Create MapPalette")]
    public class MapPalette : ScriptableObject
    {
        [SerializeField]
        private List<GameObject> groundTiles = new List<GameObject>();

        [SerializeField]
        private List<MapDecoEntry> decorationObjects = new List<MapDecoEntry>();

        public IReadOnlyList<GameObject> GroundTiles => groundTiles;
        public IReadOnlyList<MapDecoEntry> DecorationObjects => decorationObjects;
    }
}
