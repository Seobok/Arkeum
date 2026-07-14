using Arkeum.Production.Infrastructure.Settings;
using Arkeum.Production.Presentation.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arkeum.Production.Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MainMenuPresenter))]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuPresenter presenter;
        [SerializeField] private SettingsMenuBinder settingsMenu;
        [SerializeField] private string gameSceneName = "GameScene";
        private bool transitionStarted;

        private void Reset()
        {
            presenter = GetComponent<MainMenuPresenter>();
        }

        private void Awake()
        {
            if (presenter == null)
            {
                presenter = GetComponent<MainMenuPresenter>();
            }

            if (presenter == null || !presenter.Initialize())
            {
                enabled = false;
                return;
            }

            presenter.Bind(
                StartNewGame,
                ContinueGame,
                OpenSettings,
                QuitGame);

            if (settingsMenu != null)
            {
                settingsMenu.BindBack(ShowMainMenu);
            }
        }

        private void Start()
        {
            GameSettingsService.Initialize();
            ShowMainMenu();
        }

        private void StartNewGame()
        {
            if (transitionStarted)
            {
                return;
            }

            transitionStarted = true;
            presenter.PlayExitTransition(() => SceneManager.LoadSceneAsync(gameSceneName));
        }

        private void ContinueGame()
        {
            // SaveProfile persistence is not implemented yet. The presenter keeps this button disabled.
        }

        private void OpenSettings()
        {
            if (settingsMenu != null)
            {
                settingsMenu.Open();
                return;
            }

            Debug.LogWarning("[MainMenuController] SettingsMenuBinder is not assigned.", this);
        }

        private void ShowMainMenu()
        {
            presenter.ShowMainMenu(false);
            
            if(settingsMenu != null)
                settingsMenu.Close();
        }

        private void QuitGame()
        {
            GameSettingsService.Save();
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
