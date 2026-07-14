using System;
using Arkeum.Production.Infrastructure.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class SettingsMenuBinder : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button backButton;

        [Header("Sound")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle vibrationToggle;

        [Header("Controls - Button Size")]
        [SerializeField] private Toggle smallButtonSizeToggle;
        [SerializeField] private Toggle mediumButtonSizeToggle;
        [SerializeField] private Toggle largeButtonSizeToggle;
        [SerializeField] private Slider buttonOpacitySlider;

        [Header("Controls - Movement Side")]
        [SerializeField] private Toggle movementLeftToggle;
        [SerializeField] private Toggle movementRightToggle;

        [Header("Display - Quality")]
        [SerializeField] private Toggle lowQualityToggle;
        [SerializeField] private Toggle mediumQualityToggle;
        [SerializeField] private Toggle highQualityToggle;

        [Header("Display - Frame Rate")]
        [SerializeField] private Toggle fps30Toggle;
        [SerializeField] private Toggle fps60Toggle;
        [SerializeField] private Toggle unlimitedFpsToggle;

        [Header("Display - Options")]
        [SerializeField] private Toggle screenShakeToggle;
        [SerializeField] private Toggle batterySaverToggle;

        private bool initialized;
        private bool refreshing;
        private Action backRequested;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            GameSettingsService.Changed += RefreshUi;
            RefreshUi();
        }

        private void OnDisable()
        {
            GameSettingsService.Changed -= RefreshUi;
        }

        public void Open()
        {
            EnsureInitialized();
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            RefreshUi();
        }

        public void Close()
        {
            GameSettingsService.Save();
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        public void BindBack(Action onBack)
        {
            backRequested = onBack;
        }

        public void RefreshUi()
        {
            if (!initialized || refreshing)
            {
                return;
            }

            refreshing = true;
            SetSlider(masterVolumeSlider, GameSettingsService.MasterVolume);
            SetSlider(bgmVolumeSlider, GameSettingsService.BgmVolume);
            SetSlider(sfxVolumeSlider, GameSettingsService.SfxVolume);
            SetToggle(vibrationToggle, GameSettingsService.VibrationEnabled);
            SetToggle(smallButtonSizeToggle, GameSettingsService.ButtonSize == ControlButtonSize.Small);
            SetToggle(mediumButtonSizeToggle, GameSettingsService.ButtonSize == ControlButtonSize.Medium);
            SetToggle(largeButtonSizeToggle, GameSettingsService.ButtonSize == ControlButtonSize.Large);
            SetSlider(buttonOpacitySlider, GameSettingsService.ButtonOpacity);
            SetToggle(movementLeftToggle, GameSettingsService.MovementSide == MovementButtonSide.Left);
            SetToggle(movementRightToggle, GameSettingsService.MovementSide == MovementButtonSide.Right);
            SetToggle(lowQualityToggle, GameSettingsService.Quality == GraphicsQuality.Low);
            SetToggle(mediumQualityToggle, GameSettingsService.Quality == GraphicsQuality.Medium);
            SetToggle(highQualityToggle, GameSettingsService.Quality == GraphicsQuality.High);
            SetToggle(fps30Toggle, GameSettingsService.FrameLimit == FrameRateLimit.Fps30);
            SetToggle(fps60Toggle, GameSettingsService.FrameLimit == FrameRateLimit.Fps60);
            SetToggle(unlimitedFpsToggle, GameSettingsService.FrameLimit == FrameRateLimit.Unlimited);
            SetToggle(screenShakeToggle, GameSettingsService.ScreenShakeEnabled);
            SetToggle(batterySaverToggle, GameSettingsService.BatterySaverEnabled);
            refreshing = false;
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            GameSettingsService.Initialize();
            ConfigureSlider(masterVolumeSlider);
            ConfigureSlider(bgmVolumeSlider);
            ConfigureSlider(sfxVolumeSlider);
            ConfigureSlider(buttonOpacitySlider);
            BindListeners();
            initialized = true;
        }

        private void BindListeners()
        {
            AddSliderListener(masterVolumeSlider, GameSettingsService.SetMasterVolume);
            AddSliderListener(bgmVolumeSlider, GameSettingsService.SetBgmVolume);
            AddSliderListener(sfxVolumeSlider, GameSettingsService.SetSfxVolume);
            AddToggleListener(vibrationToggle, GameSettingsService.SetVibrationEnabled);
            AddRadioListener(smallButtonSizeToggle, () => GameSettingsService.SetButtonSize(ControlButtonSize.Small));
            AddRadioListener(mediumButtonSizeToggle, () => GameSettingsService.SetButtonSize(ControlButtonSize.Medium));
            AddRadioListener(largeButtonSizeToggle, () => GameSettingsService.SetButtonSize(ControlButtonSize.Large));
            AddSliderListener(buttonOpacitySlider, GameSettingsService.SetButtonOpacity);
            AddRadioListener(movementLeftToggle, () => GameSettingsService.SetMovementSide(MovementButtonSide.Left));
            AddRadioListener(movementRightToggle, () => GameSettingsService.SetMovementSide(MovementButtonSide.Right));
            AddRadioListener(lowQualityToggle, () => GameSettingsService.SetGraphicsQuality(GraphicsQuality.Low));
            AddRadioListener(mediumQualityToggle, () => GameSettingsService.SetGraphicsQuality(GraphicsQuality.Medium));
            AddRadioListener(highQualityToggle, () => GameSettingsService.SetGraphicsQuality(GraphicsQuality.High));
            AddRadioListener(fps30Toggle, () => GameSettingsService.SetFrameRateLimit(FrameRateLimit.Fps30));
            AddRadioListener(fps60Toggle, () => GameSettingsService.SetFrameRateLimit(FrameRateLimit.Fps60));
            AddRadioListener(unlimitedFpsToggle, () => GameSettingsService.SetFrameRateLimit(FrameRateLimit.Unlimited));
            AddToggleListener(screenShakeToggle, GameSettingsService.SetScreenShakeEnabled);
            AddToggleListener(batterySaverToggle, GameSettingsService.SetBatterySaverEnabled);
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackButton);
            }
        }

        private void HandleBackButton()
        {
            backRequested?.Invoke();
        }

        private void AddSliderListener(Slider slider, UnityEngine.Events.UnityAction<float> callback)
        {
            if (slider != null)
            {
                slider.onValueChanged.AddListener(value =>
                {
                    if (!refreshing)
                    {
                        callback(value);
                    }
                });
            }
        }

        private void AddToggleListener(Toggle toggle, UnityEngine.Events.UnityAction<bool> callback)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(value =>
                {
                    if (!refreshing)
                    {
                        callback(value);
                    }
                });
            }
        }

        private void AddRadioListener(Toggle toggle, Action callback)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn && !refreshing)
                    {
                        callback();
                    }
                });
            }
        }

        private static void ConfigureSlider(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
        }

        private static void SetSlider(Slider slider, float value)
        {
            slider?.SetValueWithoutNotify(value);
        }

        private static void SetToggle(Toggle toggle, bool value)
        {
            toggle?.SetIsOnWithoutNotify(value);
        }
    }
}
