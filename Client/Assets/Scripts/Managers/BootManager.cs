using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Boot scene manager responsible for initializing all game managers
    /// and transitioning to the main menu.
    /// This should be the first scene loaded when the game starts.
    /// </summary>
    public class BootManager : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Optional: Splash Screen")]
        [SerializeField] private float splashDuration = 1f;
        [SerializeField] private bool showSplash = false;

        [Header("Debug")]
        [SerializeField] private bool logInitialization = true;

        private void Start()
        {
            if (logInitialization)
                Debug.Log("[BootManager] Starting initialization...");

            VerifyManagers();

            if (showSplash)
                StartCoroutine(WaitAndLoadMainMenu());
            else
                LoadMainMenu();
        }

        private void VerifyManagers()
        {
            // Check GameManager
            if (GameManager.Instance != null)
            {
                if (logInitialization) Debug.Log("[BootManager] ✓ GameManager ready");
            }
            else
            {
                Debug.LogError("[BootManager] ✗ GameManager not found!");
            }

            // Check ExerciseManager
            if (ExerciseManager.Instance != null)
            {
                if (logInitialization) Debug.Log("[BootManager] ✓ ExerciseManager ready");
            }
            else
            {
                Debug.LogError("[BootManager] ✗ ExerciseManager not found!");
            }
        }

        private System.Collections.IEnumerator WaitAndLoadMainMenu()
        {
            if (logInitialization)
                Debug.Log($"[BootManager] Showing splash for {splashDuration}s...");

            yield return new WaitForSeconds(splashDuration);

            LoadMainMenu();
        }

        private void LoadMainMenu()
        {
            if (logInitialization)
                Debug.Log($"[BootManager] Loading {mainMenuSceneName}...");

            if (GameManager.Instance?.sceneManager != null)
            {
                GameManager.Instance.sceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
