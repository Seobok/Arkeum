using System.Collections.Generic;
using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Map;
using Arkeum.Production.Gameplay.Run;

namespace Arkeum.Production.Gameplay.Combat
{
    public sealed class EnemyTurnSystem
    {
        private readonly EnemyBehaviorActions behaviorActions;
        private readonly IBehaviorTreeNode defaultBehaviorTree;
        private readonly IBehaviorTreeNode bossBehaviorTree;

        public EnemyTurnSystem(CombatSystem combatSystem, TargetingService targetingService)
        {
            behaviorActions = new EnemyBehaviorActions(combatSystem, targetingService);
            EnemyBehaviorTreeFactory behaviorTreeFactory = new EnemyBehaviorTreeFactory();
            defaultBehaviorTree = behaviorTreeFactory.CreateDefaultTree();
            bossBehaviorTree = behaviorTreeFactory.CreateBossTree();
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
                if (enemy == null)
                {
                    continue;
                }

                enemy.HasMoveCollisionFeedback = false;
                enemy.MoveCollisionTargetCell = enemy.GridPosition;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                ActorEntity enemy = enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                bool isBoss = enemy.EnemyDefinition != null && enemy.EnemyDefinition.IsBoss;
                if (isBoss && !runState.BossRoomEntered)
                {
                    continue;
                }

                EnemyBehaviorContext context = new EnemyBehaviorContext(
                    enemy,
                    runState.Player,
                    mapService,
                    actorRepository,
                    behaviorActions);
                IBehaviorTreeNode behaviorTree = isBoss
                    ? bossBehaviorTree
                    : defaultBehaviorTree;
                behaviorTree.Tick(context);
                if (runState.Player.CurrentHp <= 0)
                {
                    return;
                }
            }
        }
    }
}
