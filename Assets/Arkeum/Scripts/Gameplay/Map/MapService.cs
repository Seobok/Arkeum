using System.Collections.Generic;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Map
{
    public sealed class MapService
    {
        private readonly MapGenerator mapGenerator;
        private readonly TileOccupancyService tileOccupancyService;
        private readonly HashSet<Vector2Int> walkableCells = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> wallCells = new HashSet<Vector2Int>();

        public MapDefinition CurrentMap { get; private set; }
        public RunFloorDefinition CurrentRunFloor { get; private set; }

        public MapService(MapGenerator mapGenerator, TileOccupancyService tileOccupancyService)
        {
            this.mapGenerator = mapGenerator;
            this.tileOccupancyService = tileOccupancyService;
        }

        public RunFloorDefinition GetRunFloor(int floorIndex)
        {
            return mapGenerator.GetRunFloor(floorIndex);
        }

        public void LoadRunFloor(RunFloorDefinition floorDefinition, int fallbackFloor)
        {
            CurrentRunFloor = floorDefinition;
            Debug.Log(
                $"[MapService] LoadRunFloor requested fallbackFloor={fallbackFloor}, " +
                $"floorDefinition={(floorDefinition != null ? floorDefinition.FloorIndex.ToString() : "null")}");
            SetCurrentMap(mapGenerator.CreateRunMap(floorDefinition, fallbackFloor));
        }

        public void LoadHubMap()
        {
            CurrentRunFloor = null;
            Debug.Log("[MapService] LoadHubMap requested.");
            SetCurrentMap(mapGenerator.CreateHubMap());
        }

        public bool IsWalkable(Vector2Int cell)
        {
            return walkableCells.Contains(cell) && !wallCells.Contains(cell) && !tileOccupancyService.IsOccupied(cell);
        }

        public bool IsWalkableCell(Vector2Int cell)
        {
            return walkableCells.Contains(cell);
        }

        public bool BlocksLineOfSight(Vector2Int cell)
        {
            return wallCells.Contains(cell);
        }

        public bool BlocksAttack(Vector2Int cell)
        {
            return wallCells.Contains(cell);
        }

        public bool SetRuntimeWall(Vector2Int cell, bool hasWall)
        {
            if (CurrentMap == null || !walkableCells.Contains(cell))
            {
                return false;
            }

            if (hasWall)
            {
                bool added = wallCells.Add(cell);
                if (!CurrentMap.WallCells.Contains(cell))
                {
                    CurrentMap.WallCells.Add(cell);
                }

                return added;
            }

            bool removed = wallCells.Remove(cell);
            CurrentMap.WallCells.RemoveAll(wallCell => wallCell == cell);
            return removed;
        }

        public bool BlocksLineOfSightBetween(Vector2Int from, Vector2Int to)
        {
            return HasBlockingCellBetween(from, to, BlocksLineOfSight);
        }

        public bool BlocksAttackBetween(Vector2Int from, Vector2Int to)
        {
            return HasBlockingCellBetween(from, to, BlocksAttack);
        }

        public bool TryGetWeaponSpawn(Vector2Int cell, out WeaponSpawnDefinition weaponSpawn)
        {
            if (CurrentMap?.WeaponSpawns != null)
            {
                for (int i = 0; i < CurrentMap.WeaponSpawns.Count; i++)
                {
                    weaponSpawn = CurrentMap.WeaponSpawns[i];
                    if (weaponSpawn != null && weaponSpawn.Position == cell)
                    {
                        return true;
                    }
                }
            }

            weaponSpawn = null;
            return false;
        }

        public bool TryPickupWeaponAt(
            Vector2Int cell,
            bool hasDroppedWeapon,
            WeaponDefinition droppedWeapon,
            out WeaponSpawnDefinition pickedUpWeaponSpawn)
        {
            if (CurrentMap?.WeaponSpawns == null)
            {
                pickedUpWeaponSpawn = null;
                return false;
            }

            for (int i = 0; i < CurrentMap.WeaponSpawns.Count; i++)
            {
                WeaponSpawnDefinition weaponSpawn = CurrentMap.WeaponSpawns[i];
                if (weaponSpawn == null || weaponSpawn.Position != cell)
                {
                    continue;
                }

                pickedUpWeaponSpawn = weaponSpawn;
                CurrentMap.WeaponSpawns.RemoveAt(i);
                if (hasDroppedWeapon)
                {
                    CurrentMap.WeaponSpawns.Add(new WeaponSpawnDefinition
                    {
                        Weapon = droppedWeapon,
                        Position = cell,
                    });
                }

                return true;
            }

            pickedUpWeaponSpawn = null;
            return false;
        }

        private void SetCurrentMap(MapDefinition mapDefinition)
        {
            CurrentMap = mapDefinition;
            walkableCells.Clear();
            wallCells.Clear();
            tileOccupancyService.Clear();

            if (mapDefinition == null)
            {
                Debug.LogError("[MapService] SetCurrentMap received null map.");
                return;
            }

            for (int i = 0; i < mapDefinition.WalkableCells.Count; i++)
            {
                walkableCells.Add(mapDefinition.WalkableCells[i]);
            }

            for (int i = 0; i < mapDefinition.WallCells.Count; i++)
            {
                wallCells.Add(mapDefinition.WallCells[i]);
            }

            Debug.Log(
                $"[MapService] Current map set. floor={mapDefinition.RunFloor}, " +
                $"walkableCells={walkableCells.Count}, " +
                $"wallCells={wallCells.Count}, " +
                $"rooms={mapDefinition.Rooms.Count}, corridors={mapDefinition.Corridors.Count}, " +
                $"playerSpawn={mapDefinition.PlayerSpawn}, floorExit={mapDefinition.FloorExitPosition}");
        }

        private static bool HasBlockingCellBetween(Vector2Int from, Vector2Int to, System.Func<Vector2Int, bool> blocks)
        {
            Vector2Int delta = to - from;
            int steps = GreatestCommonDivisor(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            if (steps <= 1)
            {
                return false;
            }

            Vector2Int step = new Vector2Int(delta.x / steps, delta.y / steps);
            Vector2Int current = from;
            for (int i = 1; i < steps; i++)
            {
                current += step;
                if (blocks(current))
                {
                    return true;
                }
            }

            return false;
        }

        private static int GreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                int next = a % b;
                a = b;
                b = next;
            }

            return a;
        }
    }
}
