using System.Collections.Generic;
using Arkeum.Production.Gameplay.Combat;
using Arkeum.Production.Gameplay.Map;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Actors
{
    public sealed class EnemyBehaviorActions
    {
        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        private readonly CombatSystem combatSystem;
        private readonly TargetingService targetingService;

        public EnemyBehaviorActions(CombatSystem combatSystem, TargetingService targetingService)
        {
            this.combatSystem = combatSystem;
            this.targetingService = targetingService;
        }

        public BehaviorTreeStatus UpdateTarget(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            ActorEntity player = context.Player;
            enemy.TargetActorId = IsInDetectionRange(enemy, player.GridPosition) ? player.Id : null;
            return BehaviorTreeStatus.Success;
        }

        public bool HasTarget(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            ActorEntity player = context.Player;
            return !string.IsNullOrEmpty(enemy.TargetActorId) && enemy.TargetActorId == player.Id;
        }

        public bool HasNoTarget(EnemyBehaviorContext context)
        {
            return !HasTarget(context);
        }

        public bool HasPendingAttack(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            return enemy.PendingEnemyAction == EnemyActionType.Attack && enemy.HasPendingEnemyTargetCell;
        }

        public bool HasPendingMove(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            return (enemy.PendingEnemyAction == EnemyActionType.WanderMove ||
                    enemy.PendingEnemyAction == EnemyActionType.ChaseMove) &&
                   enemy.HasPendingEnemyTargetCell;
        }

        public BehaviorTreeStatus FaceTarget(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            enemy.FacingDirection = GetFacingToward(enemy.GridPosition, context.Player.GridPosition, enemy.FacingDirection);
            return BehaviorTreeStatus.Success;
        }

        public bool CanAttackTarget(EnemyBehaviorContext context)
        {
            return HasPendingAttack(context) || CanAttack(context.Enemy, context.Player.GridPosition);
        }

        public BehaviorTreeStatus AttackTarget(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            ActorEntity player = context.Player;
            Vector2Int targetCell = HasPendingAttack(context)
                ? enemy.PendingEnemyTargetCell
                : player.GridPosition;
            if (!TryCompletePreparation(enemy, EnemyActionType.Attack, enemy.Stats.AttackPreparationTurns, targetCell))
            {
                return BehaviorTreeStatus.Running;
            }

            enemy.FacingDirection = enemy.PendingEnemyFacingDirection;
            if (player.GridPosition == enemy.PendingEnemyTargetCell)
            {
                combatSystem.ResolveEnemyAttack(enemy, player);
            }

            ClearPreparation(enemy);
            return BehaviorTreeStatus.Success;
        }

        public BehaviorTreeStatus WanderMove(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            if (!TryGetWanderTarget(context, out Vector2Int targetCell))
            {
                ClearPreparation(enemy);
                return BehaviorTreeStatus.Failure;
            }

            if (!TryCompletePreparation(enemy, EnemyActionType.WanderMove, enemy.Stats.MovePreparationTurns, targetCell))
            {
                return BehaviorTreeStatus.Running;
            }

            ExecutePreparedMove(context);
            return BehaviorTreeStatus.Success;
        }

        public BehaviorTreeStatus ChaseMove(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            Vector2Int targetCell = HasPendingMove(context)
                ? enemy.PendingEnemyTargetCell
                : GetChaseTargetCell(context);
            if (targetCell == enemy.GridPosition)
            {
                ClearPreparation(enemy);
                return BehaviorTreeStatus.Failure;
            }

            if (!TryCompletePreparation(enemy, EnemyActionType.ChaseMove, enemy.Stats.MovePreparationTurns, targetCell))
            {
                return BehaviorTreeStatus.Running;
            }

            ExecutePreparedMove(context);
            return BehaviorTreeStatus.Success;
        }

        public BehaviorTreeStatus MoveToPreparedTarget(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            if (!enemy.HasPendingEnemyTargetCell)
            {
                ClearPreparation(enemy);
                return BehaviorTreeStatus.Failure;
            }

            if (!TryCompletePreparation(enemy, enemy.PendingEnemyAction, enemy.Stats.MovePreparationTurns, enemy.PendingEnemyTargetCell))
            {
                return BehaviorTreeStatus.Running;
            }

            ExecutePreparedMove(context);
            return BehaviorTreeStatus.Success;
        }

        private static bool TryGetWanderTarget(EnemyBehaviorContext context, out Vector2Int targetCell)
        {
            ActorEntity enemy = context.Enemy;
            if (enemy.PendingEnemyAction == EnemyActionType.WanderMove && enemy.HasPendingEnemyTargetCell)
            {
                targetCell = enemy.PendingEnemyTargetCell;
                return true;
            }

            List<Vector2Int> candidates = new List<Vector2Int>();
            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                Vector2Int direction = CardinalDirections[i];
                Vector2Int candidate = enemy.GridPosition + direction;
                if (CanMoveTo(candidate, context.Player.GridPosition, context.MapService, context.ActorRepository))
                {
                    candidates.Add(candidate);
                }
            }

            if (candidates.Count > 0)
            {
                targetCell = candidates[Random.Range(0, candidates.Count)];
                return true;
            }

            targetCell = enemy.GridPosition;
            return false;
        }

        private Vector2Int GetChaseTargetCell(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            Vector2Int current = enemy.GridPosition;
            int movementRange = Mathf.Max(1, enemy.Stats.MovementRange);
            for (int i = 0; i < movementRange; i++)
            {
                Vector2Int step = GetChaseStep(current, context.Player.GridPosition, context.MapService, context.ActorRepository);
                Vector2Int targetCell = current + step;
                if (step == Vector2Int.zero ||
                    !CanMoveTo(targetCell, context.Player.GridPosition, context.MapService, context.ActorRepository))
                {
                    break;
                }

                current = targetCell;
            }

            return current;
        }

        private static void ExecutePreparedMove(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            Vector2Int targetCell = enemy.PendingEnemyTargetCell;
            if (enemy.HasPendingEnemyTargetCell &&
                CanMoveTo(targetCell, context.Player.GridPosition, context.MapService, context.ActorRepository))
            {
                enemy.FacingDirection = GetFacingToward(enemy.GridPosition, targetCell, enemy.FacingDirection);
                enemy.GridPosition = targetCell;
            }

            ClearPreparation(enemy);
        }

        private Vector2Int GetChaseStep(Vector2Int enemyPosition, Vector2Int playerPosition, MapService mapService, ActorRepository actorRepository)
        {
            Vector2Int primaryStep = targetingService.GetNextStep(enemyPosition, playerPosition);
            if (CanMoveTo(enemyPosition + primaryStep, playerPosition, mapService, actorRepository))
            {
                return primaryStep;
            }

            Vector2Int alternateStep = GetAlternateStep(enemyPosition, playerPosition, primaryStep);
            if (CanMoveTo(enemyPosition + alternateStep, playerPosition, mapService, actorRepository))
            {
                return alternateStep;
            }

            return Vector2Int.zero;
        }

        private static Vector2Int GetAlternateStep(Vector2Int enemyPosition, Vector2Int playerPosition, Vector2Int primaryStep)
        {
            Vector2Int delta = playerPosition - enemyPosition;
            if (primaryStep.x != 0 && delta.y != 0)
            {
                return new Vector2Int(0, delta.y > 0 ? 1 : -1);
            }

            if (primaryStep.y != 0 && delta.x != 0)
            {
                return new Vector2Int(delta.x > 0 ? 1 : -1, 0);
            }

            return Vector2Int.zero;
        }

        private static bool CanMoveTo(Vector2Int targetCell, Vector2Int playerPosition, MapService mapService, ActorRepository actorRepository)
        {
            return targetCell != playerPosition &&
                   mapService.IsWalkableCell(targetCell) &&
                   !actorRepository.IsEnemyOccupied(targetCell);
        }

        private static bool IsInDetectionRange(ActorEntity enemy, Vector2Int playerPosition)
        {
            int detectionRange = Mathf.Max(0, enemy.Stats.DetectionRange);
            Vector2Int delta = playerPosition - enemy.GridPosition;
            return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) <= detectionRange;
        }

        private static bool TryCompletePreparation(ActorEntity enemy, EnemyActionType actionType, int requiredTurns, Vector2Int targetCell)
        {
            requiredTurns = Mathf.Max(0, requiredTurns);
            if (enemy.PendingEnemyAction != actionType)
            {
                enemy.PendingEnemyAction = actionType;
                enemy.PendingEnemyActionTurns = 0;
                enemy.PendingEnemyTargetCell = targetCell;
                enemy.PendingEnemyFacingDirection = enemy.FacingDirection;
                enemy.HasPendingEnemyTargetCell = true;
                return requiredTurns == 0;
            }

            if (!enemy.HasPendingEnemyTargetCell)
            {
                enemy.PendingEnemyTargetCell = targetCell;
                enemy.PendingEnemyFacingDirection = enemy.FacingDirection;
                enemy.HasPendingEnemyTargetCell = true;
            }

            if (requiredTurns == 0)
            {
                return true;
            }

            enemy.PendingEnemyActionTurns += 1;
            return enemy.PendingEnemyActionTurns >= requiredTurns;
        }

        private static void ClearPreparation(ActorEntity enemy)
        {
            enemy.PendingEnemyAction = EnemyActionType.None;
            enemy.PendingEnemyActionTurns = 0;
            enemy.PendingEnemyTargetCell = Vector2Int.zero;
            enemy.PendingEnemyFacingDirection = Vector2Int.up;
            enemy.HasPendingEnemyTargetCell = false;
        }

        private static bool CanAttack(ActorEntity enemy, Vector2Int target)
        {
            EnemyAttackPatternDefinition attackPattern = enemy.EnemyDefinition != null
                ? enemy.EnemyDefinition.AttackPattern
                : null;
            if (attackPattern != null)
            {
                return attackPattern.ContainsTarget(enemy.GridPosition, enemy.FacingDirection, target);
            }

            Vector2Int delta = target - enemy.GridPosition;
            return IsStraightLine(delta, 1);
        }

        private static bool IsStraightLine(Vector2Int delta, int range)
        {
            return delta != Vector2Int.zero &&
                   (delta.x == 0 || delta.y == 0) &&
                   Mathf.Abs(delta.x) + Mathf.Abs(delta.y) <= range;
        }

        private static Vector2Int GetFacingToward(Vector2Int origin, Vector2Int target, Vector2Int fallback)
        {
            Vector2Int delta = target - origin;
            if (delta == Vector2Int.zero)
            {
                return fallback;
            }

            return EnemyAttackPatternDefinition.NormalizeFacing(delta);
        }
    }
}
