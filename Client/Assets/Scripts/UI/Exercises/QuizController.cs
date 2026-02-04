using System.Collections.Generic;
using Data.Exercises;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Exercises
{
    /// <summary>
    /// Controller for Quiz (Multiple Choice) exercises.
    /// This is the most similar to the legacy QuestionData system.
    /// </summary>
    public class QuizController : BaseExerciseController
    {
        [Header("Quiz UI Elements")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Image questionImage;
        [SerializeField] private List<Button> answerButtons;
        [SerializeField] private List<TextMeshProUGUI> answerButtonTexts;

        [Header("Feedback Colors")]
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color incorrectColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color defaultColor = Color.white;

        private List<Image> _answerButtonImages;
        private QuizExerciseData _exerciseData;
        private int _selectedAnswerIndex = -1;

        private void Awake()
        {
            CacheButtonImages();
            SetupButtonListeners();
        }

        /// <summary>
        /// Caches button image references.
        /// </summary>
        private void CacheButtonImages()
        {
            if (answerButtons == null) return;

            _answerButtonImages = new List<Image>();
            foreach (var button in answerButtons)
            {
                if (button != null)
                {
                    _answerButtonImages.Add(button.GetComponent<Image>());
                }
            }
        }

        /// <summary>
        /// Sets up button click listeners.
        /// </summary>
        private void SetupButtonListeners()
        {
            if (answerButtons == null) return;

            for (int i = 0; i < answerButtons.Count; i++)
            {
                if (answerButtons[i] != null)
                {
                    int index = i; // Capture for closure
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
                }
            }
        }

        /// <summary>
        /// Initializes the controller with Quiz exercise data.
        /// </summary>
        public override void Initialize(BaseExerciseData exerciseData)
        {
            base.Initialize(exerciseData);

            _exerciseData = exerciseData as QuizExerciseData;
            _selectedAnswerIndex = -1;

            if (_exerciseData == null)
            {
                Debug.LogError("QuizController: Invalid exercise data type!");
                return;
            }

            // Update question text
            if (questionText != null)
            {
                questionText.text = _exerciseData.questionText;
            }

            // Update question image
            if (questionImage != null)
            {
                if (_exerciseData.image != null)
                {
                    questionImage.sprite = _exerciseData.image;
                    questionImage.gameObject.SetActive(true);
                }
                else
                {
                    questionImage.gameObject.SetActive(false);
                }
            }

            // Update answer buttons
            UpdateAnswerButtons();

            // Reset colors
            ResetButtonColors();
        }

        /// <summary>
        /// Updates answer button texts and visibility.
        /// </summary>
        private void UpdateAnswerButtons()
        {
            if (_exerciseData == null) return;

            int optionCount = _exerciseData.options?.Count ?? 0;

            // Update button texts
            if (answerButtonTexts != null)
            {
                for (int i = 0; i < answerButtonTexts.Count; i++)
                {
                    if (i < optionCount)
                    {
                        answerButtonTexts[i].text = _exerciseData.options[i];
                        answerButtons[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        answerButtons[i].gameObject.SetActive(false);
                    }
                }
            }
            else if (answerButtons != null)
            {
                // Fallback: Get text from button children
                for (int i = 0; i < answerButtons.Count; i++)
                {
                    if (i < optionCount)
                    {
                        var buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = _exerciseData.options[i];
                        }
                        answerButtons[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        answerButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Handles answer selection.
        /// </summary>
        private void OnAnswerSelected(int index)
        {
            if (!isInteractable) return;

            _selectedAnswerIndex = index;
            Debug.Log($"Quiz: Player selected answer {index}");

            // Disable further interaction
            SetInteractable(false);

            // Submit answer
            RaiseAnswerSubmitted(index);
        }

        /// <summary>
        /// Shows visual feedback for the answer.
        /// </summary>
        public override void ShowFeedback(bool isCorrect, object correctAnswer = null)
        {
            if (_exerciseData == null || _answerButtonImages == null) return;

            int correctIndex = _exerciseData.correctOptionIndex;

            // Highlight correct answer in green
            if (correctIndex >= 0 && correctIndex < _answerButtonImages.Count)
            {
                if (_answerButtonImages[correctIndex] != null)
                {
                    _answerButtonImages[correctIndex].color = correctColor;
                }
            }

            // If player was wrong, highlight their choice in red
            if (!isCorrect && _selectedAnswerIndex >= 0 && _selectedAnswerIndex < _answerButtonImages.Count)
            {
                if (_answerButtonImages[_selectedAnswerIndex] != null)
                {
                    _answerButtonImages[_selectedAnswerIndex].color = incorrectColor;
                }
            }
        }

        /// <summary>
        /// Resets the panel to its initial state.
        /// </summary>
        public override void Reset()
        {
            _selectedAnswerIndex = -1;
            ResetButtonColors();
            SetInteractable(true);
        }

        /// <summary>
        /// Resets all button colors to default.
        /// </summary>
        private void ResetButtonColors()
        {
            if (_answerButtonImages == null) return;

            foreach (var image in _answerButtonImages)
            {
                if (image != null)
                {
                    image.color = defaultColor;
                }
            }
        }

        /// <summary>
        /// Sets the interactable state for all buttons.
        /// </summary>
        public override void SetInteractable(bool interactable)
        {
            base.SetInteractable(interactable);

            if (answerButtons == null) return;

            foreach (var button in answerButtons)
            {
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }
    }
}
