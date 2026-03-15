using System.Collections.Generic;
using Data;
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
        [SerializeField] private List<Button> answerButtons;
        [SerializeField] private List<TextMeshProUGUI> answerButtonTexts;

        [Header("Theme")]
        [SerializeField] private UITheme theme;

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

            // Update answer buttons
            UpdateAnswerButtons();

            // Apply default theme styling
            ApplyDefaultButtonStyle();
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
            if (_exerciseData == null || _answerButtonImages == null || theme == null) return;

            int correctIndex = _exerciseData.correctOptionIndex;

            for (int i = 0; i < _answerButtonImages.Count; i++)
            {
                if (_answerButtonImages[i] == null) continue;

                if (i == correctIndex)
                {
                    _answerButtonImages[i].color = theme.success;
                    SetButtonTextColor(i, theme.textOnDark);
                }
                else if (!isCorrect && i == _selectedAnswerIndex)
                {
                    _answerButtonImages[i].color = theme.error;
                    SetButtonTextColor(i, theme.textOnDark);
                }
                else
                {
                    _answerButtonImages[i].color = theme.neutral50;
                    SetButtonTextColor(i, theme.textMuted);
                }

                SetButtonOutlineColor(i, Color.clear);
            }
        }

        /// <summary>
        /// Resets the panel to its initial state.
        /// </summary>
        public override void Reset()
        {
            _selectedAnswerIndex = -1;
            ApplyDefaultButtonStyle();
            SetInteractable(true);
        }

        /// <summary>
        /// Applies the default theme styling to all buttons.
        /// </summary>
        private void ApplyDefaultButtonStyle()
        {
            if (_answerButtonImages == null || theme == null)
            {
                if (theme == null)
                    Debug.LogWarning("QuizController: UITheme is not assigned! Wire MainTheme in the Inspector.");
                return;
            }

            for (int i = 0; i < _answerButtonImages.Count; i++)
            {
                if (_answerButtonImages[i] != null)
                    _answerButtonImages[i].color = theme.bgCard;

                SetButtonTextColor(i, theme.textPrimary);
                SetButtonOutlineColor(i, theme.borderDefault);
            }

            foreach (var button in answerButtons)
            {
                if (button == null) continue;
                var colors = button.colors;
                colors.normalColor = theme.bgCard;
                colors.highlightedColor = theme.bgSurface;
                colors.pressedColor = theme.borderStrong;
                colors.disabledColor = theme.neutral50;
                button.colors = colors;
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

        private void SetButtonTextColor(int index, Color color)
        {
            if (answerButtonTexts != null && index < answerButtonTexts.Count && answerButtonTexts[index] != null)
                answerButtonTexts[index].color = color;
        }

        private void SetButtonOutlineColor(int index, Color color)
        {
            if (index < answerButtons.Count && answerButtons[index] != null)
            {
                var outline = answerButtons[index].GetComponent<Outline>();
                if (outline != null) outline.effectColor = color;
            }
        }
    }
}
