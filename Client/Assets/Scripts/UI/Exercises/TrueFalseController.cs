using Data.Exercises;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Exercises
{
    /// <summary>
    /// Controller for True/False exercise UI.
    /// Displays a statement and two buttons (True/False) for the player to choose.
    /// </summary>
    public class TrueFalseController : BaseExerciseController
    {
        [Header("True/False UI Elements")]
        [SerializeField] private TextMeshProUGUI statementText;
        [SerializeField] private Button trueButton;
        [SerializeField] private Button falseButton;
        [SerializeField] private TextMeshProUGUI trueButtonText;
        [SerializeField] private TextMeshProUGUI falseButtonText;

        [Header("Feedback Colors")]
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color incorrectColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color defaultColor = Color.white;

        private Image _trueButtonImage;
        private Image _falseButtonImage;
        private TrueFalseExerciseData _exerciseData;

        private void Awake()
        {
            // Cache button images
            if (trueButton != null)
            {
                _trueButtonImage = trueButton.GetComponent<Image>();
            }

            if (falseButton != null)
            {
                _falseButtonImage = falseButton.GetComponent<Image>();
            }

            // Setup button listeners
            SetupButtonListeners();
        }

        private void SetupButtonListeners()
        {
            if (trueButton != null)
            {
                trueButton.onClick.RemoveAllListeners();
                trueButton.onClick.AddListener(() => OnButtonClicked(true));
            }

            if (falseButton != null)
            {
                falseButton.onClick.RemoveAllListeners();
                falseButton.onClick.AddListener(() => OnButtonClicked(false));
            }
        }

        /// <summary>
        /// Initializes the controller with True/False exercise data.
        /// </summary>
        public override void Initialize(BaseExerciseData exerciseData)
        {
            base.Initialize(exerciseData);

            _exerciseData = exerciseData as TrueFalseExerciseData;

            if (_exerciseData == null)
            {
                Debug.LogError("TrueFalseController: Invalid exercise data type!");
                return;
            }

            // Update UI
            if (statementText != null)
            {
                statementText.text = _exerciseData.statement;
            }

            // Set button texts (can be customized)
            if (trueButtonText != null)
            {
                trueButtonText.text = "VRAI";
            }

            if (falseButtonText != null)
            {
                falseButtonText.text = "FAUX";
            }

            // Reset colors
            ResetButtonColors();
        }

        /// <summary>
        /// Handles button click events.
        /// </summary>
        private void OnButtonClicked(bool selectedTrue)
        {
            if (!isInteractable) return;

            Debug.Log($"TrueFalse: Player selected {(selectedTrue ? "TRUE" : "FALSE")}");

            // Disable further interaction
            SetInteractable(false);

            // Submit the answer
            RaiseAnswerSubmitted(selectedTrue);
        }

        /// <summary>
        /// Shows visual feedback for the player's answer.
        /// </summary>
        public override void ShowFeedback(bool isCorrect, object correctAnswer = null)
        {
            if (_exerciseData == null) return;

            bool correctValue = _exerciseData.isTrue;

            // Highlight buttons based on correctness
            if (_trueButtonImage != null)
            {
                _trueButtonImage.color = correctValue ? correctColor : incorrectColor;
            }

            if (_falseButtonImage != null)
            {
                _falseButtonImage.color = correctValue ? incorrectColor : correctColor;
            }

            // If the player was wrong, make the wrong choice more obvious
            if (!isCorrect)
            {
                // The player's choice was the opposite of the correct answer
                // So we need to show which one was their (wrong) choice
                if (_exerciseData.isTrue)
                {
                    // Correct was TRUE, player chose FALSE
                    if (_falseButtonImage != null)
                    {
                        // Add visual indication this was their wrong choice
                        _falseButtonImage.color = incorrectColor;
                    }
                }
                else
                {
                    // Correct was FALSE, player chose TRUE
                    if (_trueButtonImage != null)
                    {
                        _trueButtonImage.color = incorrectColor;
                    }
                }
            }
        }

        /// <summary>
        /// Resets the panel to its initial state.
        /// </summary>
        public override void Reset()
        {
            ResetButtonColors();
            SetInteractable(true);
        }

        /// <summary>
        /// Resets button colors to default.
        /// </summary>
        private void ResetButtonColors()
        {
            if (_trueButtonImage != null)
            {
                _trueButtonImage.color = defaultColor;
            }

            if (_falseButtonImage != null)
            {
                _falseButtonImage.color = defaultColor;
            }
        }

        /// <summary>
        /// Sets the interactable state for the buttons.
        /// </summary>
        public override void SetInteractable(bool interactable)
        {
            base.SetInteractable(interactable);

            if (trueButton != null)
            {
                trueButton.interactable = interactable;
            }

            if (falseButton != null)
            {
                falseButton.interactable = interactable;
            }
        }
    }
}
