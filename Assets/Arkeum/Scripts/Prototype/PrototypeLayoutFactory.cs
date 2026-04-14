using UnityEngine;

namespace Arkeum.Prototype
{
    public sealed class PrototypeLayoutFactory
    {
        public DungeonLayout BuildRunLayout()
        {
            DungeonLayout newLayout = new DungeonLayout
            {
                PlayerSpawn = new Vector2Int(0, 0),
                MerchantPosition = new Vector2Int(9, -1),
                ReliquaryPosition = new Vector2Int(14, 0),
            };

            AddRoom(newLayout, -1, -2, 5, 2, 1);
            AddCorridor(newLayout, 5, 0, 9, 1);
            AddRoom(newLayout, 8, -2, 11, 2, 1);
            AddCorridor(newLayout, 11, 0, 14, 2);
            AddRoom(newLayout, 12, -2, 15, 2, 2);
            newLayout.TemporaryWeaponSpawns.Add(new Vector2Int(10, 2));

            return newLayout;
        }

        public HubSceneLayout BuildHubLayout()
        {
            HubSceneLayout hubSceneLayout = new HubSceneLayout
            {
                Layout = new DungeonLayout(),
                StartGatePosition = new Vector2Int(-2, 0),
                UnlockPosition = new Vector2Int(0, 2),
                UndertakerPosition = new Vector2Int(2, 0),
                PlayerPosition = new Vector2Int(0, 0),
            };

            AddRoom(hubSceneLayout.Layout, -3, -2, 3, 2, 0);
            return hubSceneLayout;
        }

        private void AddRoom(DungeonLayout target, int minX, int minY, int maxX, int maxY, int depth)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    target.Walkable.Add(cell);
                    target.DepthByCell[cell] = depth;
                }
            }
        }

        private void AddCorridor(DungeonLayout target, int minX, int y, int maxX, int depth)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                target.Walkable.Add(cell);
                target.DepthByCell[cell] = depth;
            }
        }
    }
}
