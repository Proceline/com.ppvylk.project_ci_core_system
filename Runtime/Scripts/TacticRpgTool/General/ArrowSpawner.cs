using System.Collections;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.GridData;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    [ExecuteInEditMode]
    public class ArrowSpawner : MonoBehaviour
    {
        public CompassDir direction;

        DirectionalCellSpawner TileSpawner;

        void Start()
        {
            TileSpawner = GetComponentInParent<DirectionalCellSpawner>();
        }

        void Update()
        {

        }

        public LevelCellBase OnRightClick()
        {
            return TileSpawner.SpawnTile(direction, false);
        }

        public LevelCellBase OnLeftClick()
        {
            return TileSpawner.SpawnTile(direction, true);
        }
    }
}
