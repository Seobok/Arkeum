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
            return walkableCells.Contains(cell) && !tileOccupancyService.IsOccupied(cell);
        }

        public bool IsWalkableCell(Vector2Int cell)
        {
            return walkableCells.Contains(cell);
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

        private void SetCurrentMap(MapDefinition mapDefinition)
        {
            CurrentMap = mapDefinition;
            walkableCells.Clear();
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

            Debug.Log(
                $"[MapService] Current map set. floor={mapDefinition.RunFloor}, " +
                $"walkableCells={walkableCells.Count}, " +
                $"rooms={mapDefinition.Rooms.Count}, corridors={mapDefinition.Corridors.Count}, " +
                $"playerSpawn={mapDefinition.PlayerSpawn}, floorExit={mapDefinition.FloorExitPosition}");
        }
    }
}
