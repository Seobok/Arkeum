using System.Collections.Generic;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Run;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Combat
{
    public sealed class EnemyTurnSystem
    {
        private readonly CombatSystem combatSystem;
        private readonly TargetingService targetingService;

        public EnemyTurnSystem(CombatSystem combatSystem, TargetingService targetingService)
        {
            this.combatSystem = combatSystem;
            this.targetingService = targetingService;
        }

        public void ResolveEnemyTurn(RunState runState, IReadOnlyList<ActorEntity> enemies, MapService mapService, ActorRepository actorRepository)
        {
            // Player Validation
            if (runState?.Player == null)
            {
                return;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                ActorEntity enemy = enemies[i];
                // Enemy Validation
                if (!enemy.IsAlive)
                {
                    continue;
                }

                // Enemy는 자기 행동 주기에 맞는 턴에만 행동 가능
                if (enemy.Stats.ActionInterval > 1 && runState.TurnCount % enemy.Stats.ActionInterval != 0)
                {
                    continue;
                }

                enemy.FacingDirection = GetFacingToward(enemy.GridPosition, runState.Player.GridPosition, enemy.FacingDirection);
                if (CanAttack(enemy, runState.Player.GridPosition))
                {
                    combatSystem.ResolveEnemyAttack(enemy, runState.Player);
                    if (runState.Player.CurrentHp <= 0)
                    {
                        return;
                    }

                    continue;
                }

                Vector2Int step = targetingService.GetNextStep(enemy.GridPosition, runState.Player.GridPosition);
                Vector2Int targetCell = enemy.GridPosition + step;
                if (step != Vector2Int.zero &&
                    mapService.IsWalkableCell(targetCell) &&
                    targetCell != runState.Player.GridPosition &&
                    !actorRepository.IsEnemyOccupied(targetCell))
                {
                    enemy.GridPosition = targetCell;
                    enemy.FacingDirection = step;
                }
            }
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
