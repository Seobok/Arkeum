using UnityEngine;

namespace Arkeum.Production.Gameplay.Combat
{
    public sealed class DamageResolver
    {
        public int ResolveDamage(int attackPower, int defense)
        {
            return Mathf.Max(1, attackPower - defense);
        }
    }
}
