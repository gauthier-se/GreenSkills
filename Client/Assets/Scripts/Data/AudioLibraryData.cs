using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "GreenSkills/Audio Library")]
    public class AudioLibraryData : ScriptableObject
    {
        public List<SoundEntry> sounds = new List<SoundEntry>();
    }

    [Serializable]
    public struct SoundEntry
    {
        [Tooltip("Lookup key used by PlaySound (e.g. \"Correct\", \"GameOverMusic\")")]
        public string name;

        [Tooltip("The AudioClip to play")]
        public AudioClip clip;

        [Tooltip("If true, plays on musicSource with looping; otherwise plays as a one-shot SFX")]
        public bool isMusic;
    }
}
