using System;
using Arkeum.Production.Gameplay.Actors;

namespace Arkeum.Production.Gameplay.Run
{
    [Serializable]
    public sealed class RunState
    {
        public int RunIndex;
        public int CurrentFloor;
        public int PlayerTurnCount;
        public RunFloorDefinition CurrentFloorDefinition;
        public bool FloorExitUsed;
        public bool BossRoomEntered;
        public bool BossRoomCleared;
        public bool HasEquippedWeapon;
        public WeaponDefinition EquippedWeapon;
        public bool IsTimingModeEnabled;
        public RunEndReason EndReason;
        public ActorEntity Player;
    }
}
