using System.Linq;
using System.Threading.Tasks;
using Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers
{
    /// <summary>
    /// Data Transfer Object for deserializing question data from API responses.
    /// </summary>
    [System.Serializable]
    public class QuestionDto
    {
        public string questionText;
        public string[] options;
        public int correctOptionIndex;
        public string explanation;
        public Difficulty difficulty;
        public Category category;
        public string imageName;
    }

    /// <summary>
    /// Data Transfer Object for deserializing a level containing multiple questions.
    /// </summary>
    [System.Serializable]
    public class LevelDTO
    {
        public int levelId;
        public string theme;
        public QuestionDto[] questions;
    }

    /// <summary>
    /// Wrapper class for deserializing multiple levels from a JSON file.
    /// JsonUtility requires arrays to be wrapped in an object.
    /// </summary>
    [System.Serializable]
    public class AllLevelsDTO
    {
        public LevelDTO[] levels;
    }

    /// <summary>
    /// Manages question data loading from API and conversion to QuestionData objects.
    /// Implements the Singleton pattern to ensure a single instance across the application.
    /// </summary>
    public class QuestionManager : MonoBehaviour
    {
        public static QuestionManager Instance { get; private set; }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Ensure this GameObject is at root level for DontDestroyOnLoad to work
            if (transform.parent != null)
            {
                transform.SetParent(null);
                Debug.LogWarning("QuestionManager was not at root level. It has been moved automatically.");
            }

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

        /// <summary>
        /// Fetches question data from a REST API endpoint asynchronously.
        /// </summary>
        /// <param name="apiUrl">The API endpoint URL to fetch the question from.</param>
        /// <returns>A QuestionDto object if successful, null otherwise.</returns>
        public async Task<QuestionDto> LoadQuestionsFromAPI(string apiUrl)
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
                    Debug.LogError("Error: " + request.error);
                    return null;
                }

                var json = request.downloadHandler.text;
                return JsonUtility.FromJson<QuestionDto>(json);

            }
        }

        /// <summary>
        /// Creates a QuestionData ScriptableObject from a QuestionDto.
        /// Loads the sprite from Resources if an image name is provided.
        /// </summary>
        /// <param name="questionDto">The DTO containing question data from the API.</param>
        /// <returns>A QuestionData ScriptableObject instance.</returns>
        public QuestionData CreateQuestionFromDto(QuestionDto questionDto)
        {
            var questionData = ScriptableObject.CreateInstance<QuestionData>();

            questionData.questionText = questionDto.questionText;
            questionData.options = questionDto.options.ToList();
            questionData.correctOptionIndex = questionDto.correctOptionIndex;
            questionData.explanation = questionDto.explanation;
            questionData.difficulty = questionDto.difficulty;
            questionData.category = questionDto.category;

            if (string.IsNullOrEmpty(questionDto.imageName))
            {
                return questionData;
            }

            questionData.image = Resources.Load<Sprite>(questionDto.imageName);

            if (questionData.image == null)
            {
                Debug.LogWarning($"Image '{questionDto.imageName}' not found in Resources");
            }

            return questionData;
        }

        /// <summary>
        /// Fetches a level with multiple questions from either a local JSON file or a REST API endpoint.
        /// Supports "local://" protocol for loading from Resources/Data/levels_data.json.
        /// </summary>
        /// <param name="apiUrl">The API endpoint URL or local path (e.g., "local://level/1").</param>
        /// <returns>A LevelDTO object if successful, null otherwise.</returns>
        public async Task<LevelDTO> LoadLevelFromAPI(string apiUrl)
        {
            // MOCK MODE: Load from local JSON file
            if (apiUrl.StartsWith("local://"))
            {
                // Simulate network delay for realism (optional)
                await Task.Delay(500);

                // 1. Load the JSON file from Resources/Data/levels_data
                TextAsset jsonFile = Resources.Load<TextAsset>("Data/levels_data");

                if (jsonFile == null)
                {
                    Debug.LogError("File 'Data/levels_data' not found in Resources! Make sure the file is in Assets/Resources/Data/");
                    return null;
                }

                // 2. Deserialize the entire JSON file
                AllLevelsDTO allLevels = JsonUtility.FromJson<AllLevelsDTO>(jsonFile.text);

                if (allLevels == null || allLevels.levels == null)
                {
                    Debug.LogError("Error deserializing JSON file");
                    return null;
                }

                // 3. Extract the level ID from the URL (e.g., "local://level/1" -> "1")
                string idString = apiUrl.Substring(apiUrl.LastIndexOf('/') + 1);

                if (!int.TryParse(idString, out int levelIdRequested))
                {
                    Debug.LogError($"Invalid level ID in URL: {apiUrl}");
                    return null;
                }

                // 4. Find the requested level in the array
                LevelDTO foundLevel = allLevels.levels.FirstOrDefault(l => l.levelId == levelIdRequested);

                if (foundLevel == null)
                {
                    Debug.LogError($"Level {levelIdRequested} does not exist in the local JSON file.");
                    return null;
                }

                Debug.Log($"Level {levelIdRequested} loaded from local mock (Theme: {foundLevel.theme})");
                return foundLevel;
            }

            // WEB MODE: Load from actual REST API
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Web API Error: " + request.error);
                    return null;
                }

                var json = request.downloadHandler.text;
                LevelDTO level = JsonUtility.FromJson<LevelDTO>(json);

                if (level != null)
                {
                    Debug.Log($"Level {level.levelId} loaded from Web API");
                }

                return level;
            }
        }

        /// <summary>
        /// Converts a list of QuestionDto objects to a list of QuestionData ScriptableObjects.
        /// </summary>
        /// <param name="questionDtos">Array of QuestionDto objects to convert.</param>
        /// <returns>A list of QuestionData ScriptableObject instances.</returns>
        public System.Collections.Generic.List<QuestionData> CreateQuestionsFromDtos(QuestionDto[] questionDtos)
        {
            var questions = new System.Collections.Generic.List<QuestionData>();

            if (questionDtos == null || questionDtos.Length == 0)
            {
                Debug.LogWarning("No questions to convert");
                return questions;
            }

            foreach (var dto in questionDtos)
            {
                questions.Add(CreateQuestionFromDto(dto));
            }

            return questions;
        }

    }
}
