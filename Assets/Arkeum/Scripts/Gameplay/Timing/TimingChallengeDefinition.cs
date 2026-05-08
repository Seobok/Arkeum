using UnityEngine;

namespace Arkeum.Production.Gameplay.Timing
{
    public abstract class TimingChallengeDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Timing";
        [SerializeField] private float durationSeconds = 1.2f;
        [SerializeField] private float goodDamageMultiplier = 1.35f;
        [SerializeField] private float perfectDamageMultiplier = 1.75f;
        [SerializeField] private int failedFlatDamageBonus;
        [SerializeField] private int goodFlatDamageBonus;
        [SerializeField] private int perfectFlatDamageBonus;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public float DurationSeconds => Mathf.Max(0.1f, durationSeconds);
        public float GoodDamageMultiplier => Mathf.Max(0f, goodDamageMultiplier);
        public float PerfectDamageMultiplier => Mathf.Max(0f, perfectDamageMultiplier);
        public int FailedFlatDamageBonus => failedFlatDamageBonus;
        public int GoodFlatDamageBonus => goodFlatDamageBonus;
        public int PerfectFlatDamageBonus => perfectFlatDamageBonus;

        public abstract ITimingChallengeRuntime CreateRuntime();

        public TimingAttackResult BuildResult(TimingResultGrade grade)
        {
            switch (grade)
            {
                case TimingResultGrade.Perfect:
                    return new TimingAttackResult(true, grade, PerfectDamageMultiplier, PerfectFlatDamageBonus);
                case TimingResultGrade.Good:
                    return new TimingAttackResult(true, grade, GoodDamageMultiplier, GoodFlatDamageBonus);
                case TimingResultGrade.Failed:
                    return new TimingAttackResult(true, grade, 1f, FailedFlatDamageBonus);
                default:
                    return TimingAttackResult.None;
            }
        }
    }
}
