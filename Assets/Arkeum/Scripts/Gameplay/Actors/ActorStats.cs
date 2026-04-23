using System;

namespace Arkeum.Production.Gameplay.Actors
{
    [Serializable]
    public sealed class ActorStats
    {
        public int MaxHp = 1;
        public int AttackPower = 1;
        public int Defense;
        public int ActionInterval = 1;      // Legacy action cadence. Prefer preparation turns for new enemy behavior.
        public int DetectionRange = 6;
        public int MovementRange = 1;
        public int AttackPreparationTurns;
        public int MovePreparationTurns;

        public ActorStats Clone()
        {
            return new ActorStats
            {
                MaxHp = MaxHp,
                AttackPower = AttackPower,
                Defense = Defense,
                ActionInterval = ActionInterval,
                DetectionRange = DetectionRange,
                MovementRange = MovementRange,
                AttackPreparationTurns = AttackPreparationTurns,
                MovePreparationTurns = MovePreparationTurns,
            };
        }
    }
}
