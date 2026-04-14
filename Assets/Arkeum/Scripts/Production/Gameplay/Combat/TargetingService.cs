using UnityEngine;

namespace Arkeum.Production.Gameplay.Combat
{
    public sealed class TargetingService
    {
        public Vector2Int GetNextStep(Vector2Int from, Vector2Int to)
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

            return Vector2Int.zero;
        }
    }
}
