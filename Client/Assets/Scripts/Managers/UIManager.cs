using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    /// <summary>
    /// Manages all UI elements and updates them based on game state.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Question UI Elements")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Image questionImage;
        [SerializeField] private List<Button> answerButtons;
        [SerializeField] private List<TextMeshProUGUI> answerButtonTexts;

        [Header("Progress UI Elements")]
        [SerializeField] private Slider progressBar;

        [Header("Lives UI Elements")]
        [SerializeField] private List<GameObject> lifeHearts;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button gameOverRetryButton;
        [SerializeField] private Button gameOverMenuButton;

        [Header("Level Summary")]
        [SerializeField] private LevelSummaryController levelSummaryController;

        [Header("Quiz Panel (to hide on game end)")]
        [SerializeField] private GameObject quizPanel;

        private List<Image> _answerButtonImages;

        /// <summary>
        /// Initializes cached button image references to avoid expensive GetComponent calls.
        /// </summary>
        private void Awake()
        {
            CacheButtonImages();
        }

        /// <summary>
        /// Caches Image components from answer buttons for performance optimization.
        /// </summary>
        private void CacheButtonImages()
        {
            if (answerButtons == null) return;

            _answerButtonImages = new List<Image>();
            foreach (Button button in answerButtons)
            {
                Image buttonImage = button.GetComponent<Image>();
                _answerButtonImages.Add(buttonImage);
            }
        }

        /// <summary>
        /// Registers this UIManager with the GameManager.
        /// </summary>
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.uiManager = this;
                Debug.Log("[UIManager] Registered with GameManager.");
            }
            else
            {
                Debug.LogWarning("[UIManager] GameManager.Instance is null!");
            }
        }

        public void ShowMainMenu()
        {
            Debug.Log("ShowMainMenu");
            // Return to main menu scene
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
        }

        /// <summary>
        /// Shows the Game Over panel with retry and menu options.
        /// Auto-discovers buttons by name if not assigned in the Inspector.
        /// </summary>
        public void ShowGameOverScreen()
        {
            Debug.Log("[UIManager] Displaying Game Over screen");

            if (quizPanel != null)
            {
                quizPanel.SetActive(false);
            }

            if (gameOverPanel == null)
            {
                Debug.LogWarning("[UIManager] GameOverPanel is not assigned!");
                return;
            }

            gameOverPanel.SetActive(true);

            // Auto-discover buttons if not assigned in Inspector
            if (gameOverRetryButton == null)
                gameOverRetryButton = FindButtonInPanel(gameOverPanel, "RetryButton");
            if (gameOverMenuButton == null)
                gameOverMenuButton = FindButtonInPanel(gameOverPanel, "MenuButton");

            if (gameOverRetryButton != null)
            {
                gameOverRetryButton.onClick.RemoveAllListeners();
                gameOverRetryButton.onClick.AddListener(() => GameManager.Instance.RestartCurrentLevel());
            }
            else
            {
                Debug.LogWarning("[UIManager] GameOver RetryButton not found!");
            }

            if (gameOverMenuButton != null)
            {
                gameOverMenuButton.onClick.RemoveAllListeners();
                gameOverMenuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMenu());
            }
            else
            {
                Debug.LogWarning("[UIManager] GameOver MenuButton not found!");
            }
        }

        /// <summary>
        /// Finds a Button component in a panel by GameObject name.
        /// </summary>
        private Button FindButtonInPanel(GameObject panel, string buttonName)
        {
            Transform found = panel.transform.Find(buttonName);
            if (found != null)
                return found.GetComponent<Button>();
            return null;
        }

        /// <summary>
        /// Shows the level summary screen with animated star reveal,
        /// score count-up, gamification rewards, and navigation options.
        /// </summary>
        /// <param name="score">The player's score for this level.</param>
        /// <param name="starsEarned">Number of stars earned (1-3).</param>
        /// <param name="hasNextLevel">Whether there's a next level available.</param>
        /// <param name="rewards">Gamification rewards earned from this level.</param>
        public void ShowVictoryScreen(int score, int starsEarned, bool hasNextLevel, LevelRewards rewards = default)
        {
            Debug.Log($"[UIManager] Displaying level summary — Score: {score}, Stars: {starsEarned}");

            // Hide quiz panel
            if (quizPanel != null)
            {
                quizPanel.SetActive(false);
            }

            if (levelSummaryController != null)
            {
                levelSummaryController.Show(score, starsEarned, hasNextLevel, rewards);
            }
            else
            {
                Debug.LogWarning("[UIManager] LevelSummaryController is not assigned!");
            }
        }

        /// <summary>
        /// Hides all end-game panels and shows the quiz panel.
        /// Called when starting or restarting a level.
        /// </summary>
        public void ResetToQuizView()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (levelSummaryController != null) levelSummaryController.Hide();
            if (quizPanel != null) quizPanel.SetActive(true);
        }

        /// <summary>
        /// Highlights an answer button with a color based on whether it's correct or incorrect.
        /// </summary>
        /// <param name="index">The index of the answer button to highlight.</param>
        /// <param name="isCorrect">True if the answer is correct, false otherwise.</param>
        public void HighlightAnswer(int index, bool isCorrect)
        {
            if (_answerButtonImages == null || index < 0 || index >= _answerButtonImages.Count)
            {
                Debug.LogError($"Cannot highlight button at index {index}");
                return;
            }

            Image buttonImage = _answerButtonImages[index];

            if (buttonImage)
            {
                buttonImage.color = isCorrect ? Color.green : Color.red;
            }
            else
            {
                Debug.LogWarning($"Button at index {index} has no Image component");
            }
        }

        /// <summary>
        /// Resets all answer buttons to their default color.
        /// </summary>
        public void ResetAnswerColors()
        {
            if (_answerButtonImages == null) return;

            foreach (Image buttonImage in _answerButtonImages)
            {
                if (buttonImage)
                {
                    buttonImage.color = Color.white;
                }
            }
        }

        /// <summary>
        /// Enables or disables all answer buttons to prevent multiple clicks.
        /// </summary>
        /// <param name="interactable">True to enable buttons, false to disable them.</param>
        public void SetAnswerButtonsInteractable(bool interactable)
        {
            if (answerButtons == null) return;

            foreach (Button button in answerButtons)
            {
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }

        /// <summary>
        /// Updates the progress bar to reflect the player's advancement through the level.
        /// </summary>
        /// <param name="progress">A value between 0.0 and 1.0 representing the completion percentage.</param>
        public void UpdateProgressBar(float progress)
        {
            if (progressBar)
            {
                progressBar.value = progress;
                Debug.Log($"Progress bar updated: {progress * 100:F1}%");
            }
            else
            {
                Debug.LogWarning("ProgressBar is not assigned in UIManager!");
            }
        }

        /// <summary>
        /// Updates the lives display by activating/deactivating heart icons based on current lives.
        /// </summary>
        /// <param name="currentLives">The number of lives the player currently has.</param>
        public void UpdateLivesUI(int currentLives)
        {
            if (lifeHearts == null || lifeHearts.Count == 0)
            {
                Debug.LogWarning("No life hearts are assigned in UIManager!");
                return;
            }

            for (int i = 0; i < lifeHearts.Count; i++)
            {
                if (lifeHearts[i])
                {
                    // Activate heart if index is less than current lives
                    lifeHearts[i].SetActive(i < currentLives);
                }
            }

            Debug.Log($"Vies restantes : {currentLives}/{lifeHearts.Count}");
        }
    }
}
