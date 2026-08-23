using Arkeum.Production.Presentation.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arkeum.Production.Gameplay.Timing
{
    public abstract class TimingChallengeDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Timing";
        [SerializeField] private TimingChallengePresenterBase presenterPrefab;
        [SerializeField, Min(0f)] private float startDelaySeconds = 0.8f;
        [SerializeField, Min(0f)] private float lateInputGraceSeconds = 0.06f;
        [SerializeField] private float durationSeconds = 1.2f;
        [FormerlySerializedAs("goodDamageMultiplier")]
        [SerializeField] private float successDamageMultiplier = 2f;
        [SerializeField] private int failedFlatDamageBonus;
        [FormerlySerializedAs("goodFlatDamageBonus")]
        [SerializeField] private int successFlatDamageBonus;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public TimingChallengePresenterBase PresenterPrefab => presenterPrefab;
        public float StartDelaySeconds => Mathf.Max(0f, startDelaySeconds);
        public float LateInputGraceSeconds => Mathf.Max(0f, lateInputGraceSeconds);
        public float DurationSeconds => Mathf.Max(0.1f, durationSeconds);
        public float SuccessDamageMultiplier => Mathf.Max(0f, successDamageMultiplier);
        public int FailedFlatDamageBonus => failedFlatDamageBonus;
        public int SuccessFlatDamageBonus => successFlatDamageBonus;

        public abstract ITimingChallengeRuntime CreateRuntime();

        public TimingAttackResult BuildResult(TimingResultGrade grade)
        {
            switch (grade)
            {
                case TimingResultGrade.Success:
                    return new TimingAttackResult(true, grade, SuccessDamageMultiplier, SuccessFlatDamageBonus);
                case TimingResultGrade.Failed:
                    return new TimingAttackResult(true, grade, 0f, FailedFlatDamageBonus);
                default:
                    return TimingAttackResult.None;
            }
        }
    }
}
