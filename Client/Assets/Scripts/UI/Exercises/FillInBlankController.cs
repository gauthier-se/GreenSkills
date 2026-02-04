using System.Collections.Generic;
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
        [SerializeField] private Color blankColor = Color.yellow;
        [SerializeField] private Color filledColor = Color.green;

        [Header("Feedback Colors")]
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color incorrectColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color defaultColor = Color.white;

        private FillInBlankExerciseData _exerciseData;
        private List<Button> _wordButtons = new List<Button>();
        private List<Button> _selectedWordButtons = new List<Button>();
        private List<string> _selectedAnswers = new List<string>();
        private int _currentBlankIndex = 0;

        private void Awake()
        {
            if (validateButton != null)
            {
                validateButton.onClick.RemoveAllListeners();
                validateButton.onClick.AddListener(OnValidateClicked);
            }
        }

        /// <summary>
        /// Initializes the controller with Fill in Blank exercise data.
        /// </summary>
        public override void Initialize(BaseExerciseData exerciseData)
        {
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

            // Disable the selected button
            button.interactable = false;
            _selectedWordButtons.Add(button);

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
            bool allFilled = !_selectedAnswers.Contains(null) &&
                           !_selectedAnswers.Exists(s => string.IsNullOrEmpty(s));

            validateButton.interactable = allFilled && isInteractable;
        }

        /// <summary>
        /// Shows visual feedback for the player's answer.
        /// </summary>
        public override void ShowFeedback(bool isCorrect, object correctAnswer = null)
        {
            if (_exerciseData == null) return;

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

                Color answerColor = thisAnswerCorrect ? correctColor : incorrectColor;
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

            UpdateSentenceDisplay();
            UpdateValidateButtonState();
            SetInteractable(true);
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
            _selectedWordButtons.Clear();
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
