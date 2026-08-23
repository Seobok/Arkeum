using Arkeum.Production.Gameplay.Run;
using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Timing
{
    public sealed class TimingSession
    {
        public TimingSession(IReadOnlyList<WeaponAttackContext> attackContexts, TimingChallengeDefinition definition, ITimingChallengeRuntime runtime)
        {
            AttackContexts = attackContexts;
            AttackContext = attackContexts != null && attackContexts.Count > 0 ? attackContexts[0] : null;
            Definition = definition;
            Runtime = runtime;
            StartDelayRemainingSeconds = definition != null ? definition.StartDelaySeconds : 0f;
            HasStarted = StartDelayRemainingSeconds <= 0f;
        }

        public IReadOnlyList<WeaponAttackContext> AttackContexts { get; }
        public WeaponAttackContext AttackContext { get; }
        public TimingChallengeDefinition Definition { get; }
        public ITimingChallengeRuntime Runtime { get; }
        public float StartDelayRemainingSeconds { get; private set; }
        public bool HasStarted { get; private set; }

        public bool TickStartDelay(float deltaTime)
        {
            if (HasStarted)
            {
                return false;
            }

            StartDelayRemainingSeconds = Mathf.Max(
                0f,
                StartDelayRemainingSeconds - Mathf.Max(0f, deltaTime));
            if (StartDelayRemainingSeconds > 0f)
            {
                return false;
            }

            HasStarted = true;
            return true;
        }

        public TimingAttackResult BuildResult(TimingResultGrade grade)
        {
            return Definition != null ? Definition.BuildResult(grade) : TimingAttackResult.None;
        }
    }
}
