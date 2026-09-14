using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Ftg.SoundSystem
{
    [DisallowMultipleComponent]
    public sealed class SoundService : MonoBehaviour
    {
        private sealed class Voice
        {
            public AudioSource Source;
            public string Key;
        }

        private static SoundService instance;

        private readonly Dictionary<string, SoundEntry> entries = new(System.StringComparer.Ordinal);
        private readonly Dictionary<SoundChannel, float> volumes = new();
        private readonly List<Voice> voices = new();

        private SoundSystemSettings settings;
        private SoundVolumeStore volumeStore;
        private AudioSource[] bgmSources;
        private AudioSource ambienceSource;
        private int activeBgmIndex;
        private int bgmGeneration;
        private int ambienceGeneration;
        private Coroutine bgmRoutine;
        private Coroutine ambienceRoutine;
        private bool initialized;

        public static SoundService Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                instance = FindFirstObjectByType<SoundService>();
                if (instance != null)
                    return instance;

                var root = new GameObject("[SoundService]");
                instance = root.AddComponent<SoundService>();
                return instance;
            }
        }

        public bool IsInitialized => initialized;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize(SoundSystemSettings newSettings)
        {
            if (newSettings == null)
                throw new System.ArgumentNullException(nameof(newSettings));

            StopAllCoroutines();
            StopAllSources();
            settings = newSettings;
            entries.Clear();

            if (settings.Catalog != null)
            {
                settings.Catalog.TryBuildLookup(out var lookup);
                foreach (var pair in lookup)
                    entries.Add(pair.Key, pair.Value);
            }

            volumeStore = new SoundVolumeStore(settings.PlayerPrefsPrefix);
            CreatePersistentSources();
            CreateInitialVoicePool();

            foreach (SoundChannel channel in System.Enum.GetValues(typeof(SoundChannel)))
            {
                var saved = volumeStore.Load(channel);
                volumes[channel] = saved;
                ApplyMixerVolume(channel, saved);
            }

            initialized = true;
        }

        public bool PlayBgm(string key, float fadeSeconds = -1f)
        {
            if (!TryGetEntry(key, SoundChannel.Bgm, out var entry))
                return false;

            EnsureInitialized();
            bgmGeneration++;
            if (bgmRoutine != null)
                StopCoroutine(bgmRoutine);

            var duration = fadeSeconds < 0f ? settings.DefaultCrossfadeSeconds : Mathf.Max(0f, fadeSeconds);
            bgmRoutine = StartCoroutine(CrossfadeBgm(entry, duration, bgmGeneration));
            return true;
        }

        public void StopBgm(float fadeSeconds = -1f)
        {
            EnsureInitialized();
            bgmGeneration++;
            if (bgmRoutine != null)
                StopCoroutine(bgmRoutine);

            var duration = fadeSeconds < 0f ? settings.DefaultCrossfadeSeconds : Mathf.Max(0f, fadeSeconds);
            bgmRoutine = StartCoroutine(FadeOutAndStop(bgmSources[activeBgmIndex], duration));
        }

        public bool PlayAmbience(string key, float fadeSeconds = -1f)
        {
            if (!TryGetEntry(key, SoundChannel.Ambience, out var entry))
                return false;

            EnsureInitialized();
            ambienceGeneration++;
            if (ambienceRoutine != null)
                StopCoroutine(ambienceRoutine);

            var duration = fadeSeconds < 0f ? settings.DefaultCrossfadeSeconds : Mathf.Max(0f, fadeSeconds);
            ambienceRoutine = StartCoroutine(ChangeAmbience(entry, duration, ambienceGeneration));
            return true;
        }

        public void StopAmbience(float fadeSeconds = -1f)
        {
            EnsureInitialized();
            ambienceGeneration++;
            if (ambienceRoutine != null)
                StopCoroutine(ambienceRoutine);

            var duration = fadeSeconds < 0f ? settings.DefaultCrossfadeSeconds : Mathf.Max(0f, fadeSeconds);
            ambienceRoutine = StartCoroutine(FadeOutAndStop(ambienceSource, duration));
        }

        public bool PlayOneShot(string key)
        {
            if (!TryGetEntry(key, null, out var entry))
                return false;

            if (entry.Channel == SoundChannel.Bgm || entry.Channel == SoundChannel.Ambience)
            {
                Debug.LogWarning($"[SoundSystem] '{key}' must be played with its channel-specific API.");
                return false;
            }

            if (CountPlaying(key) >= entry.MaxSimultaneous)
                return false;

            var voice = GetAvailableVoice();
            if (voice == null)
                return false;

            ConfigureSource(voice.Source, entry.Channel);
            voice.Key = key;
            voice.Source.clip = entry.Clip;
            voice.Source.volume = entry.Volume;
            voice.Source.pitch = entry.RandomPitch;
            voice.Source.loop = false;
            voice.Source.Play();
            return true;
        }

        public void StopAllOneShots()
        {
            foreach (var voice in voices)
            {
                voice.Source.Stop();
                voice.Key = null;
            }
        }

        public void SetVolume(SoundChannel channel, float linearVolume, bool save = true)
        {
            EnsureInitialized();
            var value = Mathf.Clamp01(linearVolume);
            volumes[channel] = value;
            ApplyMixerVolume(channel, value);

            if (save)
                volumeStore.Save(channel, value);
        }

        public float GetVolume(SoundChannel channel)
        {
            EnsureInitialized();
            return volumes.TryGetValue(channel, out var value) ? value : 1f;
        }

        private bool TryGetEntry(string key, SoundChannel? requiredChannel, out SoundEntry entry)
        {
            EnsureInitialized();
            entry = null;
            if (string.IsNullOrWhiteSpace(key) || !entries.TryGetValue(key, out entry))
            {
                Debug.LogWarning($"[SoundSystem] Unknown sound key: '{key}'.");
                return false;
            }

            if (requiredChannel.HasValue && entry.Channel != requiredChannel.Value)
            {
                Debug.LogWarning($"[SoundSystem] '{key}' is {entry.Channel}, expected {requiredChannel.Value}.");
                return false;
            }

            return true;
        }

        private void EnsureInitialized()
        {
            if (!initialized)
                throw new System.InvalidOperationException("SoundService.Initialize(settings) must be called before playback.");
        }

        private void CreatePersistentSources()
        {
            if (bgmSources == null)
            {
                bgmSources = new[]
                {
                    CreateSource("BGM A", SoundChannel.Bgm),
                    CreateSource("BGM B", SoundChannel.Bgm)
                };
                ambienceSource = CreateSource("Ambience", SoundChannel.Ambience);
            }
            else
            {
                ConfigureSource(bgmSources[0], SoundChannel.Bgm);
                ConfigureSource(bgmSources[1], SoundChannel.Bgm);
                ConfigureSource(ambienceSource, SoundChannel.Ambience);
            }
        }

        private void CreateInitialVoicePool()
        {
            while (voices.Count < settings.InitialOneShotPoolSize)
                voices.Add(new Voice { Source = CreateSource($"One Shot {voices.Count + 1}", SoundChannel.Se) });
        }

        private AudioSource CreateSource(string sourceName, SoundChannel channel)
        {
            var child = new GameObject(sourceName);
            child.transform.SetParent(transform, false);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            ConfigureSource(source, channel);
            return source;
        }

        private void ConfigureSource(AudioSource source, SoundChannel channel)
        {
            source.outputAudioMixerGroup = settings != null ? settings.GetOutput(channel) : null;
            source.playOnAwake = false;
        }

        private Voice GetAvailableVoice()
        {
            foreach (var voice in voices)
            {
                if (voice.Source.isPlaying)
                    continue;
                voice.Key = null;
                return voice;
            }

            if (voices.Count >= settings.MaximumOneShotPoolSize)
                return null;

            var created = new Voice { Source = CreateSource($"One Shot {voices.Count + 1}", SoundChannel.Se) };
            voices.Add(created);
            return created;
        }

        private int CountPlaying(string key)
        {
            var count = 0;
            foreach (var voice in voices)
                if (voice.Source.isPlaying && voice.Key == key)
                    count++;
            return count;
        }

        private IEnumerator CrossfadeBgm(SoundEntry entry, float duration, int generation)
        {
            var oldSource = bgmSources[activeBgmIndex];
            activeBgmIndex = 1 - activeBgmIndex;
            var newSource = bgmSources[activeBgmIndex];

            ConfigureForMusic(newSource, entry);
            newSource.volume = duration <= 0f ? entry.Volume : 0f;
            newSource.Play();

            if (duration > 0f)
            {
                var elapsed = 0f;
                var oldStartVolume = oldSource.volume;
                while (elapsed < duration && generation == bgmGeneration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    var t = Mathf.Clamp01(elapsed / duration);
                    oldSource.volume = Mathf.Lerp(oldStartVolume, 0f, t);
                    newSource.volume = Mathf.Lerp(0f, entry.Volume, t);
                    yield return null;
                }
            }

            oldSource.Stop();
            oldSource.clip = null;
            newSource.volume = entry.Volume;

            if (generation == bgmGeneration && entry.PlaybackMode == SoundPlaybackMode.RepeatWithInterval)
                yield return RepeatMusic(newSource, entry, generation, true);
        }

        private IEnumerator ChangeAmbience(SoundEntry entry, float duration, int generation)
        {
            yield return FadeOutAndStop(ambienceSource, duration * 0.5f);
            if (generation != ambienceGeneration)
                yield break;

            ConfigureForMusic(ambienceSource, entry);
            ambienceSource.volume = 0f;
            ambienceSource.Play();

            var fadeIn = duration * 0.5f;
            if (fadeIn <= 0f)
                ambienceSource.volume = entry.Volume;
            else
            {
                var elapsed = 0f;
                while (elapsed < fadeIn && generation == ambienceGeneration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    ambienceSource.volume = Mathf.Lerp(0f, entry.Volume, Mathf.Clamp01(elapsed / fadeIn));
                    yield return null;
                }
            }

            ambienceSource.volume = entry.Volume;
            if (generation == ambienceGeneration && entry.PlaybackMode == SoundPlaybackMode.RepeatWithInterval)
                yield return RepeatMusic(ambienceSource, entry, generation, false);
        }

        private static void ConfigureForMusic(AudioSource source, SoundEntry entry)
        {
            source.clip = entry.Clip;
            source.volume = entry.Volume;
            source.pitch = 1f;
            source.loop = entry.PlaybackMode == SoundPlaybackMode.Loop;
        }

        private IEnumerator RepeatMusic(AudioSource source, SoundEntry entry, int generation, bool bgm)
        {
            while ((bgm && generation == bgmGeneration) || (!bgm && generation == ambienceGeneration))
            {
                while (source.isPlaying)
                    yield return null;

                yield return new WaitForSecondsRealtime(entry.RandomRepeatInterval);

                if ((bgm && generation != bgmGeneration) || (!bgm && generation != ambienceGeneration))
                    yield break;

                source.Play();
            }
        }

        private static IEnumerator FadeOutAndStop(AudioSource source, float duration)
        {
            if (source == null)
                yield break;

            if (duration > 0f && source.isPlaying)
            {
                var startVolume = source.volume;
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    source.volume = Mathf.Lerp(startVolume, 0f, Mathf.Clamp01(elapsed / duration));
                    yield return null;
                }
            }

            source.Stop();
            source.clip = null;
            source.volume = 1f;
        }

        private void ApplyMixerVolume(SoundChannel channel, float value)
        {
            var parameter = settings.GetVolumeParameter(channel);
            AudioMixer mixer = settings.Mixer;
            if (mixer != null && !string.IsNullOrWhiteSpace(parameter))
                mixer.SetFloat(parameter, SoundMath.LinearToDecibels(value));
        }

        private void StopAllSources()
        {
            if (bgmSources != null)
                foreach (var source in bgmSources)
                    if (source != null)
                        source.Stop();

            if (ambienceSource != null)
                ambienceSource.Stop();

            foreach (var voice in voices)
                if (voice.Source != null)
                    voice.Source.Stop();
        }
    }
}
