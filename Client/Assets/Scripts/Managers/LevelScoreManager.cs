using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Extension of GameManager to handle level scoring and star ratings.
    /// This is a FUTURE FEATURE example - not yet implemented in the main GameManager.
    /// </summary>
    public class LevelScoreManager : MonoBehaviour
    {
        private const string LevelStarsKeyPrefix = "Level_";
        private const string LevelStarsKeySuffix = "_Stars";
        private const string LevelBestTimeKeySuffix = "_BestTime";

        /// <summary>
        /// Calculates the star rating for a level based on performance.
        /// </summary>
        /// <param name="correctAnswers">Number of correct answers.</param>
        /// <param name="totalQuestions">Total number of questions.</param>
        /// <param name="livesRemaining">Lives remaining at the end.</param>
        /// <returns>Star rating from 1 to 3.</returns>
        public static int CalculateStars(int correctAnswers, int totalQuestions, int livesRemaining)
        {
            // Perfect score with full lives = 3 stars
            if (correctAnswers == totalQuestions && livesRemaining == 3)
            {
                return 3;
            }

            // Perfect score with 1-2 lives = 2 stars
            if (correctAnswers == totalQuestions && livesRemaining > 0)
            {
                return 2;
            }

            // Completed but with mistakes = 1 star
            if (correctAnswers >= totalQuestions * 0.7f) // 70% or more
            {
                return 1;
            }

            // Failed (shouldn't reach here if game over is handled correctly)
            return 0;
        }

        /// <summary>
        /// Saves the star rating for a specific level.
        /// Only saves if it's better than the previous score.
        /// </summary>
        /// <param name="levelId">The level ID.</param>
        /// <param name="stars">Number of stars earned (1-3).</param>
        public static void SaveLevelStars(int levelId, int stars)
        {
            string key = LevelStarsKeyPrefix + levelId + LevelStarsKeySuffix;
            int previousStars = PlayerPrefs.GetInt(key, 0);

            // Only save if we got more stars than before
            if (stars > previousStars)
            {
                PlayerPrefs.SetInt(key, stars);
                PlayerPrefs.Save();

                Debug.Log($"Level {levelId}: New record of {stars} stars!");
            }
            else
            {
                Debug.Log($"Level {levelId}: {stars} stars (current record: {previousStars})");
            }
        }

        /// <summary>
        /// Gets the star rating for a specific level.
        /// </summary>
        /// <param name="levelId">The level ID.</param>
        /// <returns>Number of stars earned (0-3), or 0 if never played.</returns>
        public static int GetLevelStars(int levelId)
        {
            string key = LevelStarsKeyPrefix + levelId + LevelStarsKeySuffix;
            return PlayerPrefs.GetInt(key, 0);
        }

        /// <summary>
        /// Saves the best completion time for a level.
        /// </summary>
        /// <param name="levelId">The level ID.</param>
        /// <param name="timeInSeconds">Time taken to complete the level.</param>
        public static void SaveBestTime(int levelId, float timeInSeconds)
        {
            string key = LevelStarsKeyPrefix + levelId + LevelBestTimeKeySuffix;
            float previousBest = PlayerPrefs.GetFloat(key, float.MaxValue);

            if (timeInSeconds < previousBest)
            {
                PlayerPrefs.SetFloat(key, timeInSeconds);
                PlayerPrefs.Save();

                Debug.Log($"Level {levelId}: New time record {timeInSeconds:F2}s!");
            }
        }

        /// <summary>
        /// Gets the best completion time for a level.
        /// </summary>
        /// <param name="levelId">The level ID.</param>
        /// <returns>Best time in seconds, or -1 if never completed.</returns>
        public static float GetBestTime(int levelId)
        {
            string key = LevelStarsKeyPrefix + levelId + LevelBestTimeKeySuffix;
            float bestTime = PlayerPrefs.GetFloat(key, -1f);
            return bestTime > 0 ? bestTime : -1f;
        }

        /// <summary>
        /// Calculates the total number of stars earned across all levels.
        /// </summary>
        /// <param name="totalLevels">Total number of levels in the game.</param>
        /// <returns>Total stars earned.</returns>
        public static int GetTotalStars(int totalLevels)
        {
            int total = 0;

            for (int i = 1; i <= totalLevels; i++)
            {
                total += GetLevelStars(i);
            }

            return total;
        }

        /// <summary>
        /// Checks if the player has earned 3 stars on all levels (100% completion).
        /// </summary>
        /// <param name="totalLevels">Total number of levels in the game.</param>
        /// <returns>True if all levels have 3 stars.</returns>
        public static bool HasPerfectCompletion(int totalLevels)
        {
            for (int i = 1; i <= totalLevels; i++)
            {
                if (GetLevelStars(i) < 3)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Example usage in GameManager.WinLevel()
        /// </summary>
        public static void ExampleIntegration()
        {
            // In GameManager.WinLevel(), add this:
            /*
            int correctAnswers = CalculateCorrectAnswers(); // You need to track this
            int stars = LevelScoreManager.CalculateStars(
                correctAnswers,
                _currentLevelQuestions.Count,
                _currentLives
            );

            LevelScoreManager.SaveLevelStars(_currentLevelId, stars);

            // Optionally save time if you track it
            float timeTaken = Time.time - _levelStartTime;
            LevelScoreManager.SaveBestTime(_currentLevelId, timeTaken);
            */
        }
    }
}
