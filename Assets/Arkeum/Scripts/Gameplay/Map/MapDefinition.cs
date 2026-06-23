using System;
using System.Collections.Generic;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Map
{
    [Serializable]
    public sealed class MapDefinition
    {
        public int RunFloor;
        public Vector2Int PlayerSpawn;
        public Vector2Int FloorExitPosition;
        public Vector2Int DungeonEntrancePosition;
        public Vector2Int ShopEntrancePosition;
        public Vector2Int ShopInteriorSpawnPosition;
        public Vector2Int ShopExitPosition;
        public int BossRoomId = -1;
        public List<Vector2Int> BossEntranceBlockCells = new List<Vector2Int>();
        public List<Vector2Int> WalkableCells = new List<Vector2Int>();
        public List<Vector2Int> WallCells = new List<Vector2Int>();
        public List<Vector2Int> ShopCells = new List<Vector2Int>();
        public List<WeaponSpawnDefinition> WeaponSpawns = new List<WeaponSpawnDefinition>();
        public List<ShopOfferDefinition> ShopOffers = new List<ShopOfferDefinition>();
        public List<EnemySpawnDefinition> EnemySpawns = new List<EnemySpawnDefinition>();
        public List<DungeonRoomDefinition> Rooms = new List<DungeonRoomDefinition>();
        public List<DungeonCorridorDefinition> Corridors = new List<DungeonCorridorDefinition>();
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
        public bool IsSpecialRoom;
        public RunSpecialRoomType SpecialRoomType;
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
