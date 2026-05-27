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

        // 플레이어 공격력을 계산하는 함수
        public static int CalculatePlayerAttack(RunState runState)
        {
            // 기본 공격력
            int attackPower = runState.Player.Stats.AttackPower;
            if (runState == null)
            {
                return attackPower;
            }

            // 무기 공격력
            if (runState.HasEquippedWeapon)
            {
                attackPower += runState.EquippedWeapon != null ? runState.EquippedWeapon.AttackBonus : 1;
            }

            return attackPower;
        }
    }
}
