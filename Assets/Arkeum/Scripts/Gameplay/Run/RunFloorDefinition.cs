using System;
using System.Collections.Generic;
using Arkeum.Production.Gameplay.Map;

namespace Arkeum.Production.Gameplay.Run
{
    [Serializable]
    public sealed class RunFloorDefinition
    {
        public int FloorIndex = 1;
        public MapAsset MapAsset;
        public int MinimumRoomCount = 6;
        public int RoomGap = 5;
        public int PlacementAttempts = 300;
        public int RandomSeed = 173;
        public List<EnemySpawnDefinition> EnemySpawns = new List<EnemySpawnDefinition>();
    }
}
