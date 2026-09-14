using UnityEngine;

namespace Ftg.SoundSystem
{
    [CreateAssetMenu(fileName = "SpatialProfile", menuName = "FTG/Sound System/Spatial Profile")]
    public sealed class SpatialProfile : ScriptableObject
    {
        [SerializeField, Min(0f)] private float minDistance = 3f;
        [SerializeField, Min(0f)] private float maxDistance = 35f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        [SerializeField, Range(0f, 5f)] private float dopplerLevel;
        [SerializeField, Range(0f, 360f)] private float spread;

        public float MinDistance => Mathf.Max(0f, minDistance);
        public float MaxDistance => Mathf.Max(MinDistance, maxDistance);
        public AudioRolloffMode RolloffMode => rolloffMode;
        public float DopplerLevel => Mathf.Max(0f, dopplerLevel);
        public float Spread => Mathf.Clamp(spread, 0f, 360f);

        public void ApplyTo(AudioSource source)
        {
            if (source == null)
                throw new System.ArgumentNullException(nameof(source));

            source.spatialBlend = 1f;
            source.minDistance = MinDistance;
            source.maxDistance = MaxDistance;
            source.rolloffMode = RolloffMode;
            source.dopplerLevel = DopplerLevel;
            source.spread = Spread;
        }

        internal static void ApplyDefaults(AudioSource source)
        {
            source.spatialBlend = 1f;
            source.minDistance = 3f;
            source.maxDistance = 35f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.dopplerLevel = 0f;
            source.spread = 0f;
        }
    }
}
