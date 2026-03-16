using System.Collections.Generic;
using Data;
using Data.Exercises;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Exercises
{
    /// <summary>
    /// Controller for Fill in the Blank exercise UI.
    /// Displays a sentence with blanks and word options to select.
    /// </summary>
    public class FillInBlankController : BaseExerciseController
    {
        [Header("Fill in Blank UI Elements")]
        [SerializeField] private TextMeshProUGUI sentenceText;
        [SerializeField] private Transform wordOptionsContainer;
        [SerializeField] private Transform selectedWordsContainer;
        [SerializeField] private Button wordButtonPrefab;
        [SerializeField] private Button validateButton;

        [Header("Blank Display Settings")]
        [SerializeField] private string blankPlaceholder = "_____";

        [Header("Theme")]
        [SerializeField] private UITheme theme;

        private FillInBlankExerciseData _exerciseData;
        private List<Button> _wordButtons = new List<Button>();
        private List<Image> _wordButtonImages = new List<Image>();
        private List<Button> _selectedWordButtons = new List<Button>();
        private List<string> _selectedAnswers = new List<string>();
        private int _currentBlankIndex = 0;

        /// <summary>
        /// Initializes the controller with Fill in Blank exercise data.
        /// </summary>
        public override void Initialize(BaseExerciseData exerciseData)
        {
            // Ensure validate button listener is set up
            // (done here instead of Awake to handle panels that start inactive)
            if (validateButton != null)
            {
                validateButton.onClick.RemoveAllListeners();
                validateButton.onClick.AddListener(OnValidateClicked);
            }

            base.Initialize(exerciseData);

            _exerciseData = exerciseData as FillInBlankExerciseData;

            if (_exerciseData == null)
            {
                Debug.LogError("FillInBlankController: Invalid exercise data type!");
                return;
            }

            // Reset state
            _selectedAnswers.Clear();
            _currentBlankIndex = 0;

            // Initialize with empty answers
            for (int i = 0; i < _exerciseData.BlankCount; i++)
            {
                _selectedAnswers.Add(null);
            }

            // Update sentence display
            UpdateSentenceDisplay();

            // Create word option buttons
            CreateWordButtons();

            // Apply theme styling
            ApplyDefaultButtonStyle();
            ApplyValidateButtonStyle();

            // Update validate button state
            UpdateValidateButtonState();
        }

        /// <summary>
        /// Updates the sentence display with current answers.
        /// </summary>
        private void UpdateSentenceDisplay()
        {
            if (sentenceText == null || _exerciseData == null) return;

            string displayText = _exerciseData.sentenceWithBlanks;

            Color blankColor = theme != null ? theme.primary : Color.yellow;
            Color filledColor = theme != null ? theme.primaryLight : Color.green;

            for (int i = 0; i < _exerciseData.BlankCount; i++)
            {
                string replacement;

                if (_selectedAnswers.Count > i && !string.IsNullOrEmpty(_selectedAnswers[i]))
                {
                    // Show selected word with color
                    replacement = $"<color=#{ColorUtility.ToHtmlStringRGB(filledColor)}><b>{_selectedAnswers[i]}</b></color>";
                }
                else
                {
                    // Show blank placeholder
                    replacement = $"<color=#{ColorUtility.ToHtmlStringRGB(blankColor)}>{blankPlaceholder}</color>";
                }

                displayText = displayText.Replace($"{{{i}}}", replacement);
            }

            sentenceText.text = displayText;
        }

        /// <summary>
        /// Creates word option buttons from the exercise data.
        /// </summary>
        private void CreateWordButtons()
        {
            // Clear existing buttons
            ClearWordButtons();

            if (wordOptionsContainer == null || wordButtonPrefab == null || _exerciseData == null) return;

            // Shuffle word options for variety
            List<string> shuffledOptions = new List<string>(_exerciseData.wordOptions);
            ShuffleList(shuffledOptions);

            foreach (string word in shuffledOptions)
            {
                Button button = Instantiate(wordButtonPrefab, wordOptionsContainer);
                button.gameObject.SetActive(true);

                // Set button text
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = word;
                }

                // Store word in button name for easy retrieval
                button.name = word;

                // Add click listener
                string capturedWord = word; // Capture for closure
                button.onClick.AddListener(() => OnWordSelected(capturedWord, button));

                _wordButtons.Add(button);
                _wordButtonImages.Add(button.GetComponent<Image>());
            }
        }

        /// <summary>
        /// Handles word selection.
        /// </summary>
        private void OnWordSelected(string word, Button button)
        {
            if (!isInteractable) return;

            // Find the first empty blank
            int blankToFill = -1;
            for (int i = 0; i < _selectedAnswers.Count; i++)
            {
                if (string.IsNullOrEmpty(_selectedAnswers[i]))
                {
                    blankToFill = i;
                    break;
                }
            }

            if (blankToFill == -1)
            {
                Debug.Log("All blanks are filled!");
                return;
            }

            // Fill the blank
            _selectedAnswers[blankToFill] = word;

            // Disable the selected button and apply dimmed styling
            button.interactable = false;
            _selectedWordButtons.Add(button);

            int buttonIndex = _wordButtons.IndexOf(button);
            if (buttonIndex >= 0 && theme != null)
            {
                if (buttonIndex < _wordButtonImages.Count && _wordButtonImages[buttonIndex] != null)
                    _wordButtonImages[buttonIndex].color = theme.neutral50;
                SetWordButtonTextColor(buttonIndex, theme.textMuted);
            }

            // Update display
            UpdateSentenceDisplay();
            UpdateValidateButtonState();

            Debug.Log($"FillInBlank: Selected '{word}' for blank {blankToFill + 1}");
        }

        /// <summary>
        /// Handles validate button click.
        /// </summary>
        private void OnValidateClicked()
        {
            if (!isInteractable) return;

            // Check if all blanks are filled
            if (_selectedAnswers.Contains(null) || _selectedAnswers.Exists(s => string.IsNullOrEmpty(s)))
            {
                Debug.Log("Please fill all blanks before validating!");
                return;
            }

            Debug.Log($"FillInBlank: Submitting answers: {string.Join(", ", _selectedAnswers)}");

            // Disable further interaction
            SetInteractable(false);

            // Submit the answer
            RaiseAnswerSubmitted(new List<string>(_selectedAnswers));
        }

        /// <summary>
        /// Updates the validate button interactability.
        /// </summary>
        private void UpdateValidateButtonState()
        {
            if (validateButton == null) return;

            // Enable validate only when all blanks are filled
            bool allFilled = _selectedAnswers.Count > 0 &&
                           !_selectedAnswers.Contains(null) &&
                           !_selectedAnswers.Exists(s => string.IsNullOrEmpty(s));

            validateButton.interactable = allFilled && isInteractable;
        }

        /// <summary>
        /// Shows visual feedback for the player's answer.
        /// </summary>
        public override void ShowFeedback(bool isCorrect, object correctAnswer = null)
        {
            if (_exerciseData == null) return;

            Color feedbackCorrectColor = theme != null ? theme.success : new Color(0.2f, 0.8f, 0.2f);
            Color feedbackIncorrectColor = theme != null ? theme.error : new Color(0.8f, 0.2f, 0.2f);

            string displayText = _exerciseData.sentenceWithBlanks;

            for (int i = 0; i < _exerciseData.BlankCount; i++)
            {
                bool thisAnswerCorrect = false;
                string playerAnswer = _selectedAnswers.Count > i ? _selectedAnswers[i] : "";
                string correct = _exerciseData.correctAnswers[i];

                if (_exerciseData.caseSensitive)
                {
                    thisAnswerCorrect = playerAnswer == correct;
                }
                else
                {
                    thisAnswerCorrect = playerAnswer.Equals(correct, System.StringComparison.OrdinalIgnoreCase);
                }

                Color answerColor = thisAnswerCorrect ? feedbackCorrectColor : feedbackIncorrectColor;
                string displayWord = thisAnswerCorrect ? playerAnswer : $"{playerAnswer} → {correct}";

                displayText = displayText.Replace($"{{{i}}}",
                    $"<color=#{ColorUtility.ToHtmlStringRGB(answerColor)}><b>{displayWord}</b></color>");
            }

            sentenceText.text = displayText;
        }

        /// <summary>
        /// Resets the panel to its initial state.
        /// </summary>
        public override void Reset()
        {
            _selectedAnswers.Clear();
            _currentBlankIndex = 0;

            // Re-enable word buttons
            foreach (var button in _wordButtons)
            {
                if (button != null)
                {
                    button.interactable = true;
                }
            }

            _selectedWordButtons.Clear();

            // Restore theme styling
            ApplyDefaultButtonStyle();

            UpdateSentenceDisplay();
            UpdateValidateButtonState();
            SetInteractable(true);
        }

        /// <summary>
        /// Applies the default theme styling to all word option buttons.
        /// </summary>
        private void ApplyDefaultButtonStyle()
        {
            if (theme == null)
            {
                Debug.LogWarning("FillInBlankController: UITheme is not assigned! Wire MainTheme in the Inspector.");
                return;
            }

            for (int i = 0; i < _wordButtonImages.Count; i++)
            {
                if (_wordButtonImages[i] != null)
                    _wordButtonImages[i].color = theme.bgCard;

                SetWordButtonTextColor(i, theme.textPrimary);
                SetWordButtonOutlineColor(i, theme.borderDefault);
            }

            foreach (var button in _wordButtons)
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
        /// Applies the brand green styling to the validate button.
        /// </summary>
        private void ApplyValidateButtonStyle()
        {
            if (validateButton == null || theme == null) return;

            var image = validateButton.GetComponent<Image>();
            if (image != null)
                image.color = theme.primary;

            var text = validateButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.color = theme.textOnDark;

            var colors = validateButton.colors;
            colors.normalColor = theme.primary;
            colors.highlightedColor = theme.primaryLight;
            colors.pressedColor = theme.primaryDark;
            colors.disabledColor = theme.neutral300;
            validateButton.colors = colors;
        }

        /// <summary>
        /// Clears all word buttons.
        /// </summary>
        private void ClearWordButtons()
        {
            foreach (var button in _wordButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            _wordButtons.Clear();
            _wordButtonImages.Clear();
            _selectedWordButtons.Clear();
        }

        private void SetWordButtonTextColor(int index, Color color)
        {
            if (index < _wordButtons.Count && _wordButtons[index] != null)
            {
                var text = _wordButtons[index].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.color = color;
            }
        }

        private void SetWordButtonOutlineColor(int index, Color color)
        {
            if (index < _wordButtons.Count && _wordButtons[index] != null)
            {
                var outline = _wordButtons[index].GetComponent<Outline>();
                if (outline != null) outline.effectColor = color;
            }
        }

        /// <summary>
        /// Shuffles a list using Fisher-Yates algorithm.
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearWordButtons();
        }
    }
}
