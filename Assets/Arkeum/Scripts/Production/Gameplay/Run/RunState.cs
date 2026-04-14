using System;
using Arkeum.Production.Gameplay.Actors;

namespace Arkeum.Production.Gameplay.Run
{
    [Serializable]
    public sealed class RunState
    {
        public int RunIndex;
        public int TurnCount;
        public int DepthReached;
        public int CurrentHp;
        public int MaxHp;
        public int BloodShards;
        public int BandageCount;
        public int DraughtCount;
        public int AttackBonus;
        public int GleamReward;
        public bool TemporaryWeaponEquipped;
        public bool ReliquaryClaimed;
        public bool TemporaryWeaponCollected;
        public int DraughtStock;
        public RunEndReason EndReason;
        public ActorEntity Player;

        public int EffectiveAttack => 3 + AttackBonus;
    }
}
