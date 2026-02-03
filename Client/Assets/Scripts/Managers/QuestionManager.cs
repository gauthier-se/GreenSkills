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
        public QuestionDto[] questions;
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
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    
        async void Start()
        {
            string apiUrl = "https://example.com/api/questions/1";
            QuestionDto dto = await LoadQuestionsFromAPI(apiUrl);

            if (dto == null)
            {
                return;
            }
            QuestionData question = CreateQuestionFromDto(dto);
            Debug.Log($"Question chargée : {question.questionText}");
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
                    Debug.LogError("Erreur : " + request.error);
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
                Debug.LogWarning($"Image '{questionDto.imageName}' non trouvée dans Resources");
            }

            return questionData;
        }
        
        /// <summary>
        /// Fetches a level with multiple questions from a REST API endpoint asynchronously.
        /// </summary>
        /// <param name="apiUrl">The API endpoint URL to fetch the level from.</param>
        /// <returns>A LevelDTO object if successful, null otherwise.</returns>
        public async Task<LevelDTO> LoadLevelFromAPI(string apiUrl)
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
                    Debug.LogError("Erreur : " + request.error);
                    return null;
                } 
                
                var json = request.downloadHandler.text;
                return JsonUtility.FromJson<LevelDTO>(json);
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
                Debug.LogWarning("Aucune question à convertir");
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