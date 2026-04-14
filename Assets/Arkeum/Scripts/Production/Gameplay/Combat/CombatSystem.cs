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

        public int ResolvePlayerAttack(RunState runState, ActorEntity attacker, ActorEntity defender)
        {
            int damage = damageResolver.ResolveDamage(attacker.Stats.AttackPower, defender.Stats.Defense);
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
    }
}
