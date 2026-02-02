using Data;
using UnityEngine;

namespace Managers
{
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

        // Ensure only one instance of GameManager exists (Singleton pattern)
        public void Awake() // Auto-called by Unity when the script instance is being loaded
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scenes
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
            uiManager.ShowGameOverScreen();
            audioManager.PlaySound(gameOverMusicName);
        }
    
        /// <summary>
        /// Loads a question from the API and displays it in the UI.
        /// </summary>
        /// <param name="apiUrl">The API endpoint URL to fetch the question from.</param>
        public async void LoadQuestion(string apiUrl)
    {
        QuestionDto dto = await QuestionManager.Instance.LoadQuestionsFromAPI(apiUrl);
    
        if (dto != null)
        {
            QuestionData question = QuestionManager.Instance.CreateQuestionFromDto(dto);
            
            if (uiManager != null)
            {
                uiManager.UpdateQuestionUI(question);
                Debug.Log($"Question chargée et affichée : {question.questionText}");
            }
            else
            {
                Debug.LogError("UIManager n'est pas assigné dans le GameManager !");
            }
        }
        else
        {
            Debug.LogError($"Échec du chargement de la question depuis l'API : {apiUrl}");
        }
    }
    
    }
}