using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Run;
using Arkeum.Production.Gameplay.Timing;
using UnityEngine;

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
            int attackPower = attackContext != null
                ? attackContext.AttackPower
                : RunStatCalculator.CalculatePlayerAttack(runState);

            // 무기의 특수효과 적용
            ApplyWeaponEffects(attackContext);
            if (attackContext != null)
            {
                attackPower = attackContext.AttackPower;
            }

            // 타이밍 효과 적용
            attackPower = ApplyTimingResult(attackContext, attackPower);

            // 데미지 적용
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

        public int ApplyFixedDamage(ActorEntity target, int damage)
        {
            int fixedDamage = Mathf.Max(0, damage);
            ApplyDamage(target, fixedDamage);
            return fixedDamage;
        }

        // 최종 데미지 적용
        private void ApplyDamage(ActorEntity target, int damage)
        {
            target.SetCurrentHp(target.CurrentHp - damage);
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

        private static int ApplyTimingResult(WeaponAttackContext context, int attackPower)
        {
            if (context == null || !context.TimingResult.Attempted)
            {
                return attackPower;
            }

            TimingAttackResult timingResult = context.TimingResult;
            float multipliedAttackPower = attackPower * timingResult.DamageMultiplier;
            return Mathf.Max(0, Mathf.RoundToInt(multipliedAttackPower) + timingResult.FlatDamageBonus);
        }
    }
}
