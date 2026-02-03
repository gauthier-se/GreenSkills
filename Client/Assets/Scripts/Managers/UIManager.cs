using System.Collections.Generic;
using Data;
using TMPro;
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

        [Header("Victory Panel")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TextMeshProUGUI victoryScoreText;
        [SerializeField] private Button victoryNextLevelButton;
        [SerializeField] private Button victoryMenuButton;
        [SerializeField] private List<GameObject> victoryStars;

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
        /// Registers this UIManager with the GameManager and triggers gameplay start.
        /// This implements the self-registration pattern for scene transitions.
        /// Only triggers gameplay if we're in the Game scene (not the MainMenu).
        /// </summary>
        private void Start()
        {
            // Register with the GameManager singleton
            if (GameManager.Instance != null)
            {
                GameManager.Instance.uiManager = this;
                Debug.Log("UIManager registered with GameManager.");

                // Only trigger gameplay start if we're in the Game scene
                // Check if this UIManager has question UI elements assigned (Game scene specific)
                if (questionText != null && answerButtons != null && answerButtons.Count > 0)
                {
                    Debug.Log("UIManager detected in Game scene. Starting gameplay...");
                    GameManager.Instance.StartLevelGameplay();
                }
                else
                {
                    Debug.Log("UIManager detected in a scene without quiz interface (probably MainMenu).");
                }
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is null! UIManager could not register.");
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
        /// </summary>
        public void ShowGameOverScreen()
        {
            Debug.Log("Displaying Game Over screen");

            // Hide quiz panel
            if (quizPanel != null)
            {
                quizPanel.SetActive(false);
            }

            // Show game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                // Wire up buttons
                if (gameOverRetryButton != null)
                {
                    gameOverRetryButton.onClick.RemoveAllListeners();
                    gameOverRetryButton.onClick.AddListener(() => GameManager.Instance.RestartCurrentLevel());
                }

                if (gameOverMenuButton != null)
                {
                    gameOverMenuButton.onClick.RemoveAllListeners();
                    gameOverMenuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMenu());
                }
            }
            else
            {
                Debug.LogWarning("GameOverPanel is not assigned in UIManager!");
            }
        }

        /// <summary>
        /// Shows the Victory panel with score, stars, and navigation options.
        /// </summary>
        /// <param name="score">The player's score for this level.</param>
        /// <param name="starsEarned">Number of stars earned (1-3).</param>
        /// <param name="hasNextLevel">Whether there's a next level available.</param>
        public void ShowVictoryScreen(int score, int starsEarned, bool hasNextLevel)
        {
            Debug.Log($"Displaying Victory screen - Score: {score}, Stars: {starsEarned}");

            // Hide quiz panel
            if (quizPanel != null)
            {
                quizPanel.SetActive(false);
            }

            // Show victory panel
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);

                // Update score text
                if (victoryScoreText != null)
                {
                    victoryScoreText.text = $"Score: {score}";
                }

                // Update stars display
                if (victoryStars != null)
                {
                    for (int i = 0; i < victoryStars.Count; i++)
                    {
                        if (victoryStars[i] != null)
                        {
                            victoryStars[i].SetActive(i < starsEarned);
                        }
                    }
                }

                // Wire up buttons
                if (victoryNextLevelButton != null)
                {
                    victoryNextLevelButton.gameObject.SetActive(hasNextLevel);
                    victoryNextLevelButton.onClick.RemoveAllListeners();
                    victoryNextLevelButton.onClick.AddListener(() => GameManager.Instance.LoadNextLevel());
                }

                if (victoryMenuButton != null)
                {
                    victoryMenuButton.onClick.RemoveAllListeners();
                    victoryMenuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMenu());
                }
            }
            else
            {
                Debug.LogWarning("VictoryPanel is not assigned in UIManager!");
            }
        }

        /// <summary>
        /// Hides all end-game panels and shows the quiz panel.
        /// Called when starting or restarting a level.
        /// </summary>
        public void ResetToQuizView()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (quizPanel != null) quizPanel.SetActive(true);
        }

        /// <summary>
        /// Updates the question UI with data from a QuestionData object.
        /// Updates the question text, image, and answer button texts.
        /// </summary>
        /// <param name="data">The QuestionData to display.</param>
        public void UpdateQuestionUI(QuestionData data)
        {
            if (data == null)
            {
                Debug.LogError("QuestionData is null!");
                return;
            }

            if (questionText != null)
            {
                questionText.text = data.questionText;
            }
            else
            {
                Debug.LogWarning("questionText is not assigned!");
            }

            if (questionImage != null && data.image != null)
            {
                questionImage.sprite = data.image;
                questionImage.gameObject.SetActive(true);
            }
            else if (questionImage != null && data.image == null)
            {
                questionImage.gameObject.SetActive(false);
            }

            if (answerButtonTexts != null && answerButtonTexts.Count > 0)
            {
                for (int i = 0; i < answerButtonTexts.Count && i < data.options.Count; i++)
                {
                    answerButtonTexts[i].text = data.options[i];

                    if (answerButtons != null && i < answerButtons.Count)
                    {
                        answerButtons[i].gameObject.SetActive(true);

                        // Remove previous listeners to avoid duplicate calls
                        answerButtons[i].onClick.RemoveAllListeners();

                        // Create local copy to avoid closure issue
                        int index = i;
                        answerButtons[i].onClick.AddListener(() => GameManager.Instance.SubmitAnswer(index));
                    }
                }

                if (answerButtons != null)
                {
                    for (int i = data.options.Count; i < answerButtons.Count; i++)
                    {
                        answerButtons[i].onClick.RemoveAllListeners();
                        answerButtons[i].gameObject.SetActive(false);
                    }
                }
            }
            else if (answerButtons != null && answerButtons.Count > 0)
            {
                for (int i = 0; i < answerButtons.Count && i < data.options.Count; i++)
                {
                    TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = data.options[i];
                    }
                    answerButtons[i].gameObject.SetActive(true);

                    // Remove previous listeners to avoid duplicate calls
                    answerButtons[i].onClick.RemoveAllListeners();

                    // Create local copy to avoid closure issue
                    int index = i;
                    answerButtons[i].onClick.AddListener(() => GameManager.Instance.SubmitAnswer(index));
                }

                for (int i = data.options.Count; i < answerButtons.Count; i++)
                {
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("No answer buttons are assigned!");
            }
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
