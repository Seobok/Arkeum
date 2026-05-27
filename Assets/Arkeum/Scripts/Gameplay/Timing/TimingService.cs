using Arkeum.Production.Gameplay.Run;
using System.Collections.Generic;

namespace Arkeum.Production.Gameplay.Timing
{
    public sealed class TimingService
    {
        public TimingSession CurrentSession { get; private set; }
        public bool HasPendingAttack => CurrentSession != null;

        public bool TryBegin(RunState runState, WeaponAttackContext attackContext, out TimingSession session)
        {
            return TryBegin(
                runState,
                attackContext != null ? new[] { attackContext } : null,
                out session);
        }

        public bool TryBegin(RunState runState, IReadOnlyList<WeaponAttackContext> attackContexts, out TimingSession session)
        {
            session = null;
            WeaponAttackContext attackContext = attackContexts != null && attackContexts.Count > 0
                ? attackContexts[0]
                : null;
            if (runState == null || !runState.IsTimingModeEnabled || attackContext?.Weapon == null)
            {
                return false;
            }

            TimingChallengeDefinition definition = attackContext.Weapon.TimingChallenge;
            if (definition == null)
            {
                return false;
            }

            ITimingChallengeRuntime runtime = definition.CreateRuntime();
            if (runtime == null)
            {
                return false;
            }

            session = new TimingSession(attackContexts, definition, runtime);
            CurrentSession = session;
            return true;
        }

        public void Tick(float deltaTime)
        {
            CurrentSession?.Runtime?.Tick(deltaTime);
        }

        public TimingAttackResult CompleteCurrent(TimingResultGrade grade)
        {
            TimingSession session = CurrentSession;
            CurrentSession = null;
            return session != null ? session.BuildResult(grade) : TimingAttackResult.None;
        }

        public void CancelCurrent()
        {
            CurrentSession = null;
        }
    }
}
