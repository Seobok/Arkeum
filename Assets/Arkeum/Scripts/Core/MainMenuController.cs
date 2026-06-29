using Arkeum.Production.Presentation.Audio;
using Arkeum.Production.Presentation.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arkeum.Production.Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MainMenuPresenter))]
    public sealed class MainMenuController : MonoBehaviour
    {
        private const string MasterVolumeKey = "Arkeum.Audio.MasterVolume";
        private const string BgmVolumeKey = "Arkeum.Audio.BgmVolume";
        private const string SfxVolumeKey = "Arkeum.Audio.SfxVolume";
        private const float VolumeStep = 0.25f;

        [SerializeField] private MainMenuPresenter presenter;
        [SerializeField] private string gameSceneName = "GameScene";

        private float masterVolume;
        private float bgmVolume;
        private float sfxVolume;
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
                ShowSettings,
                QuitGame,
                CycleMasterVolume,
                CycleBgmVolume,
                CycleSfxVolume,
                ShowMainMenu);
        }

        private void Start()
        {
            LoadAndApplyAudioSettings();
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

        private void ShowSettings()
        {
            presenter.ShowSettings(masterVolume, bgmVolume, sfxVolume);
        }

        private void ShowMainMenu()
        {
            presenter.ShowMainMenu(false);
        }

        private void CycleMasterVolume()
        {
            masterVolume = GetNextVolume(masterVolume);
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
            ApplyAudioSettings();
            ShowSettings();
        }

        private void CycleBgmVolume()
        {
            bgmVolume = GetNextVolume(bgmVolume);
            PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
            ApplyAudioSettings();
            ShowSettings();
        }

        private void CycleSfxVolume()
        {
            sfxVolume = GetNextVolume(sfxVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
            ApplyAudioSettings();
            ShowSettings();
        }

        private void LoadAndApplyAudioSettings()
        {
            masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            ApplyAudioSettings();
        }

        private void ApplyAudioSettings()
        {
            AudioManager audioManager = AudioManager.Instance;
            if (audioManager == null)
            {
                return;
            }

            audioManager.SetMasterVolume(masterVolume);
            audioManager.SetBgmVolume(bgmVolume);
            audioManager.SetSfxVolume(sfxVolume);
        }

        private void QuitGame()
        {
            PlayerPrefs.Save();
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private static float GetNextVolume(float current)
        {
            float next = Mathf.Round(current / VolumeStep) * VolumeStep - VolumeStep;
            return next < 0f ? 1f : Mathf.Clamp01(next);
        }
    }
}
