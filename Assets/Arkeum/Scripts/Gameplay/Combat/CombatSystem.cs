using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Run;

namespace Arkeum.Production.Gameplay.Combat
{
    public sealed class CombatSystem
    {
        private readonly DamageResolver damageResolver;

        public CombatSystem(DamageResolver damageResolver)
        {
            this.damageResolver = damageResolver;
        }

        public int ResolvePlayerAttack(RunState runState, ActorEntity attacker, ActorEntity defender, WeaponAttackContext attackContext)
        {
            int attackPower = attackContext != null ? attackContext.AttackPower : attacker.Stats.AttackPower;
            ApplyWeaponEffects(attackContext);
            if (attackContext != null)
            {
                attackPower = attackContext.AttackPower;
            }

            int damage = damageResolver.ResolveDamage(attackPower, defender.Stats.Defense);
            ApplyDamage(defender, damage);
            return damage;
        }

        public int ResolveEnemyAttack(ActorEntity attacker, ActorEntity defender)
        {
            int damage = damageResolver.ResolveDamage(attacker.Stats.AttackPower, defender.Stats.Defense);
            ApplyDamage(defender, damage);
            return damage;
        }

        private void ApplyDamage(ActorEntity target, int damage)
        {
            target.CurrentHp -= damage;
            if (target.CurrentHp < 0)
            {
                target.CurrentHp = 0;
            }
        }

        private static void ApplyWeaponEffects(WeaponAttackContext context)
        {
            if (context?.Weapon?.Effects == null)
            {
                return;
            }

            for (int i = 0; i < context.Weapon.Effects.Count; i++)
            {
                WeaponEffectDefinition effect = context.Weapon.Effects[i];
                if (effect == null)
                {
                    continue;
                }

                effect.ModifyPlayerAttack(context);
            }
        }
    }
}
