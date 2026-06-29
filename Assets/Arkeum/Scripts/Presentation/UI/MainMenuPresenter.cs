using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class MainMenuPresenter : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float fadeDuration = 0.25f;

        private Button primaryButton;
        private Button secondaryButton;
        private Button tertiaryButton;
        private Button quaternaryButton;
        private CanvasGroup canvasGroup;
        private bool showingSettings;
        private bool initialized;

        private Action newGameRequested;
        private Action continueRequested;
        private Action settingsRequested;
        private Action quitRequested;
        private Action masterVolumeRequested;
        private Action bgmVolumeRequested;
        private Action sfxVolumeRequested;
        private Action backRequested;

        public bool Initialize()
        {
            if (initialized)
            {
                return true;
            }

            primaryButton = FindButton("StartButton");
            secondaryButton = FindButton("LoadButton");
            tertiaryButton = FindButton("SettingButton");
            quaternaryButton = FindButton("QuitButton");

            if (primaryButton == null || secondaryButton == null || tertiaryButton == null || quaternaryButton == null)
            {
                Debug.LogError("[MainMenuPresenter] StartScene must contain StartButton, LoadButton, SettingButton, and QuitButton.", this);
                enabled = false;
                return false;
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            primaryButton.onClick.AddListener(HandlePrimaryButton);
            secondaryButton.onClick.AddListener(HandleSecondaryButton);
            tertiaryButton.onClick.AddListener(HandleTertiaryButton);
            quaternaryButton.onClick.AddListener(HandleQuaternaryButton);
            initialized = true;
            return true;
        }

        public void Bind(
            Action onNewGame,
            Action onContinue,
            Action onSettings,
            Action onQuit,
            Action onMasterVolume,
            Action onBgmVolume,
            Action onSfxVolume,
            Action onBack)
        {
            newGameRequested = onNewGame;
            continueRequested = onContinue;
            settingsRequested = onSettings;
            quitRequested = onQuit;
            masterVolumeRequested = onMasterVolume;
            bgmVolumeRequested = onBgmVolume;
            sfxVolumeRequested = onSfxVolume;
            backRequested = onBack;
        }

        public void ShowMainMenu(bool canContinue)
        {
            showingSettings = false;
            SetButton(primaryButton, "New Game", true);
            SetButton(secondaryButton, canContinue ? "Continue" : "Continue (No Save)", canContinue);
            SetButton(tertiaryButton, "Settings", true);
            SetButton(quaternaryButton, "Quit", true);
            Select(primaryButton);
        }

        public void ShowSettings(float masterVolume, float bgmVolume, float sfxVolume)
        {
            showingSettings = true;
            SetButton(primaryButton, $"Master  {ToPercent(masterVolume)}", true);
            SetButton(secondaryButton, $"BGM  {ToPercent(bgmVolume)}", true);
            SetButton(tertiaryButton, $"SFX  {ToPercent(sfxVolume)}", true);
            SetButton(quaternaryButton, "Back", true);
            Select(primaryButton);
        }

        public void PlayExitTransition(Action onComplete)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutRoutine(onComplete));
        }

        private IEnumerator FadeOutRoutine(Action onComplete)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            float startAlpha = canvasGroup.alpha;

            if (fadeDuration > 0f)
            {
                for (float elapsed = 0f; elapsed < fadeDuration; elapsed += Time.unscaledDeltaTime)
                {
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                    yield return null;
                }
            }

            canvasGroup.alpha = 0f;
            onComplete?.Invoke();
        }

        private void HandlePrimaryButton()
        {
            if (showingSettings)
            {
                masterVolumeRequested?.Invoke();
                return;
            }

            newGameRequested?.Invoke();
        }

        private void HandleSecondaryButton()
        {
            if (showingSettings)
            {
                bgmVolumeRequested?.Invoke();
                return;
            }

            continueRequested?.Invoke();
        }

        private void HandleTertiaryButton()
        {
            if (showingSettings)
            {
                sfxVolumeRequested?.Invoke();
                return;
            }

            settingsRequested?.Invoke();
        }

        private void HandleQuaternaryButton()
        {
            if (showingSettings)
            {
                backRequested?.Invoke();
                return;
            }

            quitRequested?.Invoke();
        }

        private Button FindButton(string expectedName)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (string.Equals(buttons[i].name.Trim(), expectedName, StringComparison.Ordinal))
                {
                    return buttons[i];
                }
            }

            return null;
        }

        private static void SetButton(Button button, string label, bool interactable)
        {
            button.interactable = interactable;
            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
            }
        }

        private static void Select(Button button)
        {
            if (button != null && button.interactable)
            {
                button.Select();
            }
        }

        private static string ToPercent(float value)
        {
            return $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
        }
    }
}
