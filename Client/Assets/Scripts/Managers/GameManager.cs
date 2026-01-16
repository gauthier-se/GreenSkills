using UnityEngine;

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

    public void Start() // Auto-called by Unity before the first frame update
    {
        // After the boot sequence, load the main menu
        Debug.Log("Function Start called");
        sceneManager.LoadScene(mainMenuSceneName);
    }

    public void StartGame()
    {
        // Debug.Log("Game Started");
        // uiManager.ShowMainMenu();
        // audioManager.PlaySound("MainMenuMusic");

        // When the player clicks "Start Game", load the game scene
        Debug.Log("Function StartGame called");
        sceneManager.LoadScene(gameSceneName);
    }

    public void EndGame()
    {
        Debug.Log("Game Ended");
        // Clean up game state here
    }

    public void OnGameOver()
    {
        Debug.Log("Game Over");
        uiManager.ShowGameOverScreen();
        audioManager.PlaySound(gameOverMusicName);
    }
}