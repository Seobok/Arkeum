using System;
using Arkeum.Production.Presentation.Audio;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class PauseMenuController : MonoBehaviour
    {
        private const string StartSceneName = "StartScene";

        [Header("Menu Roots")]
        [SerializeField] private GameObject pauseMenuCanvas;
        [SerializeField] private GameObject settingsCanvas;

        private Button pauseButton;
        private Button continueButton;
        private Button settingsButton;
        private Button exitButton;
        private Button quitGameButton;
        private SettingsMenuBinder settingsMenu;
        private float timeScaleBeforePause = 1f;
        private bool isPaused;
        private bool isShowingSettings;

        private void Awake()
        {
            ResolveReferences();
            BindButtons();

            pauseMenuCanvas?.SetActive(false);
            settingsCanvas?.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (isShowingSettings)
            {
                ShowPauseMenu();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        private void OnDestroy()
        {
            UnbindButtons();
            if (isPaused)
            {
                Time.timeScale = timeScaleBeforePause;
            }
        }

        public void PauseGame()
        {
            if (isPaused)
            {
                return;
            }

            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            isPaused = true;
            isShowingSettings = false;

            settingsCanvas?.SetActive(false);
            pauseMenuCanvas?.SetActive(true);
            continueButton?.Select();
            PlayMenuSfx("Pause");
        }

        public void ResumeGame()
        {
            if (!isPaused)
            {
                return;
            }

            CloseMenusAndRestoreTimeScale();
            pauseButton?.Select();
            PlayMenuSfx("Unpause");
        }

        public void LeaveToStartScene()
        {
            CloseMenusAndRestoreTimeScale();
            SceneManager.LoadScene(StartSceneName);
        }

        public void QuitGame()
        {
            CloseMenusAndRestoreTimeScale();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OpenSettings()
        {
            if (!isPaused || settingsMenu == null)
            {
                return;
            }

            pauseMenuCanvas?.SetActive(false);
            settingsCanvas?.SetActive(true);
            settingsMenu.Open();
            isShowingSettings = true;
        }

        private void ShowPauseMenu()
        {
            settingsMenu?.Close();
            settingsCanvas?.SetActive(false);
            pauseMenuCanvas?.SetActive(true);
            isShowingSettings = false;
            settingsButton?.Select();
        }

        private void ResolveReferences()
        {
            if (pauseMenuCanvas == null)
            {
                pauseMenuCanvas = FindSceneGameObject("PauseMenu-Canvas");
            }

            if (settingsCanvas == null)
            {
                settingsCanvas = FindSceneGameObject("Settings-Canvas");
            }

            pauseButton = FindSceneComponent<Button>("PauseMenu-Button");
            continueButton = FindSceneComponent<Button>("Continue-Button");
            settingsButton = FindSceneComponent<Button>("Settings-Button");
            exitButton = FindSceneComponent<Button>("Exit-Button");
            quitGameButton = FindSceneComponent<Button>("QuitGame-Button");
            settingsMenu = settingsCanvas != null
                ? settingsCanvas.GetComponentInChildren<SettingsMenuBinder>(true)
                : null;

            if (pauseMenuCanvas == null || settingsCanvas == null || pauseButton == null ||
                continueButton == null || settingsButton == null || exitButton == null ||
                quitGameButton == null || settingsMenu == null)
            {
                Debug.LogError(
                    "[PauseMenuController] GameScene pause menu references are incomplete. " +
                    "Check PauseMenu-Canvas, Settings-Canvas, PauseMenu-Button, Continue-Button, " +
                    "Settings-Button, Exit-Button, QuitGame-Button, and OptionPanel.",
                    this);
            }
        }

        private void BindButtons()
        {
            pauseButton?.onClick.AddListener(PauseGame);
            continueButton?.onClick.AddListener(ResumeGame);
            settingsButton?.onClick.AddListener(OpenSettings);
            exitButton?.onClick.AddListener(LeaveToStartScene);
            quitGameButton?.onClick.AddListener(QuitGame);
            settingsMenu?.BindBack(ShowPauseMenu);
        }

        private void UnbindButtons()
        {
            pauseButton?.onClick.RemoveListener(PauseGame);
            continueButton?.onClick.RemoveListener(ResumeGame);
            settingsButton?.onClick.RemoveListener(OpenSettings);
            exitButton?.onClick.RemoveListener(LeaveToStartScene);
            quitGameButton?.onClick.RemoveListener(QuitGame);
            settingsMenu?.BindBack(null);
        }

        private void CloseMenusAndRestoreTimeScale()
        {
            settingsMenu?.Close();
            settingsCanvas?.SetActive(false);
            pauseMenuCanvas?.SetActive(false);

            if (isPaused)
            {
                Time.timeScale = timeScaleBeforePause;
            }

            isPaused = false;
            isShowingSettings = false;
        }

        private GameObject FindSceneGameObject(string objectName)
        {
            Transform transform = FindSceneComponent<Transform>(objectName);
            return transform != null ? transform.gameObject : null;
        }

        private T FindSceneComponent<T>(string objectName) where T : Component
        {
            T[] components = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (component.gameObject.scene == gameObject.scene &&
                    string.Equals(component.name, objectName, StringComparison.Ordinal))
                {
                    return component;
                }
            }

            return null;
        }

        private static void PlayMenuSfx(string id)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx(id);
            }
        }
    }
}
