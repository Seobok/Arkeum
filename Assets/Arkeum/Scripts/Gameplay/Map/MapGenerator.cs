using Arkeum.Production.Gameplay.Run;

namespace Arkeum.Production.Gameplay.Map
{
    public sealed class MapGenerator
    {
        private readonly MapAsset runMapAsset;
        private readonly MapAsset hubMapAsset;
        private readonly RunDefinition runDefinition;

        public MapGenerator(MapAsset runMapAsset, MapAsset hubMapAsset, RunDefinition runDefinition)
        {
            this.runMapAsset = runMapAsset;
            this.hubMapAsset = hubMapAsset;
            this.runDefinition = runDefinition;
        }

        public RunFloorDefinition GetRunFloor(int floorIndex)
        {
            return runDefinition != null ? runDefinition.GetFloor(floorIndex) : null;
        }

        public MapDefinition CreateRunMap(RunFloorDefinition floorDefinition, int fallbackFloor)
        {
            int floor = floorDefinition != null ? floorDefinition.FloorIndex : fallbackFloor;
            MapAsset floorMapAsset = floorDefinition != null && floorDefinition.MapAsset != null
                ? floorDefinition.MapAsset
                : runMapAsset;

            if (TryCreateFromAsset(floorMapAsset, out MapDefinition assetMap))
            {
                assetMap.RunFloor = floor;
                return assetMap;
            }

            MapDefinition map = new MapDefinition
            {
                RunFloor = floor,
                PlayerSpawn = new UnityEngine.Vector2Int(0, 0),
                MerchantPosition = new UnityEngine.Vector2Int(9, -1),
                ReliquaryPosition = new UnityEngine.Vector2Int(14, 0),
            };

            AddRoom(map, -1, -2, 5, 2, 1);
            AddCorridor(map, 5, 0, 9, 1);
            AddRoom(map, 8, -2, 11, 2, 1);
            AddCorridor(map, 11, 0, 14, 2);
            AddRoom(map, 12, -2, 15, 2, 2);
            map.TemporaryWeaponSpawns.Add(new UnityEngine.Vector2Int(10, 2));
            return map;
        }

        public MapDefinition CreateHubMap()
        {
            if (TryCreateFromAsset(hubMapAsset, out MapDefinition assetMap))
            {
                return assetMap;
            }

            MapDefinition map = new MapDefinition
            {
                RunFloor = 0,
                PlayerSpawn = new UnityEngine.Vector2Int(0, 0),
                StartAltarPosition = new UnityEngine.Vector2Int(-2, 0),
                UnlockAltarPosition = new UnityEngine.Vector2Int(0, 2),
                UndertakerPosition = new UnityEngine.Vector2Int(2, 0),
            };

            AddRoom(map, -3, -2, 3, 2, 0);
            return map;
        }

        private void AddRoom(MapDefinition map, int minX, int minY, int maxX, int maxY, int depth)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    UnityEngine.Vector2Int cell = new UnityEngine.Vector2Int(x, y);
                    map.WalkableCells.Add(cell);
                    map.DepthByCell[cell] = depth;
                }
            }
        }

        private void AddCorridor(MapDefinition map, int minX, int y, int maxX, int depth)
        {
            for (int x = minX; x <= maxX; x++)
            {
                UnityEngine.Vector2Int cell = new UnityEngine.Vector2Int(x, y);
                map.WalkableCells.Add(cell);
                map.DepthByCell[cell] = depth;
            }
        }

        private static bool TryCreateFromAsset(MapAsset asset, out MapDefinition map)
        {
            if (asset == null)
            {
                map = null;
                return false;
            }

            map = new MapDefinition
            {
                RunFloor = 0,
                PlayerSpawn = asset.PlayerSpawn,
                MerchantPosition = asset.MerchantPosition,
                ReliquaryPosition = asset.ReliquaryPosition,
                StartAltarPosition = asset.StartAltarPosition,
                UnlockAltarPosition = asset.UnlockAltarPosition,
                UndertakerPosition = asset.UndertakerPosition,
            };

            for (int i = 0; i < asset.TemporaryWeaponSpawns.Count; i++)
            {
                map.TemporaryWeaponSpawns.Add(asset.TemporaryWeaponSpawns[i]);
            }

            for (int i = 0; i < asset.Cells.Count; i++)
            {
                MapCellData cell = asset.Cells[i];
                if (cell == null || !cell.Walkable)
                {
                    continue;
                }

                map.WalkableCells.Add(cell.Position);
                map.DepthByCell[cell.Position] = cell.Depth;
            }

            return map.WalkableCells.Count > 0;
        }
    }
}
