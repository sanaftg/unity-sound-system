using UnityEngine;

namespace Ftg.SoundSystem
{
    [DefaultExecutionOrder(-10000)]
    public sealed class SoundSystemBootstrap : MonoBehaviour
    {
        [SerializeField] private SoundSystemSettings settings;

        private void Awake()
        {
            if (settings == null)
            {
                Debug.LogError("[SoundSystem] SoundSystemSettings is not assigned.", this);
                return;
            }

            if (!SoundService.Instance.IsInitialized)
                SoundService.Instance.Initialize(settings);
        }
    }
}
