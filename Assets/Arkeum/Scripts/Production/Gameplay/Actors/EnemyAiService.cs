using UnityEngine;

namespace Arkeum.Production.Gameplay.Actors
{
    public sealed class EnemyAiService
    {
        public Vector2Int ChooseDirection(ActorEntity enemy, Vector2Int playerPosition)
        {
            Vector2Int delta = playerPosition - enemy.GridPosition;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                return new Vector2Int(delta.x == 0 ? 0 : (delta.x > 0 ? 1 : -1), 0);
            }

            return new Vector2Int(0, delta.y > 0 ? 1 : -1);
        }
    }
}
