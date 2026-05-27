using Arkeum.Production.Gameplay.Actors;

namespace Arkeum.Production.Gameplay.Run
{
    public static class RunStatCalculator
    {
        public const int BasePlayerAttackPower = 3;
        public const int BasePlayerDefense = 1;

        public static ActorStats CreatePlayerStats()
        {
            return new ActorStats
            {
                AttackPower = BasePlayerAttackPower,
                Defense = BasePlayerDefense,
            };
        }

        public static int CalculatePlayerAttack(RunState runState)
        {
            int attackPower = BasePlayerAttackPower;
            if (runState == null)
            {
                return attackPower;
            }

            attackPower += runState.AttackBonus;
            if (runState.HasEquippedWeapon)
            {
                attackPower += runState.EquippedWeapon != null ? runState.EquippedWeapon.AttackBonus : 1;
            }

            return attackPower;
        }
    }
}
