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
        private readonly Dictionary<Vector2Int, int> depthByCell = new Dictionary<Vector2Int, int>();

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
            SetCurrentMap(mapGenerator.CreateRunMap(floorDefinition, fallbackFloor));
        }

        public void LoadHubMap()
        {
            CurrentRunFloor = null;
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

        public int GetDepth(Vector2Int cell)
        {
            if (depthByCell.TryGetValue(cell, out int depth))
            {
                return depth;
            }

            return 1;
        }

        public bool IsTemporaryWeaponSpawn(Vector2Int cell)
        {
            return CurrentMap != null && CurrentMap.TemporaryWeaponSpawns.Contains(cell);
        }

        private void SetCurrentMap(MapDefinition mapDefinition)
        {
            CurrentMap = mapDefinition;
            walkableCells.Clear();
            depthByCell.Clear();
            tileOccupancyService.Clear();

            if (mapDefinition == null)
            {
                return;
            }

            for (int i = 0; i < mapDefinition.WalkableCells.Count; i++)
            {
                walkableCells.Add(mapDefinition.WalkableCells[i]);
            }

            foreach (KeyValuePair<Vector2Int, int> pair in mapDefinition.DepthByCell)
            {
                depthByCell[pair.Key] = pair.Value;
            }
        }
    }
}
