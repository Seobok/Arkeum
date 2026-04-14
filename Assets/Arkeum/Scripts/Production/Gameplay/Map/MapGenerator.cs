namespace Arkeum.Production.Gameplay.Map
{
    public sealed class MapGenerator
    {
        public MapDefinition CreateRunMap()
        {
            MapDefinition map = new MapDefinition
            {
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
            MapDefinition map = new MapDefinition
            {
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
    }
}
