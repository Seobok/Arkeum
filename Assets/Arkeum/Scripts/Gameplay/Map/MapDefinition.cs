using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Map
{
    [Serializable]
    public sealed class MapDefinition
    {
        public int RunFloor;
        public Vector2Int PlayerSpawn;
        public Vector2Int MerchantPosition;
        public Vector2Int ReliquaryPosition;
        public Vector2Int StartAltarPosition;
        public Vector2Int UnlockAltarPosition;
        public Vector2Int UndertakerPosition;
        public List<Vector2Int> WalkableCells = new List<Vector2Int>();
        public List<Vector2Int> TemporaryWeaponSpawns = new List<Vector2Int>();
        public Dictionary<Vector2Int, int> DepthByCell = new Dictionary<Vector2Int, int>();
    }
}
