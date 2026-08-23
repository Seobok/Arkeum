using Arkeum.Production.Infrastructure.Settings;
using Arkeum.Production.Infrastructure.Persistence;
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

            SaveGameLaunchContext.Clear();
            transitionStarted = true;
            presenter.PlayExitTransition(() => SceneManager.LoadSceneAsync(gameSceneName));
        }

        private void ContinueGame()
        {
            int mostRecentSlot = FindMostRecentSlot();
            if (mostRecentSlot > 0)
            {
                LoadGameFromSlot(mostRecentSlot);
            }
        }

        // 추후 생성할 슬롯 버튼의 OnClick(int)에 1, 2, 3을 전달하면 된다.
        public void LoadGameFromSlot(int slotNumber)
        {
            if (transitionStarted)
            {
                return;
            }

            SaveGameService saveGameService = new SaveGameService();
            if (!saveGameService.HasSlot(slotNumber))
            {
                Debug.LogWarning($"[MainMenuController] Save slot {slotNumber} is empty.", this);
                return;
            }

            SaveGameLaunchContext.RequestLoad(slotNumber);
            transitionStarted = true;
            presenter.PlayExitTransition(() => SceneManager.LoadSceneAsync(gameSceneName));
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
            presenter.ShowMainMenu(FindMostRecentSlot() > 0);
            
            if(settingsMenu != null)
                settingsMenu.Close();
        }

        public bool HasSaveSlot(int slotNumber)
        {
            return new SaveGameService().HasSlot(slotNumber);
        }

        public SaveSlotMetadata GetSaveSlotMetadata(int slotNumber)
        {
            return new SaveGameService().GetSlotMetadata(slotNumber);
        }

        private static int FindMostRecentSlot()
        {
            SaveGameService saveGameService = new SaveGameService();
            int mostRecentSlot = 0;
            long mostRecentTicks = 0;
            for (int slotNumber = 1; slotNumber <= SaveGameService.SlotCount; slotNumber++)
            {
                SaveSlotMetadata metadata = saveGameService.GetSlotMetadata(slotNumber);
                if (metadata.Exists && metadata.SavedAtUtc.Ticks >= mostRecentTicks)
                {
                    mostRecentSlot = slotNumber;
                    mostRecentTicks = metadata.SavedAtUtc.Ticks;
                }
            }

            return mostRecentSlot;
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
