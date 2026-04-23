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
        public Vector2Int FacingDirection = Vector2Int.up;
        public ActorStats Stats = new ActorStats();
        public EnemyDefinition EnemyDefinition;
        public int CurrentHp = 1;
        public bool IsEnemy;
        public int BloodReward;
        public string TargetActorId;
        public EnemyActionType PendingEnemyAction = EnemyActionType.None;
        public int PendingEnemyActionTurns;
        public bool HasPendingEnemyTargetCell;
        public Vector2Int PendingEnemyTargetCell;
        public Vector2Int PendingEnemyFacingDirection = Vector2Int.up;

        public bool IsAlive => CurrentHp > 0;
        public bool IsPlayer => !IsEnemy;
    }
}
