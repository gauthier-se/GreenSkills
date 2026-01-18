using System.Threading.Tasks;
using UnityEngine;

public class QuestionDTO
{
    public string questionText;
    public string[] options;
    public int correctOptionIndex; // j'ai changé GoodAnswer dans les deux classes en correctOptionIndex pour plus de clarté
    public string explanation;
    public Difficulty difficulty;
    public Category category;
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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public async Task<QuestionDTO> LoadQuestionsFromAPI(string apiUrl)
    {
        // Implementation for loading questions from an external API
    }
}
