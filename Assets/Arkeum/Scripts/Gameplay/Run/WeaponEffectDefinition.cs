using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    public abstract class WeaponEffectDefinition : ScriptableObject
    {
        public virtual void ModifyPlayerAttack(WeaponAttackContext context)
        {
        }
    }
}
