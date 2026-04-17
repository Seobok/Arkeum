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

                // Enemy와 Player가 붙어 있다면 공격 시도
                // TODO :: 추후 Enemy의 공격범위가 한칸이 아니게 되면 수정 필요
                if (Manhattan(enemy.GridPosition, runState.Player.GridPosition) == 1)
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
                }
            }
        }

        private int Manhattan(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
