using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Prototype
{
    public sealed class PrototypeCombatSystem
    {
        public void AttackEnemy(
            RunState run,
            ActorRuntime target,
            Action<string> setMessage,
            Action<ActorRuntime> killEnemy,
            Action<string> consumeTurn)
        {
            int damage = Mathf.Max(1, run.EffectiveAttack - target.Defense);
            target.Hp -= damage;
            setMessage($"You strike {target.DisplayName} for {damage} damage.");

            if (target.Hp <= 0)
            {
                killEnemy(target);
            }

            consumeTurn(null);
        }

        public void UpdateEnemies(
            List<ActorRuntime> enemies,
            ActorRuntime player,
            DungeonLayout layout,
            RunState run,
            Action<ActorRuntime> syncActorView,
            Action<string> setMessage)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                ActorRuntime enemy = enemies[i];
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (enemy.ActionInterval > 1 && run.TurnCount % enemy.ActionInterval != 0)
                {
                    continue;
                }

                int distance = Manhattan(enemy.Position, player.Position);
                if (distance == 1)
                {
                    EnemyAttack(enemy, player, run, setMessage);
                    if (run.CurrentHp <= 0)
                    {
                        return;
                    }

                    continue;
                }

                Vector2Int step = GetStepTowards(enemy.Position, player.Position);
                Vector2Int target = enemy.Position + step;
                if (step != Vector2Int.zero && layout.IsWalkable(target) && target != player.Position && !IsEnemyOccupied(enemies, target))
                {
                    enemy.Position = target;
                    syncActorView(enemy);
                }
            }
        }

        public bool TryGetEnemyAt(List<ActorRuntime> enemies, Vector2Int cell, out ActorRuntime enemyAtCell)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                ActorRuntime enemy = enemies[i];
                if (enemy.IsAlive && enemy.Position == cell)
                {
                    enemyAtCell = enemy;
                    return true;
                }
            }

            enemyAtCell = null;
            return false;
        }

        private void EnemyAttack(ActorRuntime enemy, ActorRuntime player, RunState run, Action<string> setMessage)
        {
            int damage = Mathf.Max(1, enemy.Attack - 1);
            run.CurrentHp = Mathf.Max(0, run.CurrentHp - damage);
            player.Hp = run.CurrentHp;
            setMessage($"{enemy.DisplayName} hits you for {damage} damage.");
        }

        private bool IsEnemyOccupied(List<ActorRuntime> enemies, Vector2Int cell)
        {
            return TryGetEnemyAt(enemies, cell, out _);
        }

        private Vector2Int GetStepTowards(Vector2Int from, Vector2Int to)
        {
            Vector2Int delta = to - from;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return new Vector2Int(delta.x > 0 ? 1 : -1, 0);
            }

            if (delta.y != 0)
            {
                return new Vector2Int(0, delta.y > 0 ? 1 : -1);
            }

            if (delta.x != 0)
            {
                return new Vector2Int(delta.x > 0 ? 1 : -1, 0);
            }

            return Vector2Int.zero;
        }

        private int Manhattan(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
