using UnityEngine;
using UI;

namespace Managers
{
    /// <summary>
    /// Boot scene manager responsible for initializing all game managers
    /// and transitioning to the appropriate scene based on authentication state.
    /// This should be the first scene loaded when the game starts.
    /// </summary>
    public class BootManager : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string loginSceneName = "Login";

        [Header("Optional: Splash Screen")]
        [SerializeField] private float splashDuration = 4.5f;
        [SerializeField] private bool showSplash = true;
        [SerializeField] private BootSplashUI splashUI;

        [Header("Debug")]
        [SerializeField] private bool logInitialization = true;

        private void Start()
        {
            if (logInitialization)
                Debug.Log("[BootManager] Starting initialization...");

            VerifyManagers();

            if (showSplash && splashUI != null)
            {
                splashUI.gameObject.SetActive(true);
                StartCoroutine(WaitAndLoadNextScene());
            }
            else
            {
                LoadNextScene();
            }
        }

        private void VerifyManagers()
        {
            // Check GameManager
            if (GameManager.Instance != null)
            {
                if (logInitialization) Debug.Log("[BootManager] GameManager ready");
            }
            else
            {
                Debug.LogError("[BootManager] GameManager not found!");
            }

            // Check ExerciseManager
            if (ExerciseManager.Instance != null)
            {
                if (logInitialization) Debug.Log("[BootManager] ExerciseManager ready");
            }
            else
            {
                Debug.LogError("[BootManager] ExerciseManager not found!");
            }

            // Check AuthManager
            if (AuthManager.Instance != null)
            {
                if (logInitialization) Debug.Log("[BootManager] AuthManager ready");
            }
            else
            {
                Debug.LogError("[BootManager] AuthManager not found!");
            }

            // Check GamificationManager
            if (GamificationManager.Instance != null)
            {
                if (logInitialization) Debug.Log("[BootManager] GamificationManager ready");
            }
            else
            {
                Debug.LogError("[BootManager] GamificationManager not found!");
            }
        }

        private System.Collections.IEnumerator WaitAndLoadNextScene()
        {
            if (logInitialization)
                Debug.Log($"[BootManager] Showing splash for {splashDuration}s...");

            yield return new WaitForSeconds(splashDuration);

            LoadNextScene();
        }

        /// <summary>
        /// Determines the next scene based on authentication state.
        /// If the user has a saved token, goes straight to MainMenu.
        /// Otherwise, shows the Login screen.
        /// </summary>
        private void LoadNextScene()
        {
            bool isAuthenticated = AuthManager.Instance != null && AuthManager.Instance.TryAutoLogin();

            string targetScene = isAuthenticated ? mainMenuSceneName : loginSceneName;

            if (logInitialization)
                Debug.Log($"[BootManager] Authenticated: {isAuthenticated}. Loading {targetScene}...");

            if (GameManager.Instance?.sceneManager != null)
            {
                GameManager.Instance.sceneManager.LoadScene(targetScene);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
            }
        }
    }
}
