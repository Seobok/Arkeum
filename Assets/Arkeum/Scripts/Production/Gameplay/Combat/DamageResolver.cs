using UnityEngine;

namespace Arkeum.Production.Gameplay.Combat
{
    // Damage 계산을 담당하는 클래스
    // 현재는 최소 1데미지를 보장하는 구조
    public sealed class DamageResolver
    {
        public int ResolveDamage(int attackPower, int defense)
        {
            return Mathf.Max(1, attackPower - defense);
        }
    }
}
