using UnityEngine;

namespace Arkeum.Production.Gameplay.Timing
{
    public interface IClockHandTimingChallengeRuntime : ITimingChallengeRuntime
    {
        float HandAngleDegrees { get; }
        float SuccessCenterAngleDegrees { get; }
        float SuccessSweepAngleDegrees { get; }
        float SuccessInnerRadiusNormalized { get; }
        float SuccessOuterRadiusNormalized { get; }
    }

    [CreateAssetMenu(fileName = "ClockHandTimingChallenge", menuName = "Arkeum/Timing/Clock Hand Challenge")]
    public sealed class ClockHandTimingChallengeDefinition : TimingChallengeDefinition
    {
        [SerializeField, Range(0f, 360f)] private float startAngleDegrees = 90f;
        [SerializeField] private bool clockwise = true;
        [SerializeField, Range(0.25f, 3f)] private float rotations = 1f;
        [SerializeField, Range(1f, 360f)] private float successZoneLengthDegrees = 45f;
        [SerializeField, Range(0f, 360f)] private float successZoneSpawnRangeMinDegrees;
        [SerializeField, Range(0f, 360f)] private float successZoneSpawnRangeMaxDegrees = 360f;
        [SerializeField, Range(0f, 1f)] private float successInnerRadiusNormalized;
        [SerializeField, Range(0f, 1f)] private float successOuterRadiusNormalized = 1f;

        public override ITimingChallengeRuntime CreateRuntime()
        {
            float innerRadius = Mathf.Clamp01(Mathf.Min(successInnerRadiusNormalized, successOuterRadiusNormalized));
            float outerRadius = Mathf.Clamp01(Mathf.Max(successInnerRadiusNormalized, successOuterRadiusNormalized));
            CalculateRandomZone(
                successZoneLengthDegrees,
                successZoneSpawnRangeMinDegrees,
                successZoneSpawnRangeMaxDegrees,
                out float successStartAngleDegrees,
                out float successEndAngleDegrees);
            float successSweepAngleDegrees = Mathf.Max(1f, successEndAngleDegrees - successStartAngleDegrees);
            float successCenterAngleDegrees = successStartAngleDegrees + successSweepAngleDegrees * 0.5f;

            return new ClockHandTimingChallengeRuntime(
                DurationSeconds,
                startAngleDegrees,
                clockwise,
                rotations,
                successCenterAngleDegrees,
                successSweepAngleDegrees,
                innerRadius,
                outerRadius);
        }

        private static void CalculateRandomZone(float zoneLength, float spawnRangeMin, float spawnRangeMax, out float zoneMin, out float zoneMax)
        {
            float rangeMin = Mathf.Clamp(spawnRangeMin, 0f, 360f);
            float rangeMax = Mathf.Clamp(spawnRangeMax, 0f, 360f);
            if (rangeMax < rangeMin)
            {
                float temp = rangeMin;
                rangeMin = rangeMax;
                rangeMax = temp;
            }

            float rangeLength = Mathf.Max(0f, rangeMax - rangeMin);
            float clampedZoneLength = Mathf.Clamp(zoneLength, 1f, Mathf.Max(1f, rangeLength));
            float latestStart = Mathf.Max(rangeMin, rangeMax - clampedZoneLength);
            zoneMin = Mathf.Approximately(latestStart, rangeMin)
                ? rangeMin
                : Random.Range(rangeMin, latestStart);
            zoneMax = Mathf.Min(rangeMax, zoneMin + clampedZoneLength);
        }

        private sealed class ClockHandTimingChallengeRuntime : IClockHandTimingChallengeRuntime
        {
            private readonly float startAngleDegrees;
            private readonly bool clockwise;
            private readonly float rotations;

            public ClockHandTimingChallengeRuntime(
                float durationSeconds,
                float startAngleDegrees,
                bool clockwise,
                float rotations,
                float successCenterAngleDegrees,
                float successSweepAngleDegrees,
                float successInnerRadiusNormalized,
                float successOuterRadiusNormalized)
            {
                DurationSeconds = durationSeconds;
                this.startAngleDegrees = Mathf.Repeat(startAngleDegrees, 360f);
                this.clockwise = clockwise;
                this.rotations = Mathf.Max(0f, rotations);
                SuccessCenterAngleDegrees = Mathf.Repeat(successCenterAngleDegrees, 360f);
                SuccessSweepAngleDegrees = Mathf.Clamp(successSweepAngleDegrees, 1f, 180f);
                SuccessInnerRadiusNormalized = successInnerRadiusNormalized;
                SuccessOuterRadiusNormalized = successOuterRadiusNormalized;
            }

            public float DurationSeconds { get; }
            public float ElapsedSeconds { get; private set; }
            public float NormalizedPosition => Mathf.Clamp01(ElapsedSeconds / DurationSeconds);
            public float SuccessZoneMin => Mathf.Repeat(SuccessCenterAngleDegrees - SuccessSweepAngleDegrees * 0.5f, 360f) / 360f;
            public float SuccessZoneMax => Mathf.Repeat(SuccessCenterAngleDegrees + SuccessSweepAngleDegrees * 0.5f, 360f) / 360f;
            public bool IsExpired => ElapsedSeconds >= DurationSeconds;
            public float HandAngleDegrees => Mathf.Repeat(startAngleDegrees + (clockwise ? -1f : 1f) * 360f * rotations * NormalizedPosition, 360f);
            public float SuccessCenterAngleDegrees { get; }
            public float SuccessSweepAngleDegrees { get; }
            public float SuccessInnerRadiusNormalized { get; }
            public float SuccessOuterRadiusNormalized { get; }

            public void Tick(float deltaTime)
            {
                ElapsedSeconds = Mathf.Min(DurationSeconds, ElapsedSeconds + Mathf.Max(0f, deltaTime));
            }

            public TimingResultGrade EvaluateAction()
            {
                return IsAngleInSuccessZone(HandAngleDegrees)
                    ? TimingResultGrade.Success
                    : TimingResultGrade.Failed;
            }

            public TimingResultGrade EvaluateTimeout()
            {
                return TimingResultGrade.Failed;
            }

            private bool IsAngleInSuccessZone(float angleDegrees)
            {
                float delta = Mathf.DeltaAngle(SuccessCenterAngleDegrees, angleDegrees);
                return Mathf.Abs(delta) <= SuccessSweepAngleDegrees * 0.5f;
            }
        }
    }
}
