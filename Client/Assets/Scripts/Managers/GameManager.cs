using System.Collections;
using System.Collections.Generic;
using Data.Exercises;
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

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "Game";

        [Header("Audio Clips")]
        [SerializeField] private string correctSoundName = "Correct";
        [SerializeField] private string incorrectSoundName = "Incorrect";
        [SerializeField] private string gameOverMusicName = "GameOverMusic";
        [SerializeField] private string victorySoundName = "Victory";

        [Header("Game Settings")]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float feedbackDelay = 2f;

        private const string HIGHEST_LEVEL_KEY = "HighestLevelUnlocked";

        // Exercise system state
        private ExerciseUIController _exerciseUIController;
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
                    StartCoroutine(WaitAndGameOver());
                    return;
                }
            }

            StartCoroutine(WaitAndLoadNextExercise());
        }

        /// <summary>
        /// Waits for feedback delay then loads next exercise.
        /// </summary>
        private IEnumerator WaitAndLoadNextExercise()
        {
            yield return new WaitForSeconds(feedbackDelay);

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
        /// Waits for feedback delay then triggers game over.
        /// </summary>
        private IEnumerator WaitAndGameOver()
        {
            yield return new WaitForSeconds(feedbackDelay);

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

            if (audioManager != null)
            {
                audioManager.PlaySound(victorySoundName);
            }

            if (uiManager != null)
            {
                uiManager.ShowVictoryScreen(score, starsEarned, hasNextLevel);
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

            // Unsubscribe from events
            if (_exerciseUIController != null)
            {
                _exerciseUIController.OnAnswerSubmitted -= HandleAnswerSubmitted;
                _exerciseUIController = null;
            }

            sceneManager?.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Restarts the current level.
        /// </summary>
        public void RestartCurrentLevel()
        {
            Debug.Log($"[GameManager] Restarting level {_currentLevelId}...");

            _currentExerciseIndex = 0;
            _currentLives = maxLives;

            if (uiManager != null)
            {
                uiManager.ResetToQuizView();
                uiManager.UpdateLivesUI(_currentLives);
            }

            _exerciseUIController?.ResetCurrentPanel();
            DisplayCurrentExercise();
        }

        /// <summary>
        /// Loads the next level.
        /// </summary>
        public void LoadNextLevel()
        {
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
        /// Resets all saved progress.
        /// </summary>
        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(HIGHEST_LEVEL_KEY);
            PlayerPrefs.Save();
            Debug.Log("[GameManager] Progress reset!");
        }

        #endregion
    }
}
