using UnityEngine;

namespace Ftg.SoundSystem
{
    public static class SoundMath
    {
        public const float SilenceDecibels = -80f;

        public static float LinearToDecibels(float linear)
        {
            if (linear <= 0.0001f)
                return SilenceDecibels;

            return Mathf.Log10(Mathf.Clamp01(linear)) * 20f;
        }
    }
}
