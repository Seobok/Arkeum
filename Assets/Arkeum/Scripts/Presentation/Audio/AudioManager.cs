using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Arkeum.Production.Presentation.Audio
{
    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        [System.Serializable]
        private sealed class AudioEntry
        {
            [SerializeField] private string id;
            [SerializeField] private AudioClip clip;
            [SerializeField, Range(0f, 1f)] private float volume = 1f;
            [SerializeField, Range(0.1f, 3f)] private float pitch = 1f;
            [SerializeField, Range(0f, 1f)] private float randomPitchRange;
            [SerializeField, Min(0f)] private float cooldownSeconds;

            public string Id => id;
            public AudioClip Clip => clip;
            public float Volume => volume;
            public float CooldownSeconds => Mathf.Max(0f, cooldownSeconds);

            public float GetPitch()
            {
                if (randomPitchRange <= 0f)
                {
                    return pitch;
                }

                return Random.Range(pitch - randomPitchRange, pitch + randomPitchRange);
            }
        }

        [Header("Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private Transform sfxSourceRoot;
        [SerializeField, Min(1)] private int sfxPoolSize = 12;

        [Header("Mixer")]
        [SerializeField] private AudioMixerGroup bgmMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

        [Header("Clips")]
        [SerializeField] private List<AudioEntry> bgmClips = new List<AudioEntry>();
        [SerializeField] private List<AudioEntry> sfxClips = new List<AudioEntry>();

        private readonly List<AudioSource> sfxSources = new List<AudioSource>();
        private readonly Dictionary<string, float> nextSfxPlayTimes = new Dictionary<string, float>();
        private Coroutine bgmFadeRoutine;
        private AudioClip currentBgmClip;
        private float currentBgmClipVolume = 1f;
        private bool isMuted;

        public static AudioManager Instance { get; private set; }

        public float MasterVolume => masterVolume;
        public float BgmVolume => bgmVolume;
        public float SfxVolume => sfxVolume;
        public bool IsMuted => isMuted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureBgmSource();
            EnsureSfxSources();
            ApplySourceVolumes();
        }

        private void OnValidate()
        {
            sfxPoolSize = Mathf.Max(1, sfxPoolSize);
            masterVolume = Mathf.Clamp01(masterVolume);
            bgmVolume = Mathf.Clamp01(bgmVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);
            ApplySourceVolumes();
        }

        public void PlayBgm(string id, float fadeDuration = 0.5f, bool restartIfSame = false)
        {
            if (!TryFindEntry(bgmClips, id, out AudioEntry entry))
            {
                Debug.LogWarning($"[AudioManager] BGM id '{id}' was not found.", this);
                return;
            }

            PlayBgm(entry.Clip, entry.Volume, fadeDuration, restartIfSame);
        }

        public void PlayBgm(AudioClip clip, float clipVolume = 1f, float fadeDuration = 0.5f, bool restartIfSame = false)
        {
            if (clip == null)
            {
                StopBgm(fadeDuration);
                return;
            }

            EnsureBgmSource();
            if (!restartIfSame && currentBgmClip == clip && bgmSource.isPlaying)
            {
                return;
            }

            StartBgmFade(clip, Mathf.Clamp01(clipVolume), fadeDuration);
        }

        public void StopBgm(float fadeDuration = 0.5f)
        {
            EnsureBgmSource();
            StartBgmFade(null, 0f, fadeDuration);
        }

        public void PauseBgm()
        {
            EnsureBgmSource();
            bgmSource.Pause();
        }

        public void ResumeBgm()
        {
            EnsureBgmSource();
            bgmSource.UnPause();
        }

        public void PlaySfx(string id)
        {
            PlaySfx(id, Vector3.zero, false);
        }

        public void PlaySfxAt(string id, Vector3 position)
        {
            PlaySfx(id, position, true);
        }

        public void PlaySfxDelayed(string id, float delaySeconds)
        {
            if (delaySeconds <= 0f)
            {
                PlaySfx(id);
                return;
            }

            StartCoroutine(PlaySfxDelayedRoutine(id, delaySeconds));
        }

        public void PlaySfx(AudioClip clip, float clipVolume = 1f, float pitch = 1f)
        {
            PlaySfxInternal(clip, Mathf.Clamp01(clipVolume), pitch, Vector3.zero, false);
        }

        public void StopAllSfx()
        {
            EnsureSfxSources();
            for (int i = 0; i < sfxSources.Count; i++)
            {
                sfxSources[i].Stop();
            }
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplySourceVolumes();
        }

        public void SetBgmVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            ApplySourceVolumes();
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            ApplySourceVolumes();
        }

        public void SetMuted(bool muted)
        {
            isMuted = muted;
            ApplySourceVolumes();
        }

        private void PlaySfx(string id, Vector3 position, bool usePosition)
        {
            if (!TryFindEntry(sfxClips, id, out AudioEntry entry))
            {
                Debug.LogWarning($"[AudioManager] SFX id '{id}' was not found.", this);
                return;
            }

            if (entry.CooldownSeconds > 0f &&
                nextSfxPlayTimes.TryGetValue(id, out float nextPlayTime) &&
                Time.unscaledTime < nextPlayTime)
            {
                return;
            }

            if (entry.CooldownSeconds > 0f)
            {
                nextSfxPlayTimes[id] = Time.unscaledTime + entry.CooldownSeconds;
            }

            PlaySfxInternal(entry.Clip, entry.Volume, entry.GetPitch(), position, usePosition);
        }

        private void PlaySfxInternal(AudioClip clip, float clipVolume, float pitch, Vector3 position, bool usePosition)
        {
            if (clip == null || isMuted)
            {
                return;
            }

            AudioSource source = GetAvailableSfxSource();
            source.transform.position = position;
            source.spatialBlend = usePosition ? 1f : 0f;
            source.clip = clip;
            source.pitch = Mathf.Max(0.1f, pitch);
            source.volume = clipVolume * sfxVolume * masterVolume;
            source.outputAudioMixerGroup = sfxMixerGroup;
            source.Play();
        }

        private void StartBgmFade(AudioClip nextClip, float nextClipVolume, float fadeDuration)
        {
            if (bgmFadeRoutine != null)
            {
                StopCoroutine(bgmFadeRoutine);
            }

            bgmFadeRoutine = StartCoroutine(FadeBgmRoutine(nextClip, nextClipVolume, Mathf.Max(0f, fadeDuration)));
        }

        private IEnumerator PlaySfxDelayedRoutine(string id, float delaySeconds)
        {
            yield return new WaitForSecondsRealtime(delaySeconds);
            PlaySfx(id);
        }

        private IEnumerator FadeBgmRoutine(AudioClip nextClip, float nextClipVolume, float fadeDuration)
        {
            float startVolume = bgmSource.volume;
            if (fadeDuration > 0f)
            {
                for (float elapsed = 0f; elapsed < fadeDuration; elapsed += Time.unscaledDeltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                    yield return null;
                }
            }

            bgmSource.volume = 0f;
            bgmSource.Stop();
            bgmSource.clip = nextClip;
            currentBgmClip = nextClip;
            currentBgmClipVolume = nextClipVolume;

            if (nextClip == null)
            {
                bgmFadeRoutine = null;
                yield break;
            }

            bgmSource.loop = true;
            bgmSource.outputAudioMixerGroup = bgmMixerGroup;
            bgmSource.Play();

            float targetVolume = GetEffectiveBgmVolume();
            if (fadeDuration > 0f)
            {
                for (float elapsed = 0f; elapsed < fadeDuration; elapsed += Time.unscaledDeltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
                    yield return null;
                }
            }

            bgmSource.volume = targetVolume;
            bgmFadeRoutine = null;
        }

        private void EnsureBgmSource()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }

            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        }

        private void EnsureSfxSources()
        {
            if (sfxSourceRoot == null)
            {
                GameObject root = new GameObject("SfxSources");
                root.transform.SetParent(transform, false);
                sfxSourceRoot = root.transform;
            }

            while (sfxSources.Count < sfxPoolSize)
            {
                GameObject sourceObject = new GameObject($"SfxSource_{sfxSources.Count:00}");
                sourceObject.transform.SetParent(sfxSourceRoot, false);
                AudioSource source = sourceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.outputAudioMixerGroup = sfxMixerGroup;
                sfxSources.Add(source);
            }
        }

        private AudioSource GetAvailableSfxSource()
        {
            EnsureSfxSources();
            for (int i = 0; i < sfxSources.Count; i++)
            {
                if (!sfxSources[i].isPlaying)
                {
                    return sfxSources[i];
                }
            }

            return sfxSources[0];
        }

        private void ApplySourceVolumes()
        {
            float effectiveMasterVolume = isMuted ? 0f : masterVolume;
            if (bgmSource != null)
            {
                bgmSource.volume = GetEffectiveBgmVolume();
            }

            for (int i = 0; i < sfxSources.Count; i++)
            {
                if (sfxSources[i] != null)
                {
                    if (!sfxSources[i].isPlaying)
                    {
                        sfxSources[i].volume = sfxVolume * effectiveMasterVolume;
                    }
                }
            }
        }

        private float GetEffectiveBgmVolume()
        {
            return isMuted ? 0f : currentBgmClipVolume * bgmVolume * masterVolume;
        }

        private static bool TryFindEntry(List<AudioEntry> entries, string id, out AudioEntry entry)
        {
            entry = null;
            if (entries == null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                AudioEntry candidate = entries[i];
                if (candidate != null && candidate.Id == id && candidate.Clip != null)
                {
                    entry = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
