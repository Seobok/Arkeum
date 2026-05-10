using Arkeum.Production.Gameplay.Timing;
using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class TimingPopupPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private RectTransform trackRect;
        [SerializeField] private RectTransform goodZoneRect;
        [SerializeField] private RectTransform perfectZoneRect;
        [SerializeField] private RectTransform markerRect;

        private TimingSession session;
        private bool missingReferencesLogged;

        public bool IsVisible => session != null;

        public void Initialize()
        {
            Hide();
        }

        public void Show(TimingSession timingSession)
        {
            session = timingSession;
            if (!HasRequiredReferences())
            {
                return;
            }

            popupPanel.SetActive(session != null);
            Refresh();
        }

        public void Hide()
        {
            session = null;
            if (popupPanel != null)
            {
                popupPanel.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (session == null || !HasRequiredReferences())
            {
                return;
            }

            ITimingChallengeRuntime runtime = session.Runtime;
            popupPanel.SetActive(runtime != null);
            if (runtime == null)
            {
                return;
            }

            titleText.text = session.Definition != null ? session.Definition.DisplayName : "Timing";
            SetZone(goodZoneRect, runtime.GoodZoneMin, runtime.GoodZoneMax);
            SetZone(perfectZoneRect, runtime.PerfectZoneMin, runtime.PerfectZoneMax);
            SetMarker(runtime.NormalizedPosition);
        }

        private bool HasRequiredReferences()
        {
            bool hasReferences =
                popupPanel != null &&
                titleText != null &&
                trackRect != null &&
                goodZoneRect != null &&
                perfectZoneRect != null &&
                markerRect != null;

            if (!hasReferences && !missingReferencesLogged)
            {
                missingReferencesLogged = true;
                Debug.LogWarning("[TimingPopupPresenter] UGUI references are not fully assigned. Create the timing popup manually and wire the serialized fields in the Inspector.", this);
            }

            return hasReferences;
        }

        private void SetZone(RectTransform zoneRect, float min, float max)
        {
            float clampedMin = Mathf.Clamp01(min);
            float clampedMax = Mathf.Clamp01(max);
            float width = trackRect.rect.width;
            if (width <= 0f)
            {
                return;
            }

            float zoneWidth = Mathf.Max(1f, (clampedMax - clampedMin) * width);
            float center = ((clampedMin + clampedMax) * 0.5f - 0.5f) * width;
            zoneRect.sizeDelta = new Vector2(zoneWidth, zoneRect.sizeDelta.y);
            zoneRect.anchoredPosition = new Vector2(center, zoneRect.anchoredPosition.y);
        }

        private void SetMarker(float normalizedPosition)
        {
            float width = trackRect.rect.width;
            if (width <= 0f)
            {
                return;
            }

            float x = (Mathf.Clamp01(normalizedPosition) - 0.5f) * width;
            markerRect.anchoredPosition = new Vector2(x, markerRect.anchoredPosition.y);
        }
    }
}
