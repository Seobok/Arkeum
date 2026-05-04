using System;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    [Serializable]
    public sealed class WeaponSpawnDefinition
    {
        public WeaponDefinition Weapon;
        public Vector2Int Position;
    }
}
