using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arkeum.Production.Gameplay.Actors
{
    [Serializable]
    public sealed class ActorStats
    {
        [FormerlySerializedAs("MaxHp")]
        [SerializeField] private int maxHp = 1;

        public int MaxHp => maxHp;
        public int AttackPower = 1;
        public int Defense = 0;
        public int DetectionRange = 6;
        public int MovementRange = 1;
        public int AttackPreparationTurns;
        public int MovePreparationTurns;

        public void SetMaxHp(int value)
        {
            maxHp = Math.Max(1, value);
        }

        public ActorStats Clone()
        {
            ActorStats clone = new ActorStats
            {
                AttackPower = AttackPower,
                Defense = Defense,
                DetectionRange = DetectionRange,
                MovementRange = MovementRange,
                AttackPreparationTurns = AttackPreparationTurns,
                MovePreparationTurns = MovePreparationTurns,
            };

            clone.SetMaxHp(MaxHp);
            return clone;
        }
    }
}
