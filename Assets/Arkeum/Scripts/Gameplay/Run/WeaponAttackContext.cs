using Arkeum.Production.Gameplay.Actors;
using Arkeum.Production.Gameplay.Timing;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    public sealed class WeaponAttackContext
    {
        public RunState RunState;
        public ActorEntity Attacker;
        public ActorEntity Defender;
        public WeaponDefinition Weapon;
        public Vector2Int FacingDirection;
        public Vector2Int WeaponOffset;
        public int AttackPower;
        public TimingAttackResult TimingResult;
    }
}
