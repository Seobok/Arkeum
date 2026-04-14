using System;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Actors
{
    [Serializable]
    public sealed class ActorEntity
    {
        public string Id;
        public string DisplayName;
        public BrainType BrainType;
        public Vector2Int GridPosition;
        public ActorStats Stats = new ActorStats();
        public int CurrentHp = 1;
        public bool IsEnemy;
        public int BloodReward;

        public bool IsAlive => CurrentHp > 0;
        public bool IsPlayer => !IsEnemy;
    }
}
