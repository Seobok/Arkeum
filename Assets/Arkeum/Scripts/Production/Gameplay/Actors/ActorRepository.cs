using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Actors
{
    public sealed class ActorRepository
    {
        private readonly List<ActorEntity> actors = new List<ActorEntity>();

        public IReadOnlyList<ActorEntity> Actors => actors;
        public ActorEntity Player { get; private set; }

        public void SetActors(IEnumerable<ActorEntity> source)
        {
            actors.Clear();
            Player = null;
            if (source == null)
            {
                return;
            }

            foreach (ActorEntity actor in source)
            {
                actors.Add(actor);
                if (actor != null && actor.IsPlayer)
                {
                    Player = actor;
                }
            }
        }

        public bool TryGetEnemyAt(Vector2Int cell, out ActorEntity enemy)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                ActorEntity actor = actors[i];
                if (actor.IsEnemy && actor.IsAlive && actor.GridPosition == cell)
                {
                    enemy = actor;
                    return true;
                }
            }

            enemy = null;
            return false;
        }

        public bool IsOccupied(Vector2Int cell)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                ActorEntity actor = actors[i];
                if (actor != null && actor.IsAlive && actor.GridPosition == cell)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsEnemyOccupied(Vector2Int cell)
        {
            return TryGetEnemyAt(cell, out _);
        }

        public IReadOnlyList<ActorEntity> GetAliveEnemies()
        {
            List<ActorEntity> aliveEnemies = new List<ActorEntity>();
            for (int i = 0; i < actors.Count; i++)
            {
                ActorEntity actor = actors[i];
                if (actor != null && actor.IsEnemy && actor.IsAlive)
                {
                    aliveEnemies.Add(actor);
                }
            }

            return aliveEnemies;
        }
    }
}
