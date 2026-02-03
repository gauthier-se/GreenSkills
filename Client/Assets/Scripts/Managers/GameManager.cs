using Data;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Central manager that coordinates all game systems and maintains game state.
    /// Implements the Singleton pattern and persists across scene loads.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public InputManager inputManager;
        public UIManager uiManager;
        public AudioManager audioManager;
        public SceneManager sceneManager;

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "Game";

        [Header("Audio Clips")]
        [SerializeField] private string gameOverMusicName = "GameOverMusic";

        [Header("Game Settings")]
        [SerializeField] private int maxLives = 3;

        private const string HIGHEST_LEVEL_KEY = "HighestLevelUnlocked";

        private System.Collections.Generic.List<QuestionData> _currentLevelQuestions;
        private int _currentQuestionIndex;
        private int _currentLives;
        private int _currentLevelId;

        /// <summary>
        /// Initializes the Singleton instance and ensures persistence across scene loads.
        /// </summary>
        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // Ensure this GameObject is at root level for DontDestroyOnLoad to work
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                    Debug.LogWarning("GameManager was not at root level. It has been moved automatically.");
                }

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Start()
        {
            Debug.Log("Function Start called");
            sceneManager.LoadScene(mainMenuSceneName);

            // Test: Load a test level with 3 questions (uncomment to test)
            // LoadTestLevel();
        }

        /// <summary>
        /// Creates and loads a test level with manually created questions for testing purposes.
        /// This method bypasses the API and creates questions directly in memory.
        /// </summary>
        private void LoadTestLevel()
        {
            _currentLevelQuestions = new System.Collections.Generic.List<QuestionData>();

            // Question 1
            var q1 = ScriptableObject.CreateInstance<QuestionData>();
            q1.questionText = "Quelle est la capitale de la France ?";
            q1.options = new System.Collections.Generic.List<string> { "Paris", "Londres", "Berlin", "Madrid" };
            q1.correctOptionIndex = 0;
            q1.explanation = "Paris est la capitale de la France.";
            q1.difficulty = Difficulty.Easy;
            q1.category = Category.RseBasics;
            _currentLevelQuestions.Add(q1);

            // Question 2
            var q2 = ScriptableObject.CreateInstance<QuestionData>();
            q2.questionText = "Qu'est-ce que l'empreinte carbone ?";
            q2.options = new System.Collections.Generic.List<string> {
                "La mesure des GES émis",
                "Une technique d'impression",
                "Un type de papier",
                "Une marque de chaussures"
            };
            q2.correctOptionIndex = 0;
            q2.explanation = "L'empreinte carbone mesure les gaz à effet de serre émis par une activité.";
            q2.difficulty = Difficulty.Easy;
            q2.category = Category.CarbonFootprint;
            _currentLevelQuestions.Add(q2);

            // Question 3
            var q3 = ScriptableObject.CreateInstance<QuestionData>();
            q3.questionText = "Que signifie 'Green IT' ?";
            q3.options = new System.Collections.Generic.List<string> {
                "Informatique écologique",
                "Recyclage des ordinateurs",
                "Écrans verts",
                "Couleur de code"
            };
            q3.correctOptionIndex = 0;
            q3.explanation = "Le Green IT désigne l'informatique responsable et durable.";
            q3.difficulty = Difficulty.Medium;
            q3.category = Category.GreenIT;
            _currentLevelQuestions.Add(q3);

            _currentQuestionIndex = 0;
            _currentLives = maxLives;
            _currentLevelId = 1; // Test level is Level 1

            if (uiManager)
            {
                uiManager.UpdateLivesUI(_currentLives);
            }

            Debug.Log($"Test level {_currentLevelId} loaded with {_currentLevelQuestions.Count} questions and {_currentLives} lives");

            LoadNextQuestion();
        }

        /// <summary>
        /// Starts a new game by loading the game scene.
        /// </summary>
        public void StartGame()
        {
            Debug.Log("Function StartGame called");
            sceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Ends the current game and cleans up game state.
        /// </summary>
        public void EndGame()
        {
            Debug.Log("Game Ended");
        }

        /// <summary>
        /// Handles the game over state by showing the game over screen and playing appropriate audio.
        /// </summary>
        public void OnGameOver()
        {
            Debug.Log("Game Over");

            if (uiManager)
            {
                uiManager.ShowGameOverScreen();
            }

            if (audioManager)
            {
                audioManager.PlaySound(gameOverMusicName);
            }
        }

        /// <summary>
        /// Called by UIManager when the Game scene is ready to display questions.
        /// This method initiates the gameplay after scene transition is complete.
        /// </summary>
        public void StartLevelGameplay()
        {
            // Safety check: ensure questions have been loaded
            if (_currentLevelQuestions != null && _currentLevelQuestions.Count > 0)
            {
                Debug.Log("Scene ready. Starting first question!");

                // Reset question index to start from the beginning
                _currentQuestionIndex = 0;

                // Update lives UI now that UIManager is available
                if (uiManager)
                {
                    uiManager.UpdateLivesUI(_currentLives);
                }

                // Display the first question
                LoadNextQuestion();
            }
            else
            {
                Debug.LogWarning("No questions loaded in memory! (Perhaps you're launching the Game scene directly without going through the Menu?)");
            }
        }

    /// <summary>
    /// Loads a level from the API and initializes the question list.
    /// </summary>
    /// <param name="apiUrl">The API endpoint URL to fetch the level from.</param>
    public async void LoadLevel(string apiUrl)
    {
        // Check if QuestionManager exists
        if (QuestionManager.Instance == null)
        {
            Debug.LogError("QuestionManager.Instance is null! Make sure a GameObject with QuestionManager exists in the scene and has DontDestroyOnLoad.");
            return;
        }

        LevelDTO levelDto = await QuestionManager.Instance.LoadLevelFromAPI(apiUrl);

        if (levelDto != null && levelDto.questions != null && levelDto.questions.Length > 0)
        {
            // Store questions in memory
            _currentLevelQuestions = QuestionManager.Instance.CreateQuestionsFromDtos(levelDto.questions);
            _currentQuestionIndex = 0;
            _currentLives = maxLives;
            _currentLevelId = levelDto.levelId;

            Debug.Log($"DATA READY: Level {_currentLevelId} loaded with {_currentLevelQuestions.Count} questions and {_currentLives} lives.");

            // Now load the Game scene.
            // The UIManager in the new scene will call StartLevelGameplay() to trigger question display.
            StartGame();
        }
        else
        {
            Debug.LogError($"Failed to load level from API: {apiUrl}");
        }
    }

    /// <summary>
    /// Loads a single question from the API (legacy method for single question loading).
    /// </summary>
    /// <param name="apiUrl">The API endpoint URL to fetch the question from.</param>
    public async void LoadQuestion(string apiUrl)
    {
        QuestionDto dto = await QuestionManager.Instance.LoadQuestionsFromAPI(apiUrl);

        if (dto != null)
        {
            QuestionData question = QuestionManager.Instance.CreateQuestionFromDto(dto);
            _currentLevelQuestions = new System.Collections.Generic.List<QuestionData> { question };
            _currentQuestionIndex = 0;

            if (uiManager)
            {
                uiManager.UpdateQuestionUI(question);
                Debug.Log($"Question chargée et affichée : {question.questionText}");
            }
            else
            {
                Debug.LogError("UIManager is not assigned in GameManager!");
            }
        }
        else
        {
            Debug.LogError($"Failed to load question from API: {apiUrl}");
        }
    }

    /// <summary>
    /// Loads and displays the next question in the current level.
    /// If all questions are completed, triggers level victory.
    /// </summary>
    public void LoadNextQuestion()
    {
        if (_currentLevelQuestions == null || _currentLevelQuestions.Count == 0)
        {
            Debug.LogError("No questions available in this level!");
            return;
        }

        // Check if there are more questions
        if (_currentQuestionIndex < _currentLevelQuestions.Count)
        {
            QuestionData currentQuestion = _currentLevelQuestions[_currentQuestionIndex];

            if (uiManager)
            {
                uiManager.UpdateQuestionUI(currentQuestion);
                Debug.Log($"Question {_currentQuestionIndex + 1}/{_currentLevelQuestions.Count} displayed: {currentQuestion.questionText}");

                // Calculate progress: (currentIndex + 1) / totalQuestions
                // IMPORTANT: Cast to float to avoid integer division (1/2 = 0 in int, but 0.5 in float)
                float progress = (_currentQuestionIndex + 1) / (float)_currentLevelQuestions.Count;
                uiManager.UpdateProgressBar(progress);
            }
            else
            {
                Debug.LogError("UIManager is not assigned in GameManager!");
            }

            _currentQuestionIndex++;
        }
        else
        {
            // All questions completed - Level won!
            WinLevel();
        }
    }

    /// <summary>
    /// Handles the player's answer submission and validates it.
    /// </summary>
    /// <param name="answerIndex">The index of the selected answer option.</param>
    public void SubmitAnswer(int answerIndex)
    {
        if (_currentLevelQuestions == null || _currentLevelQuestions.Count == 0)
        {
            Debug.LogError("No current question!");
            return;
        }

        // Get the current question (index was already incremented in LoadNextQuestion)
        int currentQuestionArrayIndex = _currentQuestionIndex - 1;

        if (currentQuestionArrayIndex < 0 || currentQuestionArrayIndex >= _currentLevelQuestions.Count)
        {
            Debug.LogError("Invalid question index!");
            return;
        }

        QuestionData currentQuestion = _currentLevelQuestions[currentQuestionArrayIndex];
        bool isCorrect = answerIndex == currentQuestion.correctOptionIndex;

        Debug.Log($"Answer submitted: {answerIndex}, Correct answer: {currentQuestion.correctOptionIndex}, Is correct: {isCorrect}");

        // Disable all answer buttons to prevent multiple clicks
        if (uiManager)
        {
            uiManager.SetAnswerButtonsInteractable(false);
        }

        // Display visual feedback
        if (uiManager)
        {
            uiManager.HighlightAnswer(answerIndex, isCorrect);
        }

        // Handle wrong answer - lose a life
        if (!isCorrect)
        {
            _currentLives--;
            Debug.Log($"Wrong answer! Lives remaining: {_currentLives}");

            if (uiManager)
            {
                uiManager.UpdateLivesUI(_currentLives);
            }

            // Check for Game Over
            if (_currentLives <= 0)
            {
                Debug.Log("No more lives! Game Over!");
                // Wait 2 seconds to show the feedback, then trigger Game Over
                StartCoroutine(WaitAndGameOver());
                return; // Don't continue to next question
            }
        }

        // Wait 2 seconds before loading next question
        StartCoroutine(WaitAndLoadNextQuestion(isCorrect));
    }

    /// <summary>
    /// Coroutine that waits for a specified delay before loading the next question.
    /// </summary>
    /// <param name="wasCorrect">Whether the previous answer was correct.</param>
    private System.Collections.IEnumerator WaitAndLoadNextQuestion(bool wasCorrect)
    {
        yield return new WaitForSeconds(2f);

        // Cache UIManager reference to avoid expensive checks in coroutine
        var cachedUIManager = uiManager;

        // Reset button colors and re-enable interaction
        if (cachedUIManager)
        {
            cachedUIManager.ResetAnswerColors();
            cachedUIManager.SetAnswerButtonsInteractable(true);
        }

        // Load next question (lives system handles game over separately)
        if (wasCorrect)
        {
            Debug.Log("Correct answer! Loading next question...");
        }
        else
        {
            Debug.Log("Wrong answer! But you still have lives. Next question...");
        }

        LoadNextQuestion();
    }

    /// <summary>
    /// Coroutine that waits for a specified delay before triggering Game Over.
    /// Used when the player runs out of lives.
    /// </summary>
    private System.Collections.IEnumerator WaitAndGameOver()
    {
        yield return new WaitForSeconds(2f);

        // Cache UIManager reference to avoid expensive checks in coroutine
        var cachedUIManager = uiManager;

        // Reset button colors and re-enable interaction
        if (cachedUIManager)
        {
            cachedUIManager.ResetAnswerColors();
            cachedUIManager.SetAnswerButtonsInteractable(true);
        }

        Debug.Log("No more lives! Game Over...");
        OnGameOver();
    }

    /// <summary>
    /// Handles the level victory state when all questions are completed successfully.
    /// Saves progression only if the player has advanced further than their previous best.
    /// </summary>
    private void WinLevel()
    {
        Debug.Log("Level completed successfully!");

        // Read the current highest level unlocked from PlayerPrefs
        // Default value is 1 if the key doesn't exist (first time playing)
        int highestLevelUnlocked = PlayerPrefs.GetInt(HIGHEST_LEVEL_KEY, 1);

        Debug.Log($"Current level completed: {_currentLevelId}");
        Debug.Log($"Highest level currently unlocked: {highestLevelUnlocked}");

        // Only save if the player has progressed (not replaying an old level)
        bool hasNextLevel = false;
        if (_currentLevelId >= highestLevelUnlocked)
        {
            // Unlock the next level
            int nextLevel = _currentLevelId + 1;
            PlayerPrefs.SetInt(HIGHEST_LEVEL_KEY, nextLevel);
            PlayerPrefs.Save(); // Force save to disk
            hasNextLevel = true;

            Debug.Log($"New level unlocked: {nextLevel}!");
        }
        else
        {
            // Check if there's a level after the current one that's already unlocked
            hasNextLevel = (_currentLevelId + 1) <= highestLevelUnlocked;
            Debug.Log($"Level replayed. No progression update (already at level {highestLevelUnlocked}).");
        }

        // Calculate score and stars based on remaining lives
        int score = _currentLives * 100; // 100 points per remaining life
        int starsEarned = CalculateStars(_currentLives, maxLives);

        // Show victory screen
        if (uiManager)
        {
            uiManager.ShowVictoryScreen(score, starsEarned, hasNextLevel);
        }
    }

    /// <summary>
    /// Calculates the number of stars earned based on remaining lives.
    /// </summary>
    /// <param name="remainingLives">Number of lives remaining at level completion.</param>
    /// <param name="totalLives">Maximum number of lives.</param>
    /// <returns>Number of stars (1-3).</returns>
    private int CalculateStars(int remainingLives, int totalLives)
    {
        float percentage = (float)remainingLives / totalLives;

        if (percentage >= 1f) return 3; // Perfect - all lives remaining
        if (percentage >= 0.66f) return 2; // Good - 2/3 lives remaining
        return 1; // Completed but with losses
    }

    /// <summary>
    /// Returns to the main menu scene.
    /// </summary>
    public void ReturnToMenu()
    {
        Debug.Log("Returning to main menu...");
        sceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Restarts the current level by reloading the questions and resetting game state.
    /// </summary>
    public void RestartCurrentLevel()
    {
        Debug.Log($"Restarting level {_currentLevelId}...");

        // Reset game state
        _currentQuestionIndex = 0;
        _currentLives = maxLives;

        // Reset UI and start gameplay
        if (uiManager)
        {
            uiManager.ResetToQuizView();
            uiManager.UpdateLivesUI(_currentLives);
        }

        LoadNextQuestion();
    }

    /// <summary>
    /// Loads the next level after completing the current one.
    /// </summary>
    public void LoadNextLevel()
    {
        int nextLevelId = _currentLevelId + 1;
        Debug.Log($"Loading level {nextLevelId}...");

        // Construct the local API URL for the next level
        string apiUrl = "local://level/" + nextLevelId;

        // Load the next level
        LoadLevel(apiUrl);
    }

    /// <summary>
    /// Gets the highest level unlocked by the player from saved data.
    /// </summary>
    /// <returns>The highest level number unlocked (default is 1 for new players).</returns>
    public int GetHighestLevelUnlocked()
    {
        return PlayerPrefs.GetInt(HIGHEST_LEVEL_KEY, 1);
    }

    /// <summary>
    /// Checks if a specific level is unlocked and can be played.
    /// </summary>
    /// <param name="levelId">The level ID to check.</param>
    /// <returns>True if the level is unlocked, false otherwise.</returns>
    public bool IsLevelUnlocked(int levelId)
    {
        int highestUnlocked = GetHighestLevelUnlocked();
        return levelId <= highestUnlocked;
    }

    /// <summary>
    /// Resets all saved progress (for testing or reset functionality).
    /// </summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(HIGHEST_LEVEL_KEY);
        PlayerPrefs.Save();
        Debug.Log("Progress reset!");
    }

    }
}
