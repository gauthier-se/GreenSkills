using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Data.Exercises;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers
{
    /// <summary>
    /// Represents a level containing multiple exercises of various types.
    /// </summary>
    public class LevelData
    {
        public int LevelId { get; set; }
        public string Theme { get; set; }
        public List<BaseExerciseData> Exercises { get; set; }

        public LevelData()
        {
            Exercises = new List<BaseExerciseData>();
        }
    }

    /// <summary>
    /// Manages exercise data loading from API and conversion to exercise data objects.
    /// Supports multiple exercise types (Quiz, TrueFalse, FillInBlank, Sorting, Matching).
    /// Implements the Singleton pattern to ensure a single instance across the application.
    /// </summary>
    public class ExerciseManager : MonoBehaviour
    {
        public static ExerciseManager Instance { get; private set; }

        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // Ensure this GameObject is at root level for DontDestroyOnLoad to work
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                    LogWarning("ExerciseManager was not at root level. It has been moved automatically.");
                }

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Loads a level with multiple exercises from either a local JSON file or a REST API endpoint.
        /// Supports "local://" protocol for loading from Resources/Data/levels_data.json.
        /// </summary>
        /// <param name="apiUrl">The API endpoint URL or local path (e.g., "local://level/1").</param>
        /// <returns>A LevelData object if successful, null otherwise.</returns>
        public async Task<LevelData> LoadLevelAsync(string apiUrl)
        {
            // MOCK MODE: Load from local JSON file
            if (apiUrl.StartsWith("local://"))
            {
                return await LoadLevelFromLocalAsync(apiUrl);
            }

            // WEB MODE: Load from actual REST API
            return await LoadLevelFromApiAsync(apiUrl);
        }

        /// <summary>
        /// Loads a level from local Resources folder.
        /// </summary>
        private async Task<LevelData> LoadLevelFromLocalAsync(string apiUrl)
        {
            // Simulate network delay for realism (optional)
            await Task.Delay(300);

            // Load the JSON file from Resources/Data/levels_data
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/levels_data");

            if (jsonFile == null)
            {
                LogError("File 'Data/levels_data' not found in Resources!");
                return null;
            }

            // Try to deserialize as new format first
            AllLevelsWithExercisesDto allLevels = null;

            try
            {
                allLevels = JsonUtility.FromJson<AllLevelsWithExercisesDto>(jsonFile.text);
            }
            catch (System.Exception e)
            {
                LogError($"Error deserializing JSON: {e.Message}");
                return null;
            }

            // Check if we got valid data
            if (allLevels?.levels == null || allLevels.levels.Length == 0)
            {
                LogError("No valid levels found in JSON file");
                return null;
            }

            // Extract the level ID from the URL
            string idString = apiUrl.Substring(apiUrl.LastIndexOf('/') + 1);

            if (!int.TryParse(idString, out int levelIdRequested))
            {
                LogError($"Invalid level ID in URL: {apiUrl}");
                return null;
            }

            // Find the requested level
            var foundLevel = allLevels.levels.FirstOrDefault(l => l.levelId == levelIdRequested);

            if (foundLevel == null)
            {
                LogError($"Level {levelIdRequested} does not exist in the local JSON file.");
                return null;
            }

            // Convert to LevelData
            var levelData = new LevelData
            {
                LevelId = foundLevel.levelId,
                Theme = foundLevel.theme,
                Exercises = ExerciseFactory.CreateFromDtos(foundLevel.exercises)
            };

            Log($"Level {levelIdRequested} loaded from local (Theme: {levelData.Theme}, Exercises: {levelData.Exercises.Count})");
            return levelData;
        }

        /// <summary>
        /// Loads a level from a REST API endpoint.
        /// </summary>
        private async Task<LevelData> LoadLevelFromApiAsync(string apiUrl)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogError("Web API Error: " + request.error);
                    return null;
                }

                var json = request.downloadHandler.text;

                try
                {
                    var levelDto = JsonUtility.FromJson<LevelWithExercisesDto>(json);

                    if (levelDto != null)
                    {
                        var levelData = new LevelData
                        {
                            LevelId = levelDto.levelId,
                            Theme = levelDto.theme,
                            Exercises = ExerciseFactory.CreateFromDtos(levelDto.exercises)
                        };

                        Log($"Level {levelData.LevelId} loaded from Web API");
                        return levelData;
                    }
                }
                catch (System.Exception e)
                {
                    LogError($"Error parsing API response: {e.Message}");
                }

                return null;
            }
        }

        /// <summary>
        /// Gets statistics about exercise types in a level.
        /// </summary>
        public Dictionary<ExerciseType, int> GetExerciseTypeStats(LevelData level)
        {
            var stats = new Dictionary<ExerciseType, int>();

            if (level?.Exercises == null) return stats;

            foreach (var exercise in level.Exercises)
            {
                if (stats.ContainsKey(exercise.exerciseType))
                {
                    stats[exercise.exerciseType]++;
                }
                else
                {
                    stats[exercise.exerciseType] = 1;
                }
            }

            return stats;
        }

        #region Logging Helpers

        private void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[ExerciseManager] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[ExerciseManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[ExerciseManager] {message}");
        }

        #endregion
    }
}
