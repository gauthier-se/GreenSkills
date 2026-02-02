using System.Linq;
using System.Threading.Tasks;
using Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers
{
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
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
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


        // Update is called once per frame
        void Update()
        {
        
        }

        public async Task<QuestionDto> LoadQuestionsFromAPI(string apiUrl)
        {
            // Create a UnityWebRequest to fetch data from the API
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {

                // Send the request and wait for a response
                var operation = request.SendWebRequest();

                // Await until the request is done
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                // Check for errors
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Erreur : " + request.error);
                    return null;
                } 
                // Parse the JSON response
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

    }
}