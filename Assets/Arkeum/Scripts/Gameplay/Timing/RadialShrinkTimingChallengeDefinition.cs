using UnityEngine;

namespace Arkeum.Production.Gameplay.Timing
{
    public interface IRadialShrinkTimingChallengeRuntime : ITimingChallengeRuntime
    {
        float MarkerRadiusNormalized { get; }
        float SuccessInnerRadiusNormalized { get; }
        float SuccessOuterRadiusNormalized { get; }
    }

    [CreateAssetMenu(fileName = "RadialShrinkTimingChallenge", menuName = "Arkeum/Timing/Radial Shrink Challenge")]
    public sealed class RadialShrinkTimingChallengeDefinition : TimingChallengeDefinition
    {
        [SerializeField, Range(0f, 2f)] private float markerStartRadiusNormalized = 1.2f;
        [SerializeField, Range(0f, 2f)] private float markerEndRadiusNormalized;
        [SerializeField, Range(0.01f, 1f)] private float successZoneLengthNormalized = 0.14f;
        [SerializeField, Range(0f, 1f)] private float successZoneSpawnRangeMinNormalized = 0.35f;
        [SerializeField, Range(0f, 1f)] private float successZoneSpawnRangeMaxNormalized = 0.75f;

        public override ITimingChallengeRuntime CreateRuntime()
        {
            CalculateRandomZone(
                successZoneLengthNormalized,
                successZoneSpawnRangeMinNormalized,
                successZoneSpawnRangeMaxNormalized,
                out float successInner,
                out float successOuter);
            float startRadius = Mathf.Max(0f, markerStartRadiusNormalized);
            float endRadius = Mathf.Max(0f, markerEndRadiusNormalized);

            return new RadialShrinkTimingChallengeRuntime(
                DurationSeconds,
                LateInputGraceSeconds,
                startRadius,
                endRadius,
                successInner,
                successOuter);
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

        private sealed class RadialShrinkTimingChallengeRuntime : IRadialShrinkTimingChallengeRuntime
        {
            private readonly float markerStartRadiusNormalized;
            private readonly float markerEndRadiusNormalized;

            public RadialShrinkTimingChallengeRuntime(
                float durationSeconds,
                float lateInputGraceSeconds,
                float markerStartRadiusNormalized,
                float markerEndRadiusNormalized,
                float successInnerRadiusNormalized,
                float successOuterRadiusNormalized)
            {
                DurationSeconds = durationSeconds;
                LateInputGraceSeconds = Mathf.Max(0f, lateInputGraceSeconds);
                this.markerStartRadiusNormalized = markerStartRadiusNormalized;
                this.markerEndRadiusNormalized = markerEndRadiusNormalized;
                SuccessInnerRadiusNormalized = successInnerRadiusNormalized;
                SuccessOuterRadiusNormalized = successOuterRadiusNormalized;
            }

            public float DurationSeconds { get; }
            private float LateInputGraceSeconds { get; }
            public float ElapsedSeconds { get; private set; }
            public float NormalizedPosition => Mathf.Clamp01(ElapsedSeconds / DurationSeconds);
            public float SuccessZoneMin => SuccessInnerRadiusNormalized;
            public float SuccessZoneMax => SuccessOuterRadiusNormalized;
            public float SuccessInnerRadiusNormalized { get; }
            public float SuccessOuterRadiusNormalized { get; }
            public float MarkerRadiusNormalized => Mathf.Lerp(markerStartRadiusNormalized, markerEndRadiusNormalized, NormalizedPosition);
            public bool IsExpired => ElapsedSeconds >= DurationSeconds;

            public void Tick(float deltaTime)
            {
                ElapsedSeconds = Mathf.Min(DurationSeconds, ElapsedSeconds + Mathf.Max(0f, deltaTime));
            }

            public TimingResultGrade EvaluateAction()
            {
                float currentRadius = MarkerRadiusNormalized;
                float graceStartPosition = Mathf.Clamp01(
                    (ElapsedSeconds - LateInputGraceSeconds) / DurationSeconds);
                float graceStartRadius = Mathf.Lerp(
                    markerStartRadiusNormalized,
                    markerEndRadiusNormalized,
                    graceStartPosition);
                float graceWindowMinRadius = Mathf.Min(currentRadius, graceStartRadius);
                float graceWindowMaxRadius = Mathf.Max(currentRadius, graceStartRadius);
                bool graceWindowOverlapsSuccessZone =
                    graceWindowMaxRadius >= SuccessInnerRadiusNormalized &&
                    graceWindowMinRadius <= SuccessOuterRadiusNormalized;
                return graceWindowOverlapsSuccessZone
                    ? TimingResultGrade.Success
                    : TimingResultGrade.Failed;
            }

            public TimingResultGrade EvaluateTimeout()
            {
                return TimingResultGrade.Failed;
            }
        }
    }
}
