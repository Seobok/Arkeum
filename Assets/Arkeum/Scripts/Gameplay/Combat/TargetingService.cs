using UnityEngine;

namespace Arkeum.Production.Gameplay.Combat
{
    public sealed class TargetingService
    {
        // TODO ::
        // 구현상 무조건 x먼저 이동후 y를 이동하도록 설계되어 있음.
        // 이는 모든 Enemey가 똑같이 이동한다면 이상해 보일 수 있음.
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
