using System;
using Arkeum.Production.Gameplay.Actors;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    [Serializable]
    public sealed class EnemySpawnDefinition
    {
        public EnemyDefinition EnemyDefinition;
        public Vector2Int Position;
    }
}
