using System.Collections.Generic;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    /// <summary>
    /// Manages all UI elements and updates them based on game state.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Question UI Elements")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Image questionImage;
        [SerializeField] private List<Button> answerButtons;
        [SerializeField] private List<TextMeshProUGUI> answerButtonTexts;
        
        [Header("Progress UI Elements")]
        [SerializeField] private Slider progressBar;
        
        [Header("Lives UI Elements")]
        [SerializeField] private List<GameObject> lifeHearts;
        
        private List<Image> _answerButtonImages;
        
        /// <summary>
        /// Initializes cached button image references to avoid expensive GetComponent calls.
        /// </summary>
        private void Awake()
        {
            CacheButtonImages();
        }
        
        /// <summary>
        /// Caches Image components from answer buttons for performance optimization.
        /// </summary>
        private void CacheButtonImages()
        {
            if (answerButtons == null) return;
            
            _answerButtonImages = new List<Image>();
            foreach (Button button in answerButtons)
            {
                Image buttonImage = button.GetComponent<Image>();
                _answerButtonImages.Add(buttonImage);
            }
        }
    
        public void ShowMainMenu()
        {
            Debug.Log("ShowMainMenu");
        }
    
        public void ShowGameOverScreen()
        {
            Debug.Log("ShowGameOverScreen");
        }
        
        /// <summary>
        /// Updates the question UI with data from a QuestionData object.
        /// Updates the question text, image, and answer button texts.
        /// </summary>
        /// <param name="data">The QuestionData to display.</param>
        public void UpdateQuestionUI(QuestionData data)
        {
            if (data == null)
            {
                Debug.LogError("QuestionData est null !");
                return;
            }
        
            if (questionText != null)
            {
                questionText.text = data.questionText;
            }
            else
            {
                Debug.LogWarning("questionText n'est pas assigné !");
            }
        
            if (questionImage != null && data.image != null)
            {
                questionImage.sprite = data.image;
                questionImage.gameObject.SetActive(true);
            }
            else if (questionImage != null && data.image == null)
            {
                questionImage.gameObject.SetActive(false);
            }
        
            if (answerButtonTexts != null && answerButtonTexts.Count > 0)
            {
                for (int i = 0; i < answerButtonTexts.Count && i < data.options.Count; i++)
                {
                    answerButtonTexts[i].text = data.options[i];
                
                    if (answerButtons != null && i < answerButtons.Count)
                    {
                        answerButtons[i].gameObject.SetActive(true);
                        
                        // Remove previous listeners to avoid duplicate calls
                        answerButtons[i].onClick.RemoveAllListeners();
                        
                        // Create local copy to avoid closure issue
                        int index = i;
                        answerButtons[i].onClick.AddListener(() => GameManager.Instance.SubmitAnswer(index));
                    }
                }
            
                if (answerButtons != null)
                {
                    for (int i = data.options.Count; i < answerButtons.Count; i++)
                    {
                        answerButtons[i].onClick.RemoveAllListeners();
                        answerButtons[i].gameObject.SetActive(false);
                    }
                }
            }
            else if (answerButtons != null && answerButtons.Count > 0)
            {
                for (int i = 0; i < answerButtons.Count && i < data.options.Count; i++)
                {
                    TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = data.options[i];
                    }
                    answerButtons[i].gameObject.SetActive(true);
                    
                    // Remove previous listeners to avoid duplicate calls
                    answerButtons[i].onClick.RemoveAllListeners();
                    
                    // Create local copy to avoid closure issue
                    int index = i;
                    answerButtons[i].onClick.AddListener(() => GameManager.Instance.SubmitAnswer(index));
                }
            
                for (int i = data.options.Count; i < answerButtons.Count; i++)
                {
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Aucun bouton de réponse n'est assigné !");
            }
        }
        
        /// <summary>
        /// Highlights an answer button with a color based on whether it's correct or incorrect.
        /// </summary>
        /// <param name="index">The index of the answer button to highlight.</param>
        /// <param name="isCorrect">True if the answer is correct, false otherwise.</param>
        public void HighlightAnswer(int index, bool isCorrect)
        {
            if (_answerButtonImages == null || index < 0 || index >= _answerButtonImages.Count)
            {
                Debug.LogError($"Impossible de mettre en surbrillance le bouton à l'index {index}");
                return;
            }
            
            Image buttonImage = _answerButtonImages[index];
            
            if (buttonImage)
            {
                buttonImage.color = isCorrect ? Color.green : Color.red;
            }
            else
            {
                Debug.LogWarning($"Le bouton à l'index {index} n'a pas de composant Image");
            }
        }
        
        /// <summary>
        /// Resets all answer buttons to their default color.
        /// </summary>
        public void ResetAnswerColors()
        {
            if (_answerButtonImages == null) return;
            
            foreach (Image buttonImage in _answerButtonImages)
            {
                if (buttonImage)
                {
                    buttonImage.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// Updates the progress bar to reflect the player's advancement through the level.
        /// </summary>
        /// <param name="progress">A value between 0.0 and 1.0 representing the completion percentage.</param>
        public void UpdateProgressBar(float progress)
        {
            if (progressBar)
            {
                progressBar.value = progress;
                Debug.Log($"Barre de progression mise à jour : {progress * 100:F1}%");
            }
            else
            {
                Debug.LogWarning("ProgressBar n'est pas assigné dans l'UIManager !");
            }
        }
        
        /// <summary>
        /// Updates the lives display by activating/deactivating heart icons based on current lives.
        /// </summary>
        /// <param name="currentLives">The number of lives the player currently has.</param>
        public void UpdateLivesUI(int currentLives)
        {
            if (lifeHearts == null || lifeHearts.Count == 0)
            {
                Debug.LogWarning("Aucun cœur de vie n'est assigné dans l'UIManager !");
                return;
            }
            
            for (int i = 0; i < lifeHearts.Count; i++)
            {
                if (lifeHearts[i])
                {
                    // Activate heart if index is less than current lives
                    lifeHearts[i].SetActive(i < currentLives);
                }
            }
            
            Debug.Log($"Vies restantes : {currentLives}/{lifeHearts.Count}");
        }
    }
}
