using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Wrapper for Unity's SceneManagement to handle scene loading.
    /// Provides a simplified interface for scene transitions.
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        /// <summary>
        /// Loads a scene by name.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        public void LoadScene(string sceneName)
        {
            Debug.Log($"Loading scene: {sceneName}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
