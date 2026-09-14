using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Ftg.SoundSystem
{
    [CreateAssetMenu(fileName = "SoundSystemSettings", menuName = "FTG/Sound System/Settings")]
    public sealed class SoundSystemSettings : ScriptableObject
    {
        [Serializable]
        public sealed class MixerBinding
        {
            [SerializeField] private SoundChannel channel;
            [SerializeField] private AudioMixerGroup output;
            [SerializeField] private string exposedVolumeParameter;

            public SoundChannel Channel => channel;
            public AudioMixerGroup Output => output;
            public string ExposedVolumeParameter => exposedVolumeParameter;
        }

        [SerializeField] private SoundCatalog catalog;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private SpatialProfile defaultSpatialProfile;
        [SerializeField] private MixerBinding[] mixerBindings = Array.Empty<MixerBinding>();
        [SerializeField, Min(1)] private int initialOneShotPoolSize = 8;
        [SerializeField, Min(1)] private int maximumOneShotPoolSize = 32;
        [SerializeField, Min(0f)] private float defaultCrossfadeSeconds = 1f;
        [SerializeField] private string playerPrefsPrefix = "Ftg.SoundSystem";

        public SoundCatalog Catalog => catalog;
        public AudioMixer Mixer => mixer;
        public SpatialProfile DefaultSpatialProfile => defaultSpatialProfile;
        public int InitialOneShotPoolSize => Mathf.Max(1, initialOneShotPoolSize);
        public int MaximumOneShotPoolSize => Mathf.Max(InitialOneShotPoolSize, maximumOneShotPoolSize);
        public float DefaultCrossfadeSeconds => Mathf.Max(0f, defaultCrossfadeSeconds);
        public string PlayerPrefsPrefix => string.IsNullOrWhiteSpace(playerPrefsPrefix) ? "Ftg.SoundSystem" : playerPrefsPrefix;

        public AudioMixerGroup GetOutput(SoundChannel channel)
        {
            foreach (var binding in mixerBindings)
                if (binding != null && binding.Channel == channel)
                    return binding.Output;
            return null;
        }

        public string GetVolumeParameter(SoundChannel channel)
        {
            foreach (var binding in mixerBindings)
                if (binding != null && binding.Channel == channel)
                    return binding.ExposedVolumeParameter;
            return null;
        }
    }
}
