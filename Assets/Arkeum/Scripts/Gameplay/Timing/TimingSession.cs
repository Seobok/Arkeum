using Arkeum.Production.Gameplay.Run;
using System.Collections.Generic;

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
        }

        public IReadOnlyList<WeaponAttackContext> AttackContexts { get; }
        public WeaponAttackContext AttackContext { get; }
        public TimingChallengeDefinition Definition { get; }
        public ITimingChallengeRuntime Runtime { get; }

        public TimingAttackResult BuildResult(TimingResultGrade grade)
        {
            return Definition != null ? Definition.BuildResult(grade) : TimingAttackResult.None;
        }
    }
}
