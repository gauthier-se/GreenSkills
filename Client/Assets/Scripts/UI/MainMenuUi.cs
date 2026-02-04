using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Main menu UI controller.
    /// Handles button clicks and level selection.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Level Selection")]
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private bool autoSelectHighestLevel = false;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClick);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClick);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClick);
        }

        /// <summary>
        /// Called when the Play button is clicked.
        /// </summary>
        public void OnPlayClick()
        {
            int levelToLoad = startingLevel;

            if (autoSelectHighestLevel && GameManager.Instance != null)
            {
                levelToLoad = GameManager.Instance.GetHighestLevelUnlocked();
            }

            Debug.Log($"[MainMenuUI] Loading level {levelToLoad}");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadLevel($"local://level/{levelToLoad}");
            }
            else
            {
                Debug.LogError("[MainMenuUI] GameManager.Instance is null!");
            }
        }

        /// <summary>
        /// Called when the Settings button is clicked.
        /// </summary>
        public void OnSettingsClick()
        {
            Debug.Log("[MainMenuUI] Settings clicked");
            // TODO: Implement settings panel
        }

        /// <summary>
        /// Called when the Quit button is clicked.
        /// </summary>
        public void OnQuitClick()
        {
            Debug.Log("[MainMenuUI] Quit clicked");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Loads a specific level (for level selection UI).
        /// </summary>
        public void LoadLevel(int levelId)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[MainMenuUI] GameManager.Instance is null!");
                return;
            }

            if (!GameManager.Instance.IsLevelUnlocked(levelId))
            {
                Debug.LogWarning($"[MainMenuUI] Level {levelId} is locked!");
                return;
            }

            GameManager.Instance.LoadLevel($"local://level/{levelId}");
        }
    }
}
