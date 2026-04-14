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
            if (runState?.Player == null)
            {
                return;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                ActorEntity enemy = enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (enemy.Stats.ActionInterval > 1 && runState.TurnCount % enemy.Stats.ActionInterval != 0)
                {
                    continue;
                }

                if (Manhattan(enemy.GridPosition, runState.Player.GridPosition) == 1)
                {
                    combatSystem.ResolveEnemyAttack(enemy, runState.Player);
                    runState.CurrentHp = runState.Player.CurrentHp;
                    if (runState.CurrentHp <= 0)
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
