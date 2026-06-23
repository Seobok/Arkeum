using Arkeum.Production.Gameplay.Timing;
using UnityEngine;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class RadialShrinkTimingChallengePresenter : TimingChallengePresenterBase
    {
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private RectTransform boardRect;
        [SerializeField] private RadialRingGraphic successZoneRing;
        [SerializeField] private RectTransform markerRect;

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

            if (session.Runtime is IRadialShrinkTimingChallengeRuntime radialRuntime)
            {
                RefreshRadialRuntime(radialRuntime);
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

        private void RefreshRadialRuntime(IRadialShrinkTimingChallengeRuntime runtime)
        {
            successZoneRing.SetRadii(runtime.SuccessInnerRadiusNormalized, runtime.SuccessOuterRadiusNormalized);
            SetMarkerRadius(runtime.MarkerRadiusNormalized);
        }

        private void RefreshFallbackRuntime(ITimingChallengeRuntime runtime)
        {
            if (runtime == null)
            {
                return;
            }

            successZoneRing.SetRadii(runtime.SuccessZoneMin, runtime.SuccessZoneMax);
            SetMarkerRadius(1f - runtime.NormalizedPosition);
        }

        private void SetMarkerRadius(float normalizedRadius)
        {
            Rect rect = boardRect.rect;
            float boardRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
            float markerDiameter = Mathf.Max(0f, normalizedRadius) * boardRadius * 2f;
            markerRect.anchoredPosition = Vector2.zero;
            markerRect.sizeDelta = new Vector2(markerDiameter, markerDiameter);
        }

        private bool HasRequiredReferences()
        {
            bool hasReferences =
                boardRect != null &&
                successZoneRing != null &&
                markerRect != null;

            if (!hasReferences && !missingReferencesLogged)
            {
                missingReferencesLogged = true;
                Debug.LogWarning("[RadialShrinkTimingChallengePresenter] UI references are not assigned. Create the radial timing UI manually and wire boardRect, successZoneRing, and markerRect in the Inspector.", this);
            }

            return hasReferences;
        }
    }
}
