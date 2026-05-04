using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    [CreateAssetMenu(fileName = "OffsetDamageModifierEffect", menuName = "Arkeum/Weapon Effects/Offset Damage Modifier")]
    public sealed class OffsetDamageModifierWeaponEffect : WeaponEffectDefinition
    {
        [SerializeField] private Vector2Int offset = Vector2Int.right;
        [SerializeField] private int attackPowerBonus = -1;

        public Vector2Int Offset => offset;
        public int AttackPowerBonus => attackPowerBonus;

        public override void ModifyPlayerAttack(WeaponAttackContext context)
        {
            if (context == null || context.WeaponOffset != offset)
            {
                return;
            }

            context.AttackPower += attackPowerBonus;
        }
    }
}
