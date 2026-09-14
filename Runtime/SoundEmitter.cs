using UnityEngine;

namespace Ftg.SoundSystem
{
    public enum SoundEmitterMode
    {
        TwoDimensional,
        AtPosition,
        Attached
    }

    [DisallowMultipleComponent]
    public sealed class SoundEmitter : MonoBehaviour
    {
        [SerializeField] private string soundKey;
        [SerializeField] private SoundEmitterMode mode = SoundEmitterMode.AtPosition;
        [SerializeField] private Transform origin;
        [SerializeField] private SpatialProfile spatialProfile;
        [SerializeField] private bool playOnEnable;

        public string SoundKey
        {
            get => soundKey;
            set => soundKey = value;
        }

        private Transform Origin => origin != null ? origin : transform;

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        public void Play()
        {
            switch (mode)
            {
                case SoundEmitterMode.TwoDimensional:
                    SoundService.Instance.PlayOneShot(soundKey);
                    break;
                case SoundEmitterMode.AtPosition:
                    SoundService.Instance.PlayOneShotAt(soundKey, Origin.position, spatialProfile);
                    break;
                case SoundEmitterMode.Attached:
                    SoundService.Instance.PlayOneShotAttached(soundKey, Origin, spatialProfile);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        // Animation Events can pass a key without creating one component per clip.
        public void Play(string key)
        {
            var previousKey = soundKey;
            soundKey = key;
            Play();
            soundKey = previousKey;
        }
    }
}
