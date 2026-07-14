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

        [SerializeField] private Button startButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button quitButton;
        private CanvasGroup canvasGroup;
        private bool initialized;

        private Action newGameRequested;
        private Action continueRequested;
        private Action settingsRequested;
        private Action quitRequested;

        public bool Initialize()
        {
            if (initialized)
            {
                return true;
            }

            if(startButton == null)
                startButton = FindButton("StartButton");
            if (loadButton == null)
                loadButton = FindButton("LoadButton");
            if (settingButton == null)
                settingButton = FindButton("SettingButton");
            if (quitButton == null)
                quitButton = FindButton("QuitButton");

            if (startButton == null || loadButton == null || settingButton == null || quitButton == null)
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

            startButton.onClick.AddListener(HandleStartButton);
            loadButton.onClick.AddListener(HandleLoadButton);
            settingButton.onClick.AddListener(HandleSettingButton);
            quitButton.onClick.AddListener(HandleQuitButton);
            
            initialized = true;
            return true;
        }

        public void Bind(
            Action onNewGame,
            Action onContinue,
            Action onSettings,
            Action onQuit)
        {
            newGameRequested = onNewGame;
            continueRequested = onContinue;
            settingsRequested = onSettings;
            quitRequested = onQuit;
        }

        public void ShowMainMenu(bool canContinue)
        {
            SetButton(startButton, "New Game", true);
            SetButton(loadButton, "Continue", canContinue);
            SetButton(settingButton, "Settings", true);
            SetButton(quitButton, "Quit", true);
            Select(startButton);
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

        private void HandleStartButton()
        {
            newGameRequested?.Invoke();
        }

        private void HandleLoadButton()
        {
            continueRequested?.Invoke();
        }

        private void HandleSettingButton()
        {
            settingsRequested?.Invoke();
        }

        private void HandleQuitButton()
        {
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
    }
}
