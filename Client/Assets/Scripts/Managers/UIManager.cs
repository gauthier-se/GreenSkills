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
                    }
                }
            
                if (answerButtons != null)
                {
                    for (int i = data.options.Count; i < answerButtons.Count; i++)
                    {
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
                }
            
                for (int i = data.options.Count; i < answerButtons.Count; i++)
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Aucun bouton de réponse n'est assigné !");
            }
        }
    }
}
