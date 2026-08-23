using System;
using System.Collections.Generic;
using System.IO;
using Arkeum.Production.Core;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Progression;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Infrastructure.Persistence
{
    public sealed class SaveGameService
    {
        public const int SlotCount = 3;

        private readonly string saveDirectory;

        public SaveGameService(string rootPath = null)
        {
            saveDirectory = string.IsNullOrWhiteSpace(rootPath)
                ? Path.Combine(Application.persistentDataPath, "Saves")
                : rootPath;
        }

        public bool HasSlot(int slotNumber)
        {
            return IsValidSlot(slotNumber) && File.Exists(GetSlotPath(slotNumber));
        }

        public bool TrySave(
            int slotNumber,
            GameState gameState,
            SaveProfile profile,
            RunState runState,
            MapDefinition map,
            IReadOnlyList<ActorEntity> actors,
            out string error)
        {
            error = string.Empty;
            if (!IsValidSlot(slotNumber))
            {
                error = $"Slot number must be between 1 and {SlotCount}.";
                return false;
            }

            if (profile == null)
            {
                error = "There is no active profile to save.";
                return false;
            }

            try
            {
                Directory.CreateDirectory(saveDirectory);
                SaveGameData data = BuildSaveData(gameState, profile, runState, map, actors);
                string json = JsonUtility.ToJson(data, true);
                string path = GetSlotPath(slotNumber);
                string temporaryPath = path + ".tmp";
                File.WriteAllText(temporaryPath, json);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.Move(temporaryPath, path);
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                Debug.LogException(exception);
                return false;
            }
        }

        public bool TryLoad(int slotNumber, out SaveGameData data, out string error)
        {
            data = null;
            error = string.Empty;
            if (!IsValidSlot(slotNumber))
            {
                error = $"Slot number must be between 1 and {SlotCount}.";
                return false;
            }

            string path = GetSlotPath(slotNumber);
            if (!File.Exists(path))
            {
                error = $"Save slot {slotNumber} is empty.";
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<SaveGameData>(File.ReadAllText(path));
                if (data == null || data.Version <= 0 || data.Version > SaveGameData.CurrentVersion)
                {
                    error = "The save file version is not supported.";
                    data = null;
                    return false;
                }

                if (data.Profile == null)
                {
                    data.Profile = new SaveProfile();
                }

                data.Profile.UnlockedFlags ??= new List<string>();
                data.Profile.CompletedQuestIds ??= new List<string>();
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                Debug.LogException(exception);
                return false;
            }
        }

        public SaveSlotMetadata GetSlotMetadata(int slotNumber)
        {
            if (!TryLoad(slotNumber, out SaveGameData data, out _))
            {
                return new SaveSlotMetadata(slotNumber, false, default, 0, 0);
            }

            DateTime savedAt = data.SavedAtUtcTicks > 0
                ? new DateTime(data.SavedAtUtcTicks, DateTimeKind.Utc)
                : default;
            return new SaveSlotMetadata(
                slotNumber,
                true,
                savedAt,
                data.Run != null ? data.Run.CurrentFloor : 0,
                data.Profile != null ? data.Profile.Gold : 0);
        }

        public bool DeleteSlot(int slotNumber)
        {
            if (!IsValidSlot(slotNumber))
            {
                return false;
            }

            string path = GetSlotPath(slotNumber);
            if (!File.Exists(path))
            {
                return true;
            }

            File.Delete(path);
            return true;
        }

        public MapDefinition RestoreMap(MapSaveData snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }

            MapDefinition map = new MapDefinition
            {
                RunFloor = snapshot.RunFloor,
                PlayerSpawn = snapshot.PlayerSpawn,
                FloorExitPosition = snapshot.FloorExitPosition,
                DungeonEntrancePosition = snapshot.DungeonEntrancePosition,
                ShopEntrancePosition = snapshot.ShopEntrancePosition,
                ShopInteriorSpawnPosition = snapshot.ShopInteriorSpawnPosition,
                ShopExitPosition = snapshot.ShopExitPosition,
                BossRoomId = snapshot.BossRoomId,
                BossEntranceBlockCells = Copy(snapshot.BossEntranceBlockCells),
                WalkableCells = Copy(snapshot.WalkableCells),
                WallCells = Copy(snapshot.WallCells),
                ShopCells = Copy(snapshot.ShopCells),
                Rooms = snapshot.Rooms ?? new List<DungeonRoomDefinition>(),
                Corridors = snapshot.Corridors ?? new List<DungeonCorridorDefinition>(),
            };

            if (snapshot.WeaponSpawns != null)
            {
                foreach (WeaponSpawnSaveData spawn in snapshot.WeaponSpawns)
                {
                    WeaponDefinition weapon = FindWeapon(spawn.WeaponId);
                    if (weapon != null)
                    {
                        map.WeaponSpawns.Add(new WeaponSpawnDefinition { Weapon = weapon, Position = spawn.Position });
                    }
                }
            }

            if (snapshot.ShopOffers != null)
            {
                foreach (ShopOfferSaveData offer in snapshot.ShopOffers)
                {
                    WeaponDefinition weapon = FindWeapon(offer.WeaponId);
                    if (weapon != null)
                    {
                        map.ShopOffers.Add(new ShopOfferDefinition
                        {
                            Position = offer.Position,
                            Weapon = weapon,
                            Price = offer.Price,
                            EffectSummary = offer.EffectSummary,
                        });
                    }
                }
            }

            if (snapshot.EnemySpawns != null)
            {
                foreach (EnemySpawnSaveData spawn in snapshot.EnemySpawns)
                {
                    EnemyDefinition enemy = FindEnemy(spawn.EnemyId);
                    if (enemy != null)
                    {
                        map.EnemySpawns.Add(new EnemySpawnDefinition { EnemyDefinition = enemy, Position = spawn.Position });
                    }
                }
            }

            return map;
        }

        public List<ActorEntity> RestoreActors(IReadOnlyList<ActorSaveData> snapshots)
        {
            List<ActorEntity> actors = new List<ActorEntity>();
            if (snapshots == null)
            {
                return actors;
            }

            for (int i = 0; i < snapshots.Count; i++)
            {
                ActorSaveData snapshot = snapshots[i];
                if (snapshot == null)
                {
                    continue;
                }

                ActorStats stats = snapshot.Stats ?? new ActorStats();
                ActorEntity actor = new ActorEntity
                {
                    Id = snapshot.Id,
                    DisplayName = snapshot.DisplayName,
                    GridPosition = snapshot.GridPosition,
                    FacingDirection = snapshot.FacingDirection,
                    Stats = stats,
                    EnemyDefinition = FindEnemy(snapshot.EnemyId),
                    IsEnemy = snapshot.IsEnemy,
                    Gold = snapshot.Gold,
                    TargetActorId = snapshot.TargetActorId,
                    PendingEnemyAction = snapshot.PendingEnemyAction,
                    PendingEnemyActionTurns = snapshot.PendingEnemyActionTurns,
                    HasPendingEnemyTargetCell = snapshot.HasPendingEnemyTargetCell,
                    PendingEnemyTargetCell = snapshot.PendingEnemyTargetCell,
                    PendingEnemyFacingDirection = snapshot.PendingEnemyFacingDirection,
                    PendingBossAffectedCells = Copy(snapshot.PendingBossAffectedCells),
                    ActiveBossWallCells = Copy(snapshot.ActiveBossWallCells),
                    BossTurnCount = snapshot.BossTurnCount,
                    LastSpaceCutTurn = snapshot.LastSpaceCutTurn,
                    BossAlignedTurnCount = snapshot.BossAlignedTurnCount,
                    BossStunTurnsRemaining = snapshot.BossStunTurnsRemaining,
                    BossWallTurnsRemaining = snapshot.BossWallTurnsRemaining,
                };
                actor.SetCurrentHp(snapshot.CurrentHp);
                actors.Add(actor);
            }

            return actors;
        }

        public RunState RestoreRunState(RunSaveData snapshot, RunFloorDefinition floorDefinition, ActorEntity player)
        {
            if (snapshot == null)
            {
                return null;
            }

            return new RunState
            {
                RunIndex = snapshot.RunIndex,
                CurrentFloor = snapshot.CurrentFloor,
                PlayerTurnCount = snapshot.PlayerTurnCount,
                CurrentFloorDefinition = floorDefinition,
                FloorExitUsed = snapshot.FloorExitUsed,
                BossRoomEntered = snapshot.BossRoomEntered,
                BossRoomCleared = snapshot.BossRoomCleared,
                HasEquippedWeapon = snapshot.HasEquippedWeapon,
                EquippedWeapon = FindWeapon(snapshot.EquippedWeaponId),
                IsTimingModeEnabled = snapshot.IsTimingModeEnabled,
                EndReason = snapshot.EndReason,
                Player = player,
            };
        }

        private string GetSlotPath(int slotNumber)
        {
            return Path.Combine(saveDirectory, $"slot_{slotNumber}.json");
        }

        private static bool IsValidSlot(int slotNumber)
        {
            return slotNumber >= 1 && slotNumber <= SlotCount;
        }

        private static WeaponDefinition FindWeapon(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                return null;
            }

            WeaponDefinition[] weapons = Resources.FindObjectsOfTypeAll<WeaponDefinition>();
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null && string.Equals(weapons[i].Id, weaponId, StringComparison.Ordinal))
                {
                    return weapons[i];
                }
            }

            Debug.LogWarning($"[SaveGameService] Weapon asset '{weaponId}' was not found while loading.");
            return null;
        }

        private static EnemyDefinition FindEnemy(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return null;
            }

            EnemyDefinition[] enemies = Resources.FindObjectsOfTypeAll<EnemyDefinition>();
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null && string.Equals(enemies[i].EnemyId, enemyId, StringComparison.Ordinal))
                {
                    return enemies[i];
                }
            }

            Debug.LogWarning($"[SaveGameService] Enemy asset '{enemyId}' was not found while loading.");
            return null;
        }

        private static SaveGameData BuildSaveData(
            GameState gameState,
            SaveProfile profile,
            RunState runState,
            MapDefinition map,
            IReadOnlyList<ActorEntity> actors)
        {
            SaveGameData data = new SaveGameData
            {
                SavedAtUtcTicks = DateTime.UtcNow.Ticks,
                GameState = gameState,
                Profile = profile,
            };

            if (runState == null || map == null)
            {
                return data;
            }

            data.Run = new RunSaveData
            {
                RunIndex = runState.RunIndex,
                CurrentFloor = runState.CurrentFloor,
                PlayerTurnCount = runState.PlayerTurnCount,
                FloorExitUsed = runState.FloorExitUsed,
                BossRoomEntered = runState.BossRoomEntered,
                BossRoomCleared = runState.BossRoomCleared,
                HasEquippedWeapon = runState.HasEquippedWeapon,
                EquippedWeaponId = runState.EquippedWeapon != null ? runState.EquippedWeapon.Id : string.Empty,
                IsTimingModeEnabled = runState.IsTimingModeEnabled,
                EndReason = runState.EndReason,
                Map = CaptureMap(map),
            };

            if (actors != null)
            {
                for (int i = 0; i < actors.Count; i++)
                {
                    ActorEntity actor = actors[i];
                    if (actor != null)
                    {
                        data.Run.Actors.Add(CaptureActor(actor));
                    }
                }
            }

            return data;
        }

        private static MapSaveData CaptureMap(MapDefinition map)
        {
            MapSaveData snapshot = new MapSaveData
            {
                RunFloor = map.RunFloor,
                PlayerSpawn = map.PlayerSpawn,
                FloorExitPosition = map.FloorExitPosition,
                DungeonEntrancePosition = map.DungeonEntrancePosition,
                ShopEntrancePosition = map.ShopEntrancePosition,
                ShopInteriorSpawnPosition = map.ShopInteriorSpawnPosition,
                ShopExitPosition = map.ShopExitPosition,
                BossRoomId = map.BossRoomId,
                BossEntranceBlockCells = Copy(map.BossEntranceBlockCells),
                WalkableCells = Copy(map.WalkableCells),
                WallCells = Copy(map.WallCells),
                ShopCells = Copy(map.ShopCells),
                Rooms = map.Rooms != null ? new List<DungeonRoomDefinition>(map.Rooms) : new List<DungeonRoomDefinition>(),
                Corridors = map.Corridors != null ? new List<DungeonCorridorDefinition>(map.Corridors) : new List<DungeonCorridorDefinition>(),
            };

            if (map.WeaponSpawns != null)
            {
                foreach (WeaponSpawnDefinition spawn in map.WeaponSpawns)
                {
                    if (spawn?.Weapon != null)
                    {
                        snapshot.WeaponSpawns.Add(new WeaponSpawnSaveData { WeaponId = spawn.Weapon.Id, Position = spawn.Position });
                    }
                }
            }

            if (map.ShopOffers != null)
            {
                foreach (ShopOfferDefinition offer in map.ShopOffers)
                {
                    if (offer?.Weapon != null)
                    {
                        snapshot.ShopOffers.Add(new ShopOfferSaveData
                        {
                            Position = offer.Position,
                            WeaponId = offer.Weapon.Id,
                            Price = offer.Price,
                            EffectSummary = offer.EffectSummary,
                        });
                    }
                }
            }

            if (map.EnemySpawns != null)
            {
                foreach (EnemySpawnDefinition spawn in map.EnemySpawns)
                {
                    if (spawn?.EnemyDefinition != null)
                    {
                        snapshot.EnemySpawns.Add(new EnemySpawnSaveData
                        {
                            EnemyId = spawn.EnemyDefinition.EnemyId,
                            Position = spawn.Position,
                        });
                    }
                }
            }

            return snapshot;
        }

        private static ActorSaveData CaptureActor(ActorEntity actor)
        {
            return new ActorSaveData
            {
                Id = actor.Id,
                DisplayName = actor.DisplayName,
                GridPosition = actor.GridPosition,
                FacingDirection = actor.FacingDirection,
                Stats = actor.Stats != null ? actor.Stats.Clone() : null,
                EnemyId = actor.EnemyDefinition != null ? actor.EnemyDefinition.EnemyId : string.Empty,
                CurrentHp = actor.CurrentHp,
                IsEnemy = actor.IsEnemy,
                Gold = actor.Gold,
                TargetActorId = actor.TargetActorId,
                PendingEnemyAction = actor.PendingEnemyAction,
                PendingEnemyActionTurns = actor.PendingEnemyActionTurns,
                HasPendingEnemyTargetCell = actor.HasPendingEnemyTargetCell,
                PendingEnemyTargetCell = actor.PendingEnemyTargetCell,
                PendingEnemyFacingDirection = actor.PendingEnemyFacingDirection,
                PendingBossAffectedCells = Copy(actor.PendingBossAffectedCells),
                ActiveBossWallCells = Copy(actor.ActiveBossWallCells),
                BossTurnCount = actor.BossTurnCount,
                LastSpaceCutTurn = actor.LastSpaceCutTurn,
                BossAlignedTurnCount = actor.BossAlignedTurnCount,
                BossStunTurnsRemaining = actor.BossStunTurnsRemaining,
                BossWallTurnsRemaining = actor.BossWallTurnsRemaining,
            };
        }

        private static List<Vector2Int> Copy(List<Vector2Int> source)
        {
            return source != null ? new List<Vector2Int>(source) : new List<Vector2Int>();
        }
    }

    public static class SaveGameLaunchContext
    {
        private static int pendingSlotNumber;

        public static void RequestLoad(int slotNumber)
        {
            pendingSlotNumber = slotNumber;
        }

        public static bool TryConsumeLoadRequest(out int slotNumber)
        {
            slotNumber = pendingSlotNumber;
            pendingSlotNumber = 0;
            return slotNumber >= 1 && slotNumber <= SaveGameService.SlotCount;
        }

        public static void Clear()
        {
            pendingSlotNumber = 0;
        }
    }
}
