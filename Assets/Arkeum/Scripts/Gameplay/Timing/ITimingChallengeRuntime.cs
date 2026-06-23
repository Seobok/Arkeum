namespace Arkeum.Production.Gameplay.Timing
{
    public interface ITimingChallengeRuntime
    {
        float DurationSeconds { get; }
        float ElapsedSeconds { get; }
        float NormalizedPosition { get; }
        float SuccessZoneMin { get; }
        float SuccessZoneMax { get; }
        bool IsExpired { get; }

        void Tick(float deltaTime);
        TimingResultGrade EvaluateAction();
        TimingResultGrade EvaluateTimeout();
    }
}
