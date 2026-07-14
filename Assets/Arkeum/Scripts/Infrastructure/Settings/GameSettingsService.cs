using System;
using Arkeum.Production.Presentation.Audio;
using UnityEngine;

namespace Arkeum.Production.Infrastructure.Settings
{
    public enum ControlButtonSize { Small, Medium, Large }
    public enum MovementButtonSide { Left, Right }
    public enum GraphicsQuality { Low, Medium, High }
    public enum FrameRateLimit { Fps30, Fps60, Unlimited }

    public static class GameSettingsService
    {
        private const string MasterVolumeKey = "Arkeum.Audio.MasterVolume";
        private const string BgmVolumeKey = "Arkeum.Audio.BgmVolume";
        private const string SfxVolumeKey = "Arkeum.Audio.SfxVolume";
        private const string VibrationKey = "Arkeum.Settings.Vibration";
        private const string ControlButtonSizeKey = "Arkeum.Settings.ControlButtonSize";
        private const string ControlButtonOpacityKey = "Arkeum.Settings.ControlButtonOpacity";
        private const string MovementButtonSideKey = "Arkeum.Settings.MovementButtonSide";
        private const string GraphicsQualityKey = "Arkeum.Settings.GraphicsQuality";
        private const string FrameRateLimitKey = "Arkeum.Settings.FrameRateLimit";
        private const string ScreenShakeKey = "Arkeum.Settings.ScreenShake";
        private const string BatterySaverKey = "Arkeum.Settings.BatterySaver";

        private static bool initialized;

        public static float MasterVolume { get; private set; } = 1f;
        public static float BgmVolume { get; private set; } = 1f;
        public static float SfxVolume { get; private set; } = 1f;
        public static bool VibrationEnabled { get; private set; } = true;
        public static ControlButtonSize ButtonSize { get; private set; } = ControlButtonSize.Medium;
        public static float ButtonOpacity { get; private set; } = 1f;
        public static MovementButtonSide MovementSide { get; private set; } = MovementButtonSide.Left;
        public static GraphicsQuality Quality { get; private set; } = GraphicsQuality.Medium;
        public static FrameRateLimit FrameLimit { get; private set; } = FrameRateLimit.Fps60;
        public static bool ScreenShakeEnabled { get; private set; } = true;
        public static bool BatterySaverEnabled { get; private set; }

        public static event Action Changed;

        public static void Initialize()
        {
            if (initialized)
            {
                ApplyAll();
                return;
            }

            MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
            BgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, 1f));
            SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
            VibrationEnabled = GetBool(VibrationKey, true);
            ButtonSize = GetEnum(ControlButtonSizeKey, ControlButtonSize.Medium);
            ButtonOpacity = Mathf.Clamp01(PlayerPrefs.GetFloat(ControlButtonOpacityKey, 1f));
            MovementSide = GetEnum(MovementButtonSideKey, MovementButtonSide.Left);
            Quality = GetEnum(GraphicsQualityKey, GraphicsQuality.Medium);
            FrameLimit = GetEnum(FrameRateLimitKey, FrameRateLimit.Fps60);
            ScreenShakeEnabled = GetBool(ScreenShakeKey, true);
            BatterySaverEnabled = GetBool(BatterySaverKey, false);
            initialized = true;
            ApplyAll();
        }

        public static void ApplyAll()
        {
            ApplyAudioSettings();
            ApplyGraphicsQuality();
            ApplyFrameRate();
        }

        public static void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
            ApplyAudioSettings();
            NotifyChanged();
        }

        public static void SetBgmVolume(float value)
        {
            BgmVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
            ApplyAudioSettings();
            NotifyChanged();
        }

        public static void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
            ApplyAudioSettings();
            NotifyChanged();
        }

        public static void SetVibrationEnabled(bool enabled)
        {
            VibrationEnabled = enabled;
            SetBool(VibrationKey, enabled);
            NotifyChanged();
        }

        public static void SetButtonSize(ControlButtonSize size)
        {
            ButtonSize = size;
            PlayerPrefs.SetInt(ControlButtonSizeKey, (int)size);
            NotifyChanged();
        }

        public static void SetButtonOpacity(float value)
        {
            ButtonOpacity = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(ControlButtonOpacityKey, ButtonOpacity);
            NotifyChanged();
        }

        public static void SetMovementSide(MovementButtonSide side)
        {
            MovementSide = side;
            PlayerPrefs.SetInt(MovementButtonSideKey, (int)side);
            NotifyChanged();
        }

        public static void SetGraphicsQuality(GraphicsQuality quality)
        {
            Quality = quality;
            PlayerPrefs.SetInt(GraphicsQualityKey, (int)quality);
            ApplyGraphicsQuality();
            NotifyChanged();
        }

        public static void SetFrameRateLimit(FrameRateLimit limit)
        {
            FrameLimit = limit;
            PlayerPrefs.SetInt(FrameRateLimitKey, (int)limit);
            ApplyFrameRate();
            NotifyChanged();
        }

        public static void SetScreenShakeEnabled(bool enabled)
        {
            ScreenShakeEnabled = enabled;
            SetBool(ScreenShakeKey, enabled);
            NotifyChanged();
        }

        public static void SetBatterySaverEnabled(bool enabled)
        {
            BatterySaverEnabled = enabled;
            SetBool(BatterySaverKey, enabled);
            ApplyGraphicsQuality();
            ApplyFrameRate();
            NotifyChanged();
        }

        public static bool TryVibrate()
        {
            if (!Application.isMobilePlatform || !VibrationEnabled)
            {
                return false;
            }

            Handheld.Vibrate();
            return true;
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        private static void ApplyAudioSettings()
        {
            AudioManager audioManager = AudioManager.Instance;
            if (audioManager == null)
            {
                return;
            }

            audioManager.SetMasterVolume(MasterVolume);
            audioManager.SetBgmVolume(BgmVolume);
            audioManager.SetSfxVolume(SfxVolume);
        }

        private static void ApplyGraphicsQuality()
        {
            int qualityCount = QualitySettings.names.Length;
            if (qualityCount <= 0)
            {
                return;
            }

            GraphicsQuality effectiveQuality = BatterySaverEnabled ? GraphicsQuality.Low : Quality;
            int qualityIndex = effectiveQuality switch
            {
                GraphicsQuality.Low => 0,
                GraphicsQuality.High => qualityCount - 1,
                _ => qualityCount / 2,
            };
            QualitySettings.SetQualityLevel(qualityIndex, true);
        }

        private static void ApplyFrameRate()
        {
            FrameRateLimit effectiveLimit = BatterySaverEnabled ? FrameRateLimit.Fps30 : FrameLimit;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = effectiveLimit switch
            {
                FrameRateLimit.Fps30 => 30,
                FrameRateLimit.Fps60 => 60,
                _ => -1,
            };
        }

        private static void NotifyChanged()
        {
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        private static bool GetBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        private static TEnum GetEnum<TEnum>(string key, TEnum defaultValue) where TEnum : struct, Enum
        {
            int rawValue = PlayerPrefs.GetInt(key, Convert.ToInt32(defaultValue));
            return Enum.IsDefined(typeof(TEnum), rawValue)
                ? (TEnum)Enum.ToObject(typeof(TEnum), rawValue)
                : defaultValue;
        }
    }
}
