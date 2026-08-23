using System;

namespace Arkeum.Production.Gameplay.Run
{
    [Flags]
    public enum RunActionFeedback
    {
        None = 0,
        PlayerMoved = 1 << 0,
        PlayerAttacked = 1 << 1,
        PlayerTeleported = 1 << 2,
        EnemyDamaged = 1 << 3,
        EnemyDefeated = 1 << 4,
        WeaponPickedUp = 1 << 5,
        WeaponDropped = 1 << 6,
        ShopPurchased = 1 << 7,
        ActionDenied = 1 << 8,
        BossEncountered = 1 << 9,
        BossRoomOpened = 1 << 10,
        FloorCleared = 1 << 11,
    }
}
