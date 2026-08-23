using System.Collections.Generic;
using Arkeum.Production.Gameplay.Combat;
using Arkeum.Production.Gameplay.Map;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Actors
{
    public sealed class EnemyBehaviorActions
    {
        private const int EnemyMoveCollisionDamage = 1;

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
            if (context.MapService.IsPlayerHiddenFromEnemies(player.GridPosition))
            {
                enemy.TargetActorId = null;
                ClearPreparation(enemy);
                return BehaviorTreeStatus.Success;
            }

            enemy.TargetActorId = IsInDetectionRange(enemy, player.GridPosition, context.MapService) ? player.Id : null;
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
            return HasPendingAttack(context) || CanAttack(context.Enemy, context.Player.GridPosition, context.MapService);
        }

        public BehaviorTreeStatus AttackTarget(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            ActorEntity player = context.Player;
            Vector2Int targetCell;
            Vector2Int attackFacing;
            if (HasPendingAttack(context))
            {
                targetCell = enemy.PendingEnemyTargetCell;
                attackFacing = enemy.PendingEnemyFacingDirection;
            }
            else if (!TryGetAttackTarget(enemy, player.GridPosition, context.MapService, out targetCell, out attackFacing))
            {
                return BehaviorTreeStatus.Failure;
            }

            if (!TryCompletePreparation(enemy, EnemyActionType.Attack, enemy.Stats.AttackPreparationTurns, targetCell, attackFacing))
            {
                return BehaviorTreeStatus.Running;
            }

            enemy.FacingDirection = enemy.PendingEnemyFacingDirection;
            if (IsInPreparedAttackRange(enemy, player.GridPosition, context.MapService))
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

        public BehaviorTreeStatus TickBoss(EnemyBehaviorContext context)
        {
            ActorEntity boss = context.Enemy;
            EnemyDefinition definition = boss.EnemyDefinition;
            if (definition == null || !definition.IsBoss)
            {
                return BehaviorTreeStatus.Failure;
            }

            boss.BossTurnCount += 1;
            AdvanceBossWalls(boss, context.MapService);

            if (boss.BossStunTurnsRemaining > 0)
            {
                boss.BossStunTurnsRemaining -= 1;
                boss.BossAlignedTurnCount = 0;
                ClearPreparation(boss);
                return BehaviorTreeStatus.Success;
            }

            if (IsBossPattern(boss.PendingEnemyAction))
            {
                ExecutePreparedBossPattern(context);
                return BehaviorTreeStatus.Success;
            }

            if (HasPendingMove(context))
            {
                return MoveToPreparedTarget(context);
            }

            ActorEntity player = context.Player;
            bool isInCloseAttackArea = IsAdjacentEightWay(boss.GridPosition, player.GridPosition);
            bool isChargeAlignment = !isInCloseAttackArea &&
                                     IsSameRowOrColumn(boss.GridPosition, player.GridPosition);
            boss.BossAlignedTurnCount = isChargeAlignment ? boss.BossAlignedTurnCount + 1 : 0;

            if (isInCloseAttackArea)
            {
                PrepareCloseAttack(context);
                return BehaviorTreeStatus.Running;
            }

            if (isChargeAlignment && boss.BossAlignedTurnCount >= 2)
            {
                PrepareCharge(context);
                boss.BossAlignedTurnCount = 0;
                return BehaviorTreeStatus.Running;
            }

            if (boss.BossTurnCount - boss.LastSpaceCutTurn >= definition.SpaceCutInterval)
            {
                PrepareSpaceCut(context);
                boss.LastSpaceCutTurn = boss.BossTurnCount;
                return BehaviorTreeStatus.Running;
            }

            if (isChargeAlignment)
            {
                // 첫 정렬 감지 후에는 이동하지 않고, 플레이어가 한 번 더 행동할 때까지 직선을 유지하는지 확인한다.
                ClearPreparation(boss);
                return BehaviorTreeStatus.Running;
            }

            boss.FacingDirection = GetFacingToward(boss.GridPosition, player.GridPosition, boss.FacingDirection);
            return ChaseMove(context);
        }

        private void ExecutePreparedBossPattern(EnemyBehaviorContext context)
        {
            ActorEntity boss = context.Enemy;
            switch (boss.PendingEnemyAction)
            {
                case EnemyActionType.BossSpaceCut:
                    ExecuteSpaceCut(context);
                    break;
                case EnemyActionType.BossCloseAttack:
                    if (boss.PendingBossAffectedCells.Contains(context.Player.GridPosition))
                    {
                        combatSystem.ResolveEnemyAttack(boss, context.Player);
                    }
                    break;
                case EnemyActionType.BossCharge:
                    ExecuteCharge(context);
                    break;
            }

            ClearPreparation(boss);
        }

        private static void PrepareSpaceCut(EnemyBehaviorContext context)
        {
            ActorEntity boss = context.Enemy;
            bool vertical = Random.Range(0, 2) == 0;
            List<Vector2Int> cells = GetBossRoomCells(context);
            List<Vector2Int> affectedCells = new List<Vector2Int>();
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];
                bool isOnCutLine = vertical ? cell.x == boss.GridPosition.x : cell.y == boss.GridPosition.y;
                if (isOnCutLine && cell != boss.GridPosition)
                {
                    affectedCells.Add(cell);
                }
            }

            Vector2Int facing = vertical ? Vector2Int.up : Vector2Int.right;
            PrepareBossPattern(boss, EnemyActionType.BossSpaceCut, affectedCells, facing);
        }

        private static void PrepareCloseAttack(EnemyBehaviorContext context)
        {
            ActorEntity boss = context.Enemy;
            List<Vector2Int> affectedCells = new List<Vector2Int>();
            List<Vector2Int> roomCells = GetBossRoomCells(context);
            for (int i = 0; i < roomCells.Count; i++)
            {
                Vector2Int cell = roomCells[i];
                if (IsAdjacentEightWay(boss.GridPosition, cell))
                {
                    affectedCells.Add(cell);
                }
            }

            PrepareBossPattern(
                boss,
                EnemyActionType.BossCloseAttack,
                affectedCells,
                GetFacingToward(boss.GridPosition, context.Player.GridPosition, boss.FacingDirection));
        }

        private static void PrepareCharge(EnemyBehaviorContext context)
        {
            ActorEntity boss = context.Enemy;
            Vector2Int direction = GetFacingToward(boss.GridPosition, context.Player.GridPosition, boss.FacingDirection);
            List<Vector2Int> affectedCells = GetChargePath(boss.GridPosition, direction, context.MapService);
            PrepareBossPattern(boss, EnemyActionType.BossCharge, affectedCells, direction);
        }

        private static void PrepareBossPattern(
            ActorEntity boss,
            EnemyActionType actionType,
            List<Vector2Int> affectedCells,
            Vector2Int facing)
        {
            boss.PendingEnemyAction = actionType;
            boss.PendingEnemyActionTurns = 0;
            boss.PendingEnemyFacingDirection = facing;
            boss.PendingBossAffectedCells.Clear();
            boss.PendingBossAffectedCells.AddRange(affectedCells);
            boss.PendingEnemyTargetCell = affectedCells.Count > 0 ? affectedCells[0] : boss.GridPosition;
            boss.HasPendingEnemyTargetCell = true;
        }

        private static void ExecuteSpaceCut(EnemyBehaviorContext context)
        {
            ActorEntity boss = context.Enemy;
            ClearActiveBossWalls(boss, context.MapService);
            for (int i = 0; i < boss.PendingBossAffectedCells.Count; i++)
            {
                Vector2Int cell = boss.PendingBossAffectedCells[i];
                if (context.MapService.SetRuntimeWall(cell, true))
                {
                    boss.ActiveBossWallCells.Add(cell);
                }
            }

            boss.BossWallTurnsRemaining = boss.ActiveBossWallCells.Count > 0
                ? boss.EnemyDefinition.SpaceCutWallDuration
                : 0;
        }

        private void ExecuteCharge(EnemyBehaviorContext context)
        {
            ActorEntity boss = context.Enemy;
            Vector2Int direction = boss.PendingEnemyFacingDirection;
            List<Vector2Int> currentPath = GetChargePath(boss.GridPosition, direction, context.MapService);
            Vector2Int destination = boss.GridPosition;
            for (int i = 0; i < currentPath.Count; i++)
            {
                Vector2Int cell = currentPath[i];
                if (cell == context.Player.GridPosition)
                {
                    combatSystem.ResolveEnemyAttack(boss, context.Player);
                    break;
                }

                if (context.ActorRepository.IsEnemyOccupied(cell))
                {
                    break;
                }

                destination = cell;
            }

            boss.FacingDirection = direction;
            boss.GridPosition = destination;
            boss.BossStunTurnsRemaining = boss.EnemyDefinition.ChargeStunDuration;
            boss.BossAlignedTurnCount = 0;
        }

        private static void AdvanceBossWalls(ActorEntity boss, MapService mapService)
        {
            if (boss.BossWallTurnsRemaining <= 0)
            {
                return;
            }

            boss.BossWallTurnsRemaining -= 1;
            if (boss.BossWallTurnsRemaining == 0)
            {
                ClearActiveBossWalls(boss, mapService);
            }
        }

        public static void ClearActiveBossWalls(ActorEntity boss, MapService mapService)
        {
            for (int i = 0; i < boss.ActiveBossWallCells.Count; i++)
            {
                mapService.SetRuntimeWall(boss.ActiveBossWallCells[i], false);
            }

            boss.ActiveBossWallCells.Clear();
            boss.BossWallTurnsRemaining = 0;
        }

        private static List<Vector2Int> GetChargePath(Vector2Int origin, Vector2Int direction, MapService mapService)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int cell = origin + direction;
            while (mapService.IsWalkableCell(cell) && !mapService.BlocksAttack(cell))
            {
                path.Add(cell);
                cell += direction;
            }

            return path;
        }

        private static List<Vector2Int> GetBossRoomCells(EnemyBehaviorContext context)
        {
            MapDefinition map = context.MapService.CurrentMap;
            if (map != null)
            {
                for (int i = 0; i < map.Rooms.Count; i++)
                {
                    DungeonRoomDefinition room = map.Rooms[i];
                    if (room != null && room.Cells.Contains(context.Enemy.GridPosition))
                    {
                        return room.Cells;
                    }
                }
            }

            return map != null ? map.WalkableCells : new List<Vector2Int>();
        }

        private static bool IsBossPattern(EnemyActionType actionType)
        {
            return actionType == EnemyActionType.BossSpaceCut ||
                   actionType == EnemyActionType.BossCloseAttack ||
                   actionType == EnemyActionType.BossCharge;
        }

        private static bool IsSameRowOrColumn(Vector2Int first, Vector2Int second)
        {
            return first != second && (first.x == second.x || first.y == second.y);
        }

        private static bool IsAdjacentEightWay(Vector2Int origin, Vector2Int target)
        {
            Vector2Int delta = target - origin;
            return delta != Vector2Int.zero && Mathf.Abs(delta.x) <= 1 && Mathf.Abs(delta.y) <= 1;
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

        private void ExecutePreparedMove(EnemyBehaviorContext context)
        {
            ActorEntity enemy = context.Enemy;
            Vector2Int targetCell = enemy.PendingEnemyTargetCell;
            ActorEntity player = context.Player;
            if (enemy.HasPendingEnemyTargetCell &&
                CanMoveTo(targetCell, context.Player.GridPosition, context.MapService, context.ActorRepository))
            {
                enemy.FacingDirection = GetFacingToward(enemy.GridPosition, targetCell, enemy.FacingDirection);
                if (targetCell == player.GridPosition)
                {
                    // 이동할 칸에 플레이어가 있으면 고정 데미지
                    enemy.HasMoveCollisionFeedback = true;
                    enemy.MoveCollisionTargetCell = targetCell;
                    combatSystem.ApplyFixedDamage(player, EnemyMoveCollisionDamage);
                    ClearPreparation(enemy);
                    return;
                }

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
            return mapService.IsEnemyWalkable(targetCell) &&
                   !actorRepository.IsEnemyOccupied(targetCell);
        }

        private static bool IsInDetectionRange(ActorEntity enemy, Vector2Int playerPosition, MapService mapService)
        {
            int detectionRange = Mathf.Max(0, enemy.Stats.DetectionRange);
            Vector2Int delta = playerPosition - enemy.GridPosition;
            return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) <= detectionRange &&
                   !mapService.BlocksLineOfSightBetween(enemy.GridPosition, playerPosition);
        }

        private static bool TryCompletePreparation(ActorEntity enemy, EnemyActionType actionType, int requiredTurns, Vector2Int targetCell)
        {
            return TryCompletePreparation(enemy, actionType, requiredTurns, targetCell, enemy.FacingDirection);
        }

        private static bool TryCompletePreparation(
            ActorEntity enemy,
            EnemyActionType actionType,
            int requiredTurns,
            Vector2Int targetCell,
            Vector2Int pendingFacingDirection)
        {
            requiredTurns = Mathf.Max(0, requiredTurns);
            if (enemy.PendingEnemyAction != actionType)
            {
                enemy.PendingEnemyAction = actionType;
                enemy.PendingEnemyActionTurns = 0;
                enemy.PendingEnemyTargetCell = targetCell;
                enemy.PendingEnemyFacingDirection = pendingFacingDirection;
                enemy.HasPendingEnemyTargetCell = true;
                return requiredTurns == 0;
            }

            if (!enemy.HasPendingEnemyTargetCell)
            {
                enemy.PendingEnemyTargetCell = targetCell;
                enemy.PendingEnemyFacingDirection = pendingFacingDirection;
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
            enemy.PendingBossAffectedCells.Clear();
        }

        private static bool CanAttack(ActorEntity enemy, Vector2Int target, MapService mapService)
        {
            return TryGetAttackTarget(enemy, target, mapService, out _, out _);
        }

        private static bool TryGetAttackTarget(
            ActorEntity enemy,
            Vector2Int target,
            MapService mapService,
            out Vector2Int targetCell,
            out Vector2Int attackFacing)
        {
            targetCell = target;
            if (mapService.BlocksAttackBetween(enemy.GridPosition, target))
            {
                attackFacing = enemy.FacingDirection;
                return false;
            }

            EnemyAttackPatternDefinition attackPattern = enemy.EnemyDefinition != null
                ? enemy.EnemyDefinition.AttackPattern
                : null;
            if (attackPattern != null)
            {
                return attackPattern.TryGetTargetFacing(enemy.GridPosition, target, out attackFacing);
            }

            Vector2Int delta = target - enemy.GridPosition;
            if (IsStraightLine(delta, 1))
            {
                attackFacing = GetFacingToward(enemy.GridPosition, target, enemy.FacingDirection);
                return true;
            }

            attackFacing = enemy.FacingDirection;
            return false;
        }

        private static bool IsInPreparedAttackRange(ActorEntity enemy, Vector2Int target, MapService mapService)
        {
            if (mapService.BlocksAttackBetween(enemy.GridPosition, target))
            {
                return false;
            }

            EnemyAttackPatternDefinition attackPattern = enemy.EnemyDefinition != null
                ? enemy.EnemyDefinition.AttackPattern
                : null;
            if (attackPattern != null)
            {
                return attackPattern.ContainsTargetAtFacing(enemy.GridPosition, enemy.PendingEnemyFacingDirection, target);
            }

            return target == enemy.PendingEnemyTargetCell;
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
