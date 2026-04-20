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
        public List<EnemySpawnDefinition> EnemySpawns = new List<EnemySpawnDefinition>();
    }
}
