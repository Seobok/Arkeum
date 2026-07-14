using Arkeum.Production.Infrastructure.Settings;
using UnityEngine;

namespace Arkeum.Production.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class MobileControlSettingsTarget : MonoBehaviour
    {
        [SerializeField] private RectTransform movementButtonsRoot;
        [SerializeField] private CanvasGroup mobileButtonsCanvasGroup;
        [SerializeField, Min(0.1f)] private float smallScale = 0.8f;
        [SerializeField, Min(0.1f)] private float mediumScale = 1f;
        [SerializeField, Min(0.1f)] private float largeScale = 1.2f;
        [SerializeField, Min(0f)] private float horizontalPadding = 80f;

        private void Awake()
        {
            GameSettingsService.Initialize();
        }

        private void OnEnable()
        {
            GameSettingsService.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            GameSettingsService.Changed -= Apply;
        }

        public void Apply()
        {
            if (!Application.isMobilePlatform && !Application.isEditor)
            {
                return;
            }

            if (mobileButtonsCanvasGroup != null)
            {
                mobileButtonsCanvasGroup.alpha = GameSettingsService.ButtonOpacity;
            }

            if (movementButtonsRoot == null)
            {
                return;
            }

            float scale = GameSettingsService.ButtonSize switch
            {
                ControlButtonSize.Small => smallScale,
                ControlButtonSize.Large => largeScale,
                _ => mediumScale,
            };
            movementButtonsRoot.localScale = new Vector3(scale, scale, 1f);

            bool placeOnLeft = GameSettingsService.MovementSide == MovementButtonSide.Left;
            Vector2 anchor = placeOnLeft ? new Vector2(0f, 0f) : new Vector2(1f, 0f);
            movementButtonsRoot.anchorMin = anchor;
            movementButtonsRoot.anchorMax = anchor;
            movementButtonsRoot.pivot = anchor;

            Vector2 position = movementButtonsRoot.anchoredPosition;
            position.x = placeOnLeft ? horizontalPadding : -horizontalPadding;
            movementButtonsRoot.anchoredPosition = position;
        }
    }
}
