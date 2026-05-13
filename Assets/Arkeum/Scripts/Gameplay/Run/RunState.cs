using System;
using Arkeum.Production.Gameplay.Actors;

namespace Arkeum.Production.Gameplay.Run
{
    [Serializable]
    public sealed class RunState
    {
        public int RunIndex;
        public int CurrentFloor;
        public RunFloorDefinition CurrentFloorDefinition;
        public int TurnCount;
        public int BandageCount;
        public int AttackBonus;
        public bool FloorExitUsed;
        public bool HasEquippedWeapon;
        public WeaponDefinition EquippedWeapon;
        public bool IsTimingModeEnabled;
        public RunEndReason EndReason;
        public ActorEntity Player;

        public int EffectiveAttack => 3 + AttackBonus + (HasEquippedWeapon ? EquippedWeaponAttackBonus : 0);

        private int EquippedWeaponAttackBonus => EquippedWeapon != null ? EquippedWeapon.AttackBonus : 1;
    }
}
