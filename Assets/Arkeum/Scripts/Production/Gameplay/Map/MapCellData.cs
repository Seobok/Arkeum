using System;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Map
{
    [Serializable]
    public sealed class MapCellData
    {
        public Vector2Int Position;
        public bool Walkable = true;
        public int Depth = 1;
    }
}
