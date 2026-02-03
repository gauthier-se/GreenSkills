using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    /// <summary>
    /// Manages the main menu interface, particularly the level selection grid.
    /// Handles level button states based on player progression.
    /// Supports both simple Button components and advanced LevelButton components.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Level Selection - Simple Mode")]
        [Tooltip("Use this for basic setup with standard Unity Buttons")]
        [SerializeField] private List<Button> levelButtons;

        [Header("Level Selection - Advanced Mode (Optional)")]
        [Tooltip("Use this for custom LevelButton components with lock icons, stars, etc.")]
        [SerializeField] private List<UI.LevelButton> advancedLevelButtons;

        [Header("Button Colors (Simple Mode Only)")]
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
        [SerializeField] private Color unlockedColor = Color.white;

        [Header("API Configuration")]
        [Tooltip("API endpoint URL. Use 'local://level/' for mock data from Resources/Data/levels_data.json")]
        [SerializeField] private string apiBaseUrl = "local://level/";

        /// <summary>
        /// Initializes the level selection UI based on player progression.
        /// </summary>
        private void Start()
        {
            InitializeLevelButtons();
        }

        /// <summary>
        /// Configures each level button based on whether it's unlocked or locked.
        /// Supports both simple Button mode and advanced LevelButton mode.
        /// </summary>
        private void InitializeLevelButtons()
        {
            // Check which mode to use
            bool useAdvancedMode = advancedLevelButtons != null && advancedLevelButtons.Count > 0;
            bool useSimpleMode = levelButtons != null && levelButtons.Count > 0;

            if (!useAdvancedMode && !useSimpleMode)
            {
                Debug.LogWarning("No level buttons are assigned in MainMenuManager!");
                return;
            }

            if (useAdvancedMode)
            {
                InitializeAdvancedButtons();
            }
            else
            {
                InitializeSimpleButtons();
            }
        }

        /// <summary>
        /// Initializes simple Unity Button components.
        /// </summary>
        private void InitializeSimpleButtons()
        {
            for (int i = 0; i < levelButtons.Count; i++)
            {
                Button button = levelButtons[i];

                if (button == null)
                {
                    Debug.LogWarning($"Button at index {i} is null!");
                    continue;
                }

                int levelId = i + 1;
                bool isUnlocked = GameManager.Instance.IsLevelUnlocked(levelId);

                button.interactable = isUnlocked;

                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = isUnlocked ? unlockedColor : lockedColor;
                }

                if (isUnlocked)
                {
                    button.onClick.RemoveAllListeners();
                    int capturedLevelId = levelId;
                    button.onClick.AddListener(() => LoadLevel(capturedLevelId));
                    Debug.Log($"Level {levelId} unlocked and ready to play");
                }
                else
                {
                    Debug.Log($"Level {levelId} locked");
                }
            }
        }

        /// <summary>
        /// Initializes advanced LevelButton components with additional features.
        /// </summary>
        private void InitializeAdvancedButtons()
        {
            for (int i = 0; i < advancedLevelButtons.Count; i++)
            {
                UI.LevelButton levelButton = advancedLevelButtons[i];

                if (levelButton == null)
                {
                    Debug.LogWarning($"Advanced button at index {i} is null!");
                    continue;
                }

                int levelId = i + 1;
                bool isUnlocked = GameManager.Instance.IsLevelUnlocked(levelId);

                levelButton.Initialize(levelId, isUnlocked, LoadLevel);

                Debug.Log($"Level {levelId} {(isUnlocked ? "unlocked" : "locked")}");
            }
        }

        /// <summary>
        /// Loads a specific level by ID through the GameManager.
        /// </summary>
        /// <param name="levelId">The ID of the level to load.</param>
        private void LoadLevel(int levelId)
        {
            Debug.Log($"Loading level {levelId}...");

            // Force local mode for MVP - use mock data from Resources/Data/levels_data.json
            // This ensures the game works offline without an API server
            const string localApiBaseUrl = "local://level/";
            string apiUrl = localApiBaseUrl + levelId;

            Debug.Log($"Loading URL: {apiUrl}");

            // Tell GameManager to load the level from API
            // GameManager.LoadLevel() will automatically call StartGame() once data is loaded
            GameManager.Instance.LoadLevel(apiUrl);
        }

        /// <summary>
        /// Refreshes the level button states (useful after returning from a level).
        /// Call this in OnEnable() if you want to update buttons when the menu is shown again.
        /// </summary>
        public void RefreshLevelButtons()
        {
            InitializeLevelButtons();
        }

        /// <summary>
        /// Resets player progression (for testing/debugging purposes).
        /// </summary>
        public void ResetProgressButton()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetProgress();
                RefreshLevelButtons();
                Debug.Log("Progress reset and buttons updated!");
            }
        }
    }
}
