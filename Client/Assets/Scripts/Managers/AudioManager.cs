using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Manages audio playback for the game including music and sound effects.
    /// Provides volume control and mute functionality with PlayerPrefs persistence.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [Tooltip("AudioSource used for background music playback")]
        [SerializeField] private AudioSource musicSource;

        [Tooltip("AudioSource used for sound effects playback")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Sound Library")]
        [Tooltip("ScriptableObject containing all sound entries")]
        [SerializeField] private AudioLibraryData audioLibrary;

        private const string KEY_MUSIC_VOLUME = "Audio_MusicVolume";
        private const string KEY_SFX_VOLUME = "Audio_SfxVolume";
        private const string KEY_MUTED = "Audio_Muted";

        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private bool _isMuted;

        private Dictionary<string, SoundEntry> _soundLookup;

        /// <summary>Current music volume (0 to 1).</summary>
        public float MusicVolume => _musicVolume;

        /// <summary>Current SFX volume (0 to 1).</summary>
        public float SfxVolume => _sfxVolume;

        /// <summary>Whether all audio is muted.</summary>
        public bool IsMuted => _isMuted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                if (transform.parent != null)
                {
                    transform.SetParent(null);
                    Debug.LogWarning("[AudioManager] Was not at root level. Moved automatically.");
                }

                DontDestroyOnLoad(gameObject);
                LoadSettingsFromPrefs();
                BuildSoundLookup();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Sets the music volume and persists the setting.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
            SaveSettingsToPrefs();
        }

        /// <summary>
        /// Sets the SFX volume and persists the setting.
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
            SaveSettingsToPrefs();
        }

        /// <summary>
        /// Sets the muted state and persists the setting.
        /// </summary>
        public void SetMuted(bool muted)
        {
            _isMuted = muted;
            ApplyVolumes();
            SaveSettingsToPrefs();
        }

        /// <summary>
        /// Plays a sound by name, routing to SFX or music based on the sound entry configuration.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        public void PlaySound(string soundName)
        {
            if (_soundLookup == null)
            {
                Debug.LogWarning($"[AudioManager] Sound lookup not initialized. Cannot play \"{soundName}\".");
                return;
            }

            if (!_soundLookup.TryGetValue(soundName, out SoundEntry entry))
            {
                Debug.LogWarning($"[AudioManager] Sound \"{soundName}\" not found in audio library.");
                return;
            }

            if (entry.clip == null)
            {
                Debug.LogWarning($"[AudioManager] No clip assigned for sound \"{soundName}\".");
                return;
            }

            if (entry.isMusic)
                PlayMusic(entry.clip);
            else
                PlaySFX(entry.clip);
        }

        /// <summary>
        /// Plays an audio clip as a one-shot sound effect.
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Plays an audio clip as looping background music. Skips if the same clip is already playing.
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        /// <summary>
        /// Stops the currently playing music and clears the clip.
        /// </summary>
        public void StopMusic()
        {
            if (musicSource == null) return;
            musicSource.Stop();
            musicSource.clip = null;
        }

        private void BuildSoundLookup()
        {
            _soundLookup = new Dictionary<string, SoundEntry>();

            if (audioLibrary == null)
            {
                Debug.LogWarning("[AudioManager] No AudioLibrary assigned. PlaySound will not work.");
                return;
            }

            foreach (SoundEntry entry in audioLibrary.sounds)
            {
                if (string.IsNullOrEmpty(entry.name))
                {
                    Debug.LogWarning("[AudioManager] Sound entry with empty name found in audio library. Skipping.");
                    continue;
                }

                if (!_soundLookup.TryAdd(entry.name, entry))
                {
                    Debug.LogWarning($"[AudioManager] Duplicate sound name \"{entry.name}\" in audio library. Keeping first entry.");
                }
            }

            Debug.Log($"[AudioManager] Sound lookup built with {_soundLookup.Count} entries.");
        }

        private void ApplyVolumes()
        {
            if (musicSource != null)
            {
                musicSource.volume = _isMuted ? 0f : _musicVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = _isMuted ? 0f : _sfxVolume;
            }
        }

        private void LoadSettingsFromPrefs()
        {
            _musicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);
            _isMuted = PlayerPrefs.GetInt(KEY_MUTED, 0) == 1;
            ApplyVolumes();

            Debug.Log($"[AudioManager] Settings loaded — Music: {_musicVolume}, SFX: {_sfxVolume}, Muted: {_isMuted}");
        }

        private void SaveSettingsToPrefs()
        {
            PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, _musicVolume);
            PlayerPrefs.SetFloat(KEY_SFX_VOLUME, _sfxVolume);
            PlayerPrefs.SetInt(KEY_MUTED, _isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (musicSource == null) return;
            if (pauseStatus)
                musicSource.Pause();
            else
                musicSource.UnPause();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (musicSource == null) return;
            if (!hasFocus)
                musicSource.Pause();
            else
                musicSource.UnPause();
        }
    }
}
