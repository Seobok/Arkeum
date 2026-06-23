using Arkeum.Production.Presentation.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arkeum.Production.Gameplay.Timing
{
    public abstract class TimingChallengeDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Timing";
        [SerializeField] private TimingChallengePresenterBase presenterPrefab;
        [SerializeField] private float durationSeconds = 1.2f;
        [FormerlySerializedAs("goodDamageMultiplier")]
        [SerializeField] private float successDamageMultiplier = 2f;
        [SerializeField] private int failedFlatDamageBonus;
        [FormerlySerializedAs("goodFlatDamageBonus")]
        [SerializeField] private int successFlatDamageBonus;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public TimingChallengePresenterBase PresenterPrefab => presenterPrefab;
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
