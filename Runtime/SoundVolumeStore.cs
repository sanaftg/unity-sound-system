using UnityEngine;

namespace Ftg.SoundSystem
{
    internal sealed class SoundVolumeStore
    {
        private readonly string prefix;

        public SoundVolumeStore(string prefix)
        {
            this.prefix = prefix;
        }

        public float Load(SoundChannel channel)
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(GetKey(channel), 1f));
        }

        public void Save(SoundChannel channel, float value)
        {
            PlayerPrefs.SetFloat(GetKey(channel), Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }

        private string GetKey(SoundChannel channel) => $"{prefix}.Volume.{channel}";
    }
}
