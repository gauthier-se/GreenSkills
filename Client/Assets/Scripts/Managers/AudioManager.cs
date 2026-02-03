using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Manages audio playback for the game including music and sound effects.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        /// <summary>
        /// Plays a sound effect by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        public void PlaySound(string soundName)
        {
            Debug.Log($"Playing sound: {soundName}");
            // TODO: Implement actual audio playback using AudioSource
        }
    }
}
