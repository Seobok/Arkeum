using System;

namespace Arkeum.Production.Gameplay.Actors
{
    [Serializable]
    public sealed class ActorStats
    {
        public int MaxHp = 1;
        public int AttackPower = 1;
        public int Defense;
        public int ActionInterval = 1;      // 행동 주기
    }
}
