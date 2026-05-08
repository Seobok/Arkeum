using UnityEngine;

namespace Arkeum.Production.Gameplay.Timing
{
    [CreateAssetMenu(fileName = "SinglePressTimingChallenge", menuName = "Arkeum/Timing/Single Press Challenge")]
    public sealed class SinglePressTimingChallengeDefinition : TimingChallengeDefinition
    {
        [SerializeField, Range(0f, 1f)] private float goodZoneMin = 0.35f;
        [SerializeField, Range(0f, 1f)] private float goodZoneMax = 0.65f;
        [SerializeField, Range(0f, 1f)] private float perfectZoneMin = 0.46f;
        [SerializeField, Range(0f, 1f)] private float perfectZoneMax = 0.54f;

        public override ITimingChallengeRuntime CreateRuntime()
        {
            float normalizedGoodMin = Mathf.Clamp01(Mathf.Min(goodZoneMin, goodZoneMax));
            float normalizedGoodMax = Mathf.Clamp01(Mathf.Max(goodZoneMin, goodZoneMax));
            float normalizedPerfectMin = Mathf.Clamp(Mathf.Min(perfectZoneMin, perfectZoneMax), normalizedGoodMin, normalizedGoodMax);
            float normalizedPerfectMax = Mathf.Clamp(Mathf.Max(perfectZoneMin, perfectZoneMax), normalizedGoodMin, normalizedGoodMax);

            return new SinglePressTimingChallengeRuntime(
                DurationSeconds,
                normalizedGoodMin,
                normalizedGoodMax,
                normalizedPerfectMin,
                normalizedPerfectMax);
        }

        private sealed class SinglePressTimingChallengeRuntime : ITimingChallengeRuntime
        {
            public SinglePressTimingChallengeRuntime(
                float durationSeconds,
                float goodZoneMin,
                float goodZoneMax,
                float perfectZoneMin,
                float perfectZoneMax)
            {
                DurationSeconds = durationSeconds;
                GoodZoneMin = goodZoneMin;
                GoodZoneMax = goodZoneMax;
                PerfectZoneMin = perfectZoneMin;
                PerfectZoneMax = perfectZoneMax;
            }

            public float DurationSeconds { get; }
            public float ElapsedSeconds { get; private set; }
            public float NormalizedPosition => Mathf.Clamp01(ElapsedSeconds / DurationSeconds);
            public float GoodZoneMin { get; }
            public float GoodZoneMax { get; }
            public float PerfectZoneMin { get; }
            public float PerfectZoneMax { get; }
            public bool IsExpired => ElapsedSeconds >= DurationSeconds;

            public void Tick(float deltaTime)
            {
                ElapsedSeconds = Mathf.Min(DurationSeconds, ElapsedSeconds + Mathf.Max(0f, deltaTime));
            }

            public TimingResultGrade EvaluateAction()
            {
                float position = NormalizedPosition;
                if (position >= PerfectZoneMin && position <= PerfectZoneMax)
                {
                    return TimingResultGrade.Perfect;
                }

                if (position >= GoodZoneMin && position <= GoodZoneMax)
                {
                    return TimingResultGrade.Good;
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
