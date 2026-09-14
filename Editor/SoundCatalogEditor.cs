using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Ftg.SoundSystem.Editor
{
    [CustomEditor(typeof(SoundCatalog))]
    public sealed class SoundCatalogEditor : UnityEditor.Editor
    {
        private SoundChannel channel = SoundChannel.Se;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Batch Registration", EditorStyles.boldLabel);
            channel = (SoundChannel)EditorGUILayout.EnumPopup("Channel", channel);

            var dropArea = GUILayoutUtility.GetRect(0f, 52f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drop AudioClips Here");
            HandleDrop(dropArea);

            EditorGUILayout.HelpBox(
                "Drag multiple AudioClip assets from the Project window. Playback API selection determines 2D or 3D.",
                MessageType.Info);
        }

        private void HandleDrop(Rect dropArea)
        {
            var current = Event.current;
            if (!dropArea.Contains(current.mousePosition) ||
                (current.type != EventType.DragUpdated && current.type != EventType.DragPerform))
                return;

            var clips = GetDraggedClips();
            DragAndDrop.visualMode = clips.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            if (current.type == EventType.DragPerform && clips.Count > 0)
            {
                DragAndDrop.AcceptDrag();
                AddClips(clips);
            }

            current.Use();
        }

        private void AddClips(IReadOnlyList<AudioClip> clips)
        {
            serializedObject.Update();
            var entries = serializedObject.FindProperty("entries");
            var usedKeys = CollectKeys(entries);

            foreach (var clip in clips)
            {
                if (ContainsClip(entries, clip))
                    continue;

                entries.InsertArrayElementAtIndex(entries.arraySize);
                var entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                ResetEntry(entry, clip, CreateUniqueKey(usedKeys, clip.name));
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private string CreateUniqueKey(HashSet<string> usedKeys, string clipName)
        {
            var baseKey = $"{channel.ToString().ToLowerInvariant()}.{Normalize(clipName)}";
            var key = baseKey;
            var suffix = 2;
            while (!usedKeys.Add(key))
                key = $"{baseKey}.{suffix++}";
            return key;
        }

        private void ResetEntry(SerializedProperty entry, AudioClip clip, string key)
        {
            entry.FindPropertyRelative("key").stringValue = key;
            entry.FindPropertyRelative("clip").objectReferenceValue = clip;
            entry.FindPropertyRelative("channel").enumValueIndex = (int)channel;
            entry.FindPropertyRelative("playbackMode").enumValueIndex = (int)SoundPlaybackMode.OneShot;
            entry.FindPropertyRelative("volume").floatValue = 1f;
            entry.FindPropertyRelative("minPitch").floatValue = 1f;
            entry.FindPropertyRelative("maxPitch").floatValue = 1f;
            entry.FindPropertyRelative("minRepeatInterval").floatValue = 5f;
            entry.FindPropertyRelative("maxRepeatInterval").floatValue = 15f;
            entry.FindPropertyRelative("maxSimultaneous").intValue = 4;
        }

        private static bool ContainsClip(SerializedProperty entries, AudioClip clip)
        {
            for (var i = 0; i < entries.arraySize; i++)
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("clip").objectReferenceValue == clip)
                    return true;
            return false;
        }

        private static HashSet<string> CollectKeys(SerializedProperty entries)
        {
            var keys = new HashSet<string>(System.StringComparer.Ordinal);
            for (var i = 0; i < entries.arraySize; i++)
                keys.Add(entries.GetArrayElementAtIndex(i).FindPropertyRelative("key").stringValue);
            return keys;
        }

        private static List<AudioClip> GetDraggedClips()
        {
            var clips = new List<AudioClip>();
            foreach (var dragged in DragAndDrop.objectReferences)
                if (dragged is AudioClip clip)
                    clips.Add(clip);
            return clips;
        }

        private static string Normalize(string value)
        {
            var builder = new StringBuilder(value.Length);
            var separator = false;
            foreach (var character in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                {
                    if (separator && builder.Length > 0)
                        builder.Append('.');
                    builder.Append(character);
                    separator = false;
                }
                else
                {
                    separator = true;
                }
            }
            return builder.Length > 0 ? builder.ToString() : "sound";
        }
    }
}
