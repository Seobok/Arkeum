using System;
using System.Collections.Generic;
using Arkeum.Production.Core;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Progression;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Infrastructure.Persistence
{
    [Serializable]
    public sealed class SaveGameData
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public long SavedAtUtcTicks;
        public GameState GameState;
        public SaveProfile Profile = new SaveProfile();
        public RunSaveData Run;
    }

    [Serializable]
    public sealed class RunSaveData
    {
        public int RunIndex;
        public int CurrentFloor;
        public int PlayerTurnCount;
        public bool FloorExitUsed;
        public bool BossRoomEntered;
        public bool BossRoomCleared;
        public bool HasEquippedWeapon;
        public string EquippedWeaponId;
        public bool IsTimingModeEnabled;
        public RunEndReason EndReason;
        public MapSaveData Map;
        public List<ActorSaveData> Actors = new List<ActorSaveData>();
    }

    [Serializable]
    public sealed class MapSaveData
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
        public List<WeaponSpawnSaveData> WeaponSpawns = new List<WeaponSpawnSaveData>();
        public List<ShopOfferSaveData> ShopOffers = new List<ShopOfferSaveData>();
        public List<EnemySpawnSaveData> EnemySpawns = new List<EnemySpawnSaveData>();
        public List<DungeonRoomDefinition> Rooms = new List<DungeonRoomDefinition>();
        public List<DungeonCorridorDefinition> Corridors = new List<DungeonCorridorDefinition>();
    }

    [Serializable]
    public sealed class WeaponSpawnSaveData
    {
        public string WeaponId;
        public Vector2Int Position;
    }

    [Serializable]
    public sealed class ShopOfferSaveData
    {
        public Vector2Int Position;
        public string WeaponId;
        public int Price;
        public string EffectSummary;
    }

    [Serializable]
    public sealed class EnemySpawnSaveData
    {
        public string EnemyId;
        public Vector2Int Position;
    }

    [Serializable]
    public sealed class ActorSaveData
    {
        public string Id;
        public string DisplayName;
        public Vector2Int GridPosition;
        public Vector2Int FacingDirection;
        public ActorStats Stats;
        public string EnemyId;
        public int CurrentHp;
        public bool IsEnemy;
        public int Gold;
        public string TargetActorId;
        public EnemyActionType PendingEnemyAction;
        public int PendingEnemyActionTurns;
        public bool HasPendingEnemyTargetCell;
        public Vector2Int PendingEnemyTargetCell;
        public Vector2Int PendingEnemyFacingDirection;
        public List<Vector2Int> PendingBossAffectedCells = new List<Vector2Int>();
        public List<Vector2Int> ActiveBossWallCells = new List<Vector2Int>();
        public int BossTurnCount;
        public int LastSpaceCutTurn;
        public int BossAlignedTurnCount;
        public int BossStunTurnsRemaining;
        public int BossWallTurnsRemaining;
    }

    public readonly struct SaveSlotMetadata
    {
        public int SlotNumber { get; }
        public bool Exists { get; }
        public DateTime SavedAtUtc { get; }
        public int CurrentFloor { get; }
        public int Gold { get; }

        public SaveSlotMetadata(int slotNumber, bool exists, DateTime savedAtUtc, int currentFloor, int gold)
        {
            SlotNumber = slotNumber;
            Exists = exists;
            SavedAtUtc = savedAtUtc;
            CurrentFloor = currentFloor;
            Gold = gold;
        }
    }
}
