using UnityEngine;

namespace Arkeum.Prototype
{
    public sealed partial class PrototypeGameController
    {
        private void AttackEnemy(ActorRuntime target)
        {
            combatSystem.AttackEnemy(Run, target, SetMessage, KillEnemy, ConsumeTurn);
        }

        private void UpdateEnemies()
        {
            combatSystem.UpdateEnemies(enemies, player, layout, Run, SyncActorView, SetMessage);
        }

        private void KillEnemy(ActorRuntime enemy)
        {
            enemy.IsAlive = false;
            if (enemy.View != null)
            {
                enemy.View.SetActive(false);
            }

            Run.Hyeolpyeon += enemy.BloodReward;
            SetMessage($"{enemy.DisplayName} falls. You gain {enemy.BloodReward} blood shards.");
        }

        private bool TryGetEnemyAt(Vector2Int cell, out ActorRuntime enemyAtCell)
        {
            return combatSystem.TryGetEnemyAt(enemies, cell, out enemyAtCell);
        }
    }
}
