using System;
using UnityEngine;

namespace Ftg.SoundSystem
{
    [Serializable]
    public sealed class SoundEntry
    {
        [SerializeField] private string key;
        [SerializeField] private AudioClip clip;
        [SerializeField] private SoundChannel channel = SoundChannel.Se;
        [SerializeField] private SoundPlaybackMode playbackMode = SoundPlaybackMode.OneShot;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(-3f, 3f)] private float minPitch = 1f;
        [SerializeField, Range(-3f, 3f)] private float maxPitch = 1f;
        [SerializeField, Min(0f)] private float minRepeatInterval = 5f;
        [SerializeField, Min(0f)] private float maxRepeatInterval = 15f;
        [SerializeField, Min(1)] private int maxSimultaneous = 4;

        public string Key => key;
        public AudioClip Clip => clip;
        public SoundChannel Channel => channel;
        public SoundPlaybackMode PlaybackMode => playbackMode;
        public float Volume => volume;
        public int MaxSimultaneous => Mathf.Max(1, maxSimultaneous);
        public float RandomPitch => UnityEngine.Random.Range(Mathf.Min(minPitch, maxPitch), Mathf.Max(minPitch, maxPitch));
        public float RandomRepeatInterval => UnityEngine.Random.Range(
            Mathf.Min(minRepeatInterval, maxRepeatInterval),
            Mathf.Max(minRepeatInterval, maxRepeatInterval));

        public bool IsValid => !string.IsNullOrWhiteSpace(key) && clip != null;
    }
}
