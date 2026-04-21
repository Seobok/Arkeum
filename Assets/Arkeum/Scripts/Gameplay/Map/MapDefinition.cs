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
        public List<DungeonRoomDefinition> Rooms = new List<DungeonRoomDefinition>();
        public List<DungeonCorridorDefinition> Corridors = new List<DungeonCorridorDefinition>();
        public Dictionary<Vector2Int, int> DepthByCell = new Dictionary<Vector2Int, int>();
    }

    public enum DoorDirection
    {
        Up,
        Down,
        Left,
        Right,
    }

    [Serializable]
    public sealed class DungeonRoomDefinition
    {
        public int Id;
        public Vector2Int Origin;
        public Vector2Int Min;
        public Vector2Int Max;
        public List<Vector2Int> Cells = new List<Vector2Int>();
        public List<DungeonDoorDefinition> Doors = new List<DungeonDoorDefinition>();
    }

    [Serializable]
    public sealed class DungeonDoorDefinition
    {
        public Vector2Int Position;
        public DoorDirection Direction;
    }

    [Serializable]
    public sealed class DungeonCorridorDefinition
    {
        public int FromRoomId;
        public int ToRoomId;
        public Vector2Int FromDoor;
        public DoorDirection FromDirection;
        public Vector2Int ToDoor;
        public DoorDirection ToDirection;
        public List<Vector2Int> Cells = new List<Vector2Int>();
    }
}
