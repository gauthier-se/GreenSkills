using System.Collections;
using System.Collections.Generic;
using Data.Exercises;
using UI;
using UI.Exercises;
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

        [Header("Manager References")]
        public UIManager uiManager;
        public AudioManager audioManager;
        public SceneManager sceneManager;
        public AuthManager authManager;
        public GamificationManager gamificationManager;

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string loginSceneName = "Login";

        [Header("Audio Clips")]
        [SerializeField] private string correctSoundName = "Correct";
        [SerializeField] private string incorrectSoundName = "Incorrect";
        [SerializeField] private string gameOverMusicName = "GameOverMusic";
        [SerializeField] private string victorySoundName = "Victory";
        [SerializeField] private string ambientMusicName = "AmbientMusic";

        [Header("Game Settings")]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float feedbackDelay = 2f;

        private const string HIGHEST_LEVEL_KEY = "HighestLevelUnlocked";

        // Exercise system state
        private ExerciseUIController _exerciseUIController;
        private ExplanationPopupController _explanationPopup;
        private LevelData _currentLevelData;
        private List<BaseExerciseData> _currentExercises;
        private int _currentExerciseIndex;
        private int _currentLives;
        private int _currentLevelId;

        #region Singleton Setup

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                if (transform.parent != null)
                {
                    transform.SetParent(null);
                    Debug.LogWarning("[GameManager] Was not at root level. Moved automatically.");
                }

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public API - Level Loading

        /// <summary>
        /// Loads a level from the specified API URL.
        /// </summary>
        /// <param name="apiUrl">API URL or local path (e.g., "local://level/1")</param>
        public async void LoadLevel(string apiUrl)
        {
            Debug.Log($"[GameManager] Loading level from: {apiUrl}");

            if (ExerciseManager.Instance == null)
            {
                Debug.LogError("[GameManager] ExerciseManager.Instance is null!");
                return;
            }

            _currentLevelData = await ExerciseManager.Instance.LoadLevelAsync(apiUrl);

            if (_currentLevelData == null || _currentLevelData.Exercises == null || _currentLevelData.Exercises.Count == 0)
            {
                Debug.LogError($"[GameManager] Failed to load level from: {apiUrl}");
                return;
            }

            _currentExercises = _currentLevelData.Exercises;
            _currentExerciseIndex = 0;
            _currentLives = maxLives;
            _currentLevelId = _currentLevelData.LevelId;

            Debug.Log($"[GameManager] Level {_currentLevelId} loaded: {_currentExercises.Count} exercises, Theme: {_currentLevelData.Theme}");

            // Log exercise types
            foreach (var exercise in _currentExercises)
            {
                Debug.Log($"  - {exercise.exerciseType}: {exercise.GetMainText()}");
            }

            StartGame();
        }

        /// <summary>
        /// Starts the game by loading the game scene.
        /// </summary>
        public void StartGame()
        {
            Debug.Log("[GameManager] Starting game...");
            sceneManager?.LoadScene(gameSceneName);
        }

        #endregion

        #region UI Controller Registration

        /// <summary>
        /// Registers the ExerciseUIController when the Game scene loads.
        /// Called by ExerciseSceneInitializer.
        /// </summary>
        public void RegisterExerciseUIController(ExerciseUIController controller)
        {
            _exerciseUIController = controller;
            _exerciseUIController.OnAnswerSubmitted += HandleAnswerSubmitted;

            // Get the explanation popup reference
            _explanationPopup = controller.GetExplanationPopup();
            if (_explanationPopup == null)
            {
                Debug.LogWarning("[GameManager] ExplanationPopupController not found on ExerciseUIController. Explanations will be skipped.");
            }

            Debug.Log("[GameManager] ExerciseUIController registered.");

            // Start gameplay if exercises are loaded
            if (_currentExercises != null && _currentExercises.Count > 0)
            {
                StartLevelGameplay();
            }
        }

        /// <summary>
        /// Called when the Game scene is ready to start gameplay.
        /// </summary>
        public void StartLevelGameplay()
        {
            if (_currentExercises == null || _currentExercises.Count == 0)
            {
                Debug.LogWarning("[GameManager] No exercises loaded!");
                return;
            }

            Debug.Log("[GameManager] Starting level gameplay...");

            _currentExerciseIndex = 0;

            if (uiManager != null)
            {
                uiManager.UpdateLivesUI(_currentLives);
            }

            if (audioManager != null)
            {
                audioManager.PlaySound(ambientMusicName);
            }

            DisplayCurrentExercise();
        }

        #endregion

        #region Exercise Display & Answer Handling

        /// <summary>
        /// Displays the current exercise.
        /// </summary>
        private void DisplayCurrentExercise()
        {
            if (_exerciseUIController == null)
            {
                Debug.LogError("[GameManager] ExerciseUIController not registered!");
                return;
            }

            if (_currentExerciseIndex >= _currentExercises.Count)
            {
                WinLevel();
                return;
            }

            BaseExerciseData exercise = _currentExercises[_currentExerciseIndex];
            _exerciseUIController.DisplayExercise(exercise);

            // Update progress
            if (uiManager != null)
            {
                float progress = (_currentExerciseIndex + 1) / (float)_currentExercises.Count;
                uiManager.UpdateProgressBar(progress);
            }

            Debug.Log($"[GameManager] Exercise {_currentExerciseIndex + 1}/{_currentExercises.Count}: {exercise.exerciseType}");
        }

        /// <summary>
        /// Handles answer submission from the exercise controller.
        /// </summary>
        private void HandleAnswerSubmitted(BaseExerciseData exercise, object answer)
        {
            bool isCorrect = exercise.ValidateAnswer(answer);
            Debug.Log($"[GameManager] Answer: {(isCorrect ? "Correct" : "Incorrect")}");

            // Play sound feedback
            if (audioManager != null)
            {
                audioManager.PlaySound(isCorrect ? correctSoundName : incorrectSoundName);
            }

            // Award XP for correct answers
            if (isCorrect)
            {
                gamificationManager?.AwardCorrectAnswerXP();
            }

            // Show visual feedback on the exercise panel (correct/incorrect highlighting)
            _exerciseUIController?.ShowFeedback(isCorrect);

            if (!isCorrect)
            {
                _currentLives--;
                Debug.Log($"[GameManager] Lives remaining: {_currentLives}");

                if (uiManager != null)
                {
                    uiManager.UpdateLivesUI(_currentLives);
                }

                if (_currentLives <= 0)
                {
                    StartCoroutine(ShowFeedbackAndGameOver(exercise, isCorrect));
                    return;
                }
            }

            StartCoroutine(ShowFeedbackAndAdvance(exercise, isCorrect));
        }

        /// <summary>
        /// Shows the explanation popup, waits for dismissal, then loads the next exercise.
        /// Falls back to a timed delay if no popup is configured.
        /// </summary>
        private IEnumerator ShowFeedbackAndAdvance(BaseExerciseData exercise, bool isCorrect)
        {
            if (_explanationPopup != null && !string.IsNullOrEmpty(exercise.explanation))
            {
                // Wait briefly so the player sees the visual feedback before the popup
                yield return new WaitForSeconds(0.5f);

                _explanationPopup.Show(exercise.explanation, isCorrect);
                yield return new WaitUntil(() => !_explanationPopup.IsVisible());
            }
            else
            {
                // Fallback: use the fixed delay if no popup is available
                yield return new WaitForSeconds(feedbackDelay);
            }

            _exerciseUIController?.ResetCurrentPanel();
            _currentExerciseIndex++;

            if (_currentExerciseIndex < _currentExercises.Count)
            {
                DisplayCurrentExercise();
            }
            else
            {
                WinLevel();
            }
        }

        /// <summary>
        /// Shows the explanation popup, waits for dismissal, then triggers game over.
        /// Falls back to a timed delay if no popup is configured.
        /// </summary>
        private IEnumerator ShowFeedbackAndGameOver(BaseExerciseData exercise, bool isCorrect)
        {
            if (_explanationPopup != null && !string.IsNullOrEmpty(exercise.explanation))
            {
                yield return new WaitForSeconds(0.5f);

                _explanationPopup.Show(exercise.explanation, isCorrect);
                yield return new WaitUntil(() => !_explanationPopup.IsVisible());
            }
            else
            {
                yield return new WaitForSeconds(feedbackDelay);
            }

            Debug.Log("[GameManager] Game Over!");
            OnGameOver();
        }

        #endregion

        #region Game End States

        /// <summary>
        /// Handles the game over state.
        /// </summary>
        public void OnGameOver()
        {
            Debug.Log("[GameManager] Showing Game Over screen...");

            if (uiManager != null)
            {
                uiManager.ShowGameOverScreen();
            }

            if (audioManager != null)
            {
                audioManager.PlaySound(gameOverMusicName);
            }
        }

        /// <summary>
        /// Handles level victory.
        /// Awards gamification rewards and persists star rating.
        /// </summary>
        private void WinLevel()
        {
            Debug.Log("[GameManager] Level completed!");

            int highestLevelUnlocked = PlayerPrefs.GetInt(HIGHEST_LEVEL_KEY, 1);

            bool hasNextLevel = false;
            if (_currentLevelId >= highestLevelUnlocked)
            {
                int nextLevel = _currentLevelId + 1;
                PlayerPrefs.SetInt(HIGHEST_LEVEL_KEY, nextLevel);
                PlayerPrefs.Save();
                hasNextLevel = true;

                Debug.Log($"[GameManager] Level {nextLevel} unlocked!");
            }
            else
            {
                hasNextLevel = (_currentLevelId + 1) <= highestLevelUnlocked;
            }

            int score = _currentLives * 100;
            int starsEarned = CalculateStars(_currentLives, maxLives);

            // Persist star rating (only saves if better than previous)
            LevelScoreManager.SaveLevelStars(_currentLevelId, starsEarned);

            // Award gamification rewards
            LevelRewards rewards = default;
            if (gamificationManager != null)
            {
                rewards = gamificationManager.AwardLevelCompletion(_currentLives, maxLives);
            }

            if (audioManager != null)
            {
                audioManager.PlaySound(victorySoundName);
            }

            if (uiManager != null)
            {
                uiManager.ShowVictoryScreen(score, starsEarned, hasNextLevel, rewards);
            }
        }

        /// <summary>
        /// Calculates stars based on remaining lives.
        /// </summary>
        private int CalculateStars(int remainingLives, int totalLives)
        {
            float percentage = (float)remainingLives / totalLives;

            if (percentage >= 1f) return 3;
            if (percentage >= 0.66f) return 2;
            return 1;
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Returns to the main menu.
        /// </summary>
        public void ReturnToMenu()
        {
            Debug.Log("[GameManager] Returning to main menu...");

            if (audioManager != null) audioManager.StopMusic();

            // Unsubscribe from events
            if (_exerciseUIController != null)
            {
                _exerciseUIController.OnAnswerSubmitted -= HandleAnswerSubmitted;
                _exerciseUIController = null;
            }

            _explanationPopup = null;

            sceneManager?.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Logs out the current user and navigates to the login scene.
        /// </summary>
        public void Logout()
        {
            Debug.Log("[GameManager] Logging out...");

            if (authManager != null)
            {
                authManager.Logout();
            }
            else if (AuthManager.Instance != null)
            {
                AuthManager.Instance.Logout();
            }

            sceneManager?.LoadScene(loginSceneName);
        }

        /// <summary>
        /// Restarts the current level.
        /// </summary>
        public void RestartCurrentLevel()
        {
            Debug.Log($"[GameManager] Restarting level {_currentLevelId}...");

            if (audioManager != null) audioManager.StopMusic();

            _currentExerciseIndex = 0;
            _currentLives = maxLives;

            if (uiManager != null)
            {
                uiManager.ResetToQuizView();
                uiManager.UpdateLivesUI(_currentLives);
            }

            _exerciseUIController?.ResetCurrentPanel();
            DisplayCurrentExercise();

            if (audioManager != null)
            {
                audioManager.PlaySound(ambientMusicName);
            }
        }

        /// <summary>
        /// Loads the next level.
        /// </summary>
        public void LoadNextLevel()
        {
            if (audioManager != null) audioManager.StopMusic();

            int nextLevelId = _currentLevelId + 1;
            Debug.Log($"[GameManager] Loading level {nextLevelId}...");

            LoadLevel($"local://level/{nextLevelId}");
        }

        #endregion

        #region Progress Management

        /// <summary>
        /// Gets the highest level unlocked.
        /// </summary>
        public int GetHighestLevelUnlocked()
        {
            return PlayerPrefs.GetInt(HIGHEST_LEVEL_KEY, 1);
        }

        /// <summary>
        /// Checks if a level is unlocked.
        /// </summary>
        public bool IsLevelUnlocked(int levelId)
        {
            return levelId <= GetHighestLevelUnlocked();
        }

        /// <summary>
        /// Resets all saved progress, including gamification data.
        /// </summary>
        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(HIGHEST_LEVEL_KEY);
            PlayerPrefs.Save();

            gamificationManager?.ResetAll();

            Debug.Log("[GameManager] Progress reset!");
        }

        #endregion
    }
}
