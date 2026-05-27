using Arkeum.Production.Gameplay.Actors;

namespace Arkeum.Production.Gameplay.Run
{
    public static class RunStatCalculator
    {
        public const int BasePlayerAttackPower = 1;
        public const int BasePlayerDefense = 0;

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

            if (runState.HasEquippedWeapon)
            {
                attackPower += runState.EquippedWeapon != null ? runState.EquippedWeapon.AttackBonus : 1;
            }

            return attackPower;
        }
    }
}
