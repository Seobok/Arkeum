using UnityEngine;

namespace Arkeum.Production.Gameplay.Timing
{
    [CreateAssetMenu(fileName = "SinglePressTimingChallenge", menuName = "Arkeum/Timing/Single Press Challenge")]
    public sealed class SinglePressTimingChallengeDefinition : TimingChallengeDefinition
    {
        [SerializeField, Range(0.01f, 1f)] private float successZoneLength = 0.1f;
        [SerializeField, Range(0f, 1f)] private float successZoneSpawnRangeMin = 0.25f;
        [SerializeField, Range(0f, 1f)] private float successZoneSpawnRangeMax = 0.75f;

        public override ITimingChallengeRuntime CreateRuntime()
        {
            CalculateRandomZone(
                successZoneLength,
                successZoneSpawnRangeMin,
                successZoneSpawnRangeMax,
                out float normalizedSuccessMin,
                out float normalizedSuccessMax);

            return new SinglePressTimingChallengeRuntime(
                DurationSeconds,
                LateInputGraceSeconds,
                normalizedSuccessMin,
                normalizedSuccessMax);
        }

        private static void CalculateRandomZone(float zoneLength, float spawnRangeMin, float spawnRangeMax, out float zoneMin, out float zoneMax)
        {
            float rangeMin = Mathf.Clamp01(Mathf.Min(spawnRangeMin, spawnRangeMax));
            float rangeMax = Mathf.Clamp01(Mathf.Max(spawnRangeMin, spawnRangeMax));
            float rangeLength = Mathf.Max(0f, rangeMax - rangeMin);
            float clampedZoneLength = Mathf.Clamp(zoneLength, 0.01f, Mathf.Max(0.01f, rangeLength));
            float latestStart = Mathf.Max(rangeMin, rangeMax - clampedZoneLength);
            zoneMin = Mathf.Approximately(latestStart, rangeMin)
                ? rangeMin
                : Random.Range(rangeMin, latestStart);
            zoneMax = Mathf.Min(rangeMax, zoneMin + clampedZoneLength);
        }

        private sealed class SinglePressTimingChallengeRuntime : ITimingChallengeRuntime
        {
            public SinglePressTimingChallengeRuntime(
                float durationSeconds,
                float lateInputGraceSeconds,
                float successZoneMin,
                float successZoneMax)
            {
                DurationSeconds = durationSeconds;
                LateInputGraceSeconds = Mathf.Max(0f, lateInputGraceSeconds);
                SuccessZoneMin = successZoneMin;
                SuccessZoneMax = successZoneMax;
            }

            public float DurationSeconds { get; }
            private float LateInputGraceSeconds { get; }
            public float ElapsedSeconds { get; private set; }
            public float NormalizedPosition => Mathf.Clamp01(ElapsedSeconds / DurationSeconds);
            public float SuccessZoneMin { get; }
            public float SuccessZoneMax { get; }
            public bool IsExpired => ElapsedSeconds >= DurationSeconds;

            public void Tick(float deltaTime)
            {
                ElapsedSeconds = Mathf.Min(DurationSeconds, ElapsedSeconds + Mathf.Max(0f, deltaTime));
            }

            public TimingResultGrade EvaluateAction()
            {
                float currentPosition = NormalizedPosition;
                float graceStartPosition = Mathf.Clamp01(
                    (ElapsedSeconds - LateInputGraceSeconds) / DurationSeconds);
                bool graceWindowOverlapsSuccessZone =
                    currentPosition >= SuccessZoneMin &&
                    graceStartPosition <= SuccessZoneMax;
                if (graceWindowOverlapsSuccessZone)
                {
                    return TimingResultGrade.Success;
                }

                return TimingResultGrade.Failed;
            }

            public TimingResultGrade EvaluateTimeout()
            {
                return TimingResultGrade.Failed;
            }
        }
    }
}
