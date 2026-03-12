using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Main menu UI controller.
    /// Handles button clicks, level selection, and gamification stat display.
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

        [Header("Gamification Display")]
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private Slider xpProgressBar;
        [SerializeField] private TextMeshProUGUI xpProgressText;
        [SerializeField] private TextMeshProUGUI coinCountText;
        [SerializeField] private TextMeshProUGUI streakText;

        private void Start()
        {
            SetupButtons();
            RefreshGamificationDisplay();
        }

        private void OnEnable()
        {
            // Refresh stats every time the menu becomes visible (e.g. returning from a level)
            RefreshGamificationDisplay();
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
