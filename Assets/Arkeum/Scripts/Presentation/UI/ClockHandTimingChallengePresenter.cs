using Arkeum.Production.Gameplay.Timing;
using UnityEngine;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class ClockHandTimingChallengePresenter : TimingChallengePresenterBase
    {
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private RectTransform boardRect;
        [SerializeField] private RadialSectorGraphic successSector;
        [SerializeField] private RectTransform handRect;

        private bool missingReferencesLogged;

        protected override void OnShow(TimingSession session)
        {
            if (popupPanel != null)
            {
                popupPanel.SetActive(true);
            }
        }

        protected override void OnRefresh(TimingSession session)
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            if (session.Runtime is IClockHandTimingChallengeRuntime clockRuntime)
            {
                RefreshClockRuntime(clockRuntime);
                return;
            }

            RefreshFallbackRuntime(session.Runtime);
        }

        protected override void OnHide()
        {
            if (popupPanel != null)
            {
                popupPanel.SetActive(false);
            }
        }

        private void RefreshClockRuntime(IClockHandTimingChallengeRuntime runtime)
        {
            successSector.SetSector(
                runtime.SuccessCenterAngleDegrees,
                runtime.SuccessSweepAngleDegrees,
                runtime.SuccessInnerRadiusNormalized,
                runtime.SuccessOuterRadiusNormalized);

            SetHandAngle(runtime.HandAngleDegrees);
        }

        private void RefreshFallbackRuntime(ITimingChallengeRuntime runtime)
        {
            if (runtime == null)
            {
                return;
            }

            float centerAngle = Mathf.Lerp(0f, 360f, (runtime.SuccessZoneMin + runtime.SuccessZoneMax) * 0.5f);
            float sweepAngle = Mathf.Abs(runtime.SuccessZoneMax - runtime.SuccessZoneMin) * 360f;
            successSector.SetSector(centerAngle, Mathf.Max(1f, sweepAngle), 0f, 1f);
            SetHandAngle(Mathf.Lerp(90f, -270f, runtime.NormalizedPosition));
        }

        private void SetHandAngle(float angleDegrees)
        {
            Rect rect = boardRect.rect;
            float boardRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
            handRect.anchoredPosition = Vector2.zero;
            handRect.sizeDelta = new Vector2(handRect.sizeDelta.x, boardRadius);
            handRect.localEulerAngles = new Vector3(0f, 0f, angleDegrees - 90f);
        }

        private bool HasRequiredReferences()
        {
            bool hasReferences =
                boardRect != null &&
                successSector != null &&
                handRect != null;

            if (!hasReferences && !missingReferencesLogged)
            {
                missingReferencesLogged = true;
                Debug.LogWarning("[ClockHandTimingChallengePresenter] UI references are not assigned. Wire boardRect, successSector, and handRect in the Inspector.", this);
            }

            return hasReferences;
        }
    }
}
