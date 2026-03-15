using System.Collections.Generic;
using Data;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Main menu UI controller.
    /// Handles dynamic level button spawning, gamification stat display, and navigation buttons.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Level Selection")]
        [SerializeField] private LevelButton levelButtonPrefab;
        [SerializeField] private Transform levelButtonsContainer;

        [Header("Buttons")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Settings")]
        [SerializeField] private SettingsPanelController settingsPanel;

        [Header("Theme")]
        [SerializeField] private UITheme theme;

        [Header("Gamification Display")]
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private Slider xpProgressBar;
        [SerializeField] private TextMeshProUGUI xpProgressText;
        [SerializeField] private TextMeshProUGUI coinCountText;
        [SerializeField] private TextMeshProUGUI streakText;

        private readonly List<LevelButton> _spawnedButtons = new List<LevelButton>();

        private void Start()
        {
            SetupButtons();
            SpawnLevelButtons();
            RefreshGamificationDisplay();
        }

        private void OnEnable()
        {
            RefreshGamificationDisplay();
            RefreshLevelButtons();
        }

        private void SetupButtons()
        {
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClick);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClick);
        }

        /// <summary>
        /// Dynamically spawns level buttons based on the available level count.
        /// </summary>
        private void SpawnLevelButtons()
        {
            if (levelButtonPrefab == null || levelButtonsContainer == null) return;
            if (ExerciseManager.Instance == null) return;

            ClearSpawnedButtons();

            int levelCount = ExerciseManager.Instance.GetLevelCount();

            for (int i = 0; i < levelCount; i++)
            {
                int levelId = i + 1;
                LevelButton instance = Instantiate(levelButtonPrefab, levelButtonsContainer);
                bool unlocked = GameManager.Instance != null
                    && GameManager.Instance.IsLevelUnlocked(levelId);
                Category cat = ExerciseManager.Instance.GetLevelCategory(levelId);
                instance.Initialize(levelId, unlocked, LoadLevel, cat, theme);
                _spawnedButtons.Add(instance);
            }
        }

        /// <summary>
        /// Refreshes all spawned level buttons with current unlock state and stars.
        /// </summary>
        private void RefreshLevelButtons()
        {
            for (int i = 0; i < _spawnedButtons.Count; i++)
            {
                if (_spawnedButtons[i] == null) continue;

                int levelId = i + 1;
                bool unlocked = GameManager.Instance != null
                    && GameManager.Instance.IsLevelUnlocked(levelId);
                Category cat = ExerciseManager.Instance.GetLevelCategory(levelId);
                _spawnedButtons[i].Initialize(levelId, unlocked, LoadLevel, cat, theme);
            }
        }

        private void ClearSpawnedButtons()
        {
            foreach (var btn in _spawnedButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            _spawnedButtons.Clear();
        }

        #region Gamification Display

        /// <summary>
        /// Updates all gamification UI elements with current values from GamificationManager.
        /// Safe to call even if GamificationManager or UI elements are not assigned.
        /// </summary>
        private void RefreshGamificationDisplay()
        {
            if (GamificationManager.Instance == null)
            {
                return;
            }

            var gm = GamificationManager.Instance;

            if (playerLevelText != null)
            {
                playerLevelText.text = $"Nv. {gm.PlayerLevel}";
            }

            if (xpProgressBar != null)
            {
                xpProgressBar.value = (float)gm.XPInCurrentLevel / gm.XPRequiredForLevel;
            }

            if (xpProgressText != null)
            {
                xpProgressText.text = $"{gm.XPInCurrentLevel} / {gm.XPRequiredForLevel} XP";
            }

            if (coinCountText != null)
            {
                coinCountText.text = $"{gm.EcoCoins}";
            }

            if (streakText != null)
            {
                streakText.text = gm.GetStreakDisplayText();
            }
        }

        #endregion

        /// <summary>
        /// Called when the Settings button is clicked.
        /// </summary>
        public void OnSettingsClick()
        {
            Debug.Log("[MainMenuUI] Settings clicked");

            if (settingsPanel != null)
            {
                settingsPanel.Show();
            }
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
        /// Loads a specific level by ID.
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
