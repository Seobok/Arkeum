using Arkeum.Production.Gameplay.Run;

namespace Arkeum.Production.Gameplay.Timing
{
    public sealed class TimingSession
    {
        public TimingSession(WeaponAttackContext attackContext, TimingChallengeDefinition definition, ITimingChallengeRuntime runtime)
        {
            AttackContext = attackContext;
            Definition = definition;
            Runtime = runtime;
        }

        public WeaponAttackContext AttackContext { get; }
        public TimingChallengeDefinition Definition { get; }
        public ITimingChallengeRuntime Runtime { get; }

        public TimingAttackResult BuildResult(TimingResultGrade grade)
        {
            return Definition != null ? Definition.BuildResult(grade) : TimingAttackResult.None;
        }
    }
}
