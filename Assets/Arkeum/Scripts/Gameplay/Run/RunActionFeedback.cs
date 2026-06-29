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
    }
}
