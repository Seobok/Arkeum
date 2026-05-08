namespace Arkeum.Production.Gameplay.Timing
{
    public interface ITimingChallengeRuntime
    {
        float DurationSeconds { get; }
        float ElapsedSeconds { get; }
        float NormalizedPosition { get; }
        float GoodZoneMin { get; }
        float GoodZoneMax { get; }
        float PerfectZoneMin { get; }
        float PerfectZoneMax { get; }
        bool IsExpired { get; }

        void Tick(float deltaTime);
        TimingResultGrade EvaluateAction();
        TimingResultGrade EvaluateTimeout();
    }
}
