using System.Collections.Generic;
using UnityEngine;

namespace Ftg.SoundSystem
{
    [CreateAssetMenu(fileName = "SoundCatalog", menuName = "FTG/Sound System/Sound Catalog")]
    public sealed class SoundCatalog : ScriptableObject
    {
        [SerializeField] private List<SoundEntry> entries = new();

        public IReadOnlyList<SoundEntry> Entries => entries;

        public bool TryBuildLookup(out Dictionary<string, SoundEntry> lookup)
        {
            lookup = new Dictionary<string, SoundEntry>(System.StringComparer.Ordinal);
            var valid = true;

            foreach (var entry in entries)
            {
                if (entry == null || !entry.IsValid || lookup.ContainsKey(entry.Key))
                {
                    valid = false;
                    continue;
                }

                lookup.Add(entry.Key, entry);
            }

            return valid;
        }
    }
}
