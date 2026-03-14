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

        private const string KEY_MUSIC_VOLUME = "Audio_MusicVolume";
        private const string KEY_SFX_VOLUME = "Audio_SfxVolume";
        private const string KEY_MUTED = "Audio_Muted";

        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private bool _isMuted;

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
        /// Plays a sound effect by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        public void PlaySound(string soundName)
        {
            Debug.Log($"[AudioManager] Playing sound: {soundName}");
            // TODO: Implement actual audio playback using AudioSource
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
    }
}
