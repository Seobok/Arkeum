using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arkeum.Production.Gameplay.Actors
{
    [Serializable]
    public sealed class ActorEntity
    {
        public string Id;
        public string DisplayName;
        public Vector2Int GridPosition;
        public Vector2Int FacingDirection = Vector2Int.up;
        public ActorStats Stats = new ActorStats();
        public EnemyDefinition EnemyDefinition;
        [FormerlySerializedAs("CurrentHp")]
        [SerializeField] private int currentHp = 1;
        public bool IsEnemy;
        [FormerlySerializedAs("BloodReward")]
        public int Gold;
        public string TargetActorId;
        public EnemyActionType PendingEnemyAction = EnemyActionType.None;
        public int PendingEnemyActionTurns;
        public bool HasPendingEnemyTargetCell;
        public Vector2Int PendingEnemyTargetCell;
        public Vector2Int PendingEnemyFacingDirection = Vector2Int.up;

        public int CurrentHp => currentHp;
        public bool IsAlive => CurrentHp > 0;
        public bool IsPlayer => !IsEnemy;

        public event Action CurrentHpChanged;
        public event Action MaxHpChanged;

        public void SetCurrentHp(int value)
        {
            EnsureStats();
            int previous = CurrentHp;
            int next = Mathf.Clamp(value, 0, Stats.MaxHp);

            if (previous == next)
            {
                return;
            }

            currentHp = next;
            CurrentHpChanged?.Invoke();
        }

        public void SetMaxHp(int value)
        {
            EnsureStats();
            int previous = Stats.MaxHp;
            int next = Mathf.Max(1, value);

            if (previous == next)
            {
                return;
            }

            Stats.SetMaxHp(next);
            MaxHpChanged?.Invoke();

            if (CurrentHp > next)
            {
                SetCurrentHp(next);
            }
        }

        private void EnsureStats()
        {
            if (Stats == null)
            {
                Stats = new ActorStats();
            }
        }
    }
}
