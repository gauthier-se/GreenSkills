using Data;
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
        [SerializeField] private Button trueButton;
        [SerializeField] private Button falseButton;
        [SerializeField] private TextMeshProUGUI trueButtonText;
        [SerializeField] private TextMeshProUGUI falseButtonText;

        [Header("Theme")]
        [SerializeField] private UITheme theme;

        private Image _trueButtonImage;
        private Image _falseButtonImage;
        private TrueFalseExerciseData _exerciseData;
        private bool _selectedTrue;

        private void Awake()
        {
            CacheButtonImages();
            SetupButtonListeners();
        }

        private void CacheButtonImages()
        {
            if (_trueButtonImage == null && trueButton != null)
            {
                _trueButtonImage = trueButton.GetComponent<Image>();
            }

            if (_falseButtonImage == null && falseButton != null)
            {
                _falseButtonImage = falseButton.GetComponent<Image>();
            }
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

            // Set button texts (can be customized)
            if (trueButtonText != null)
            {
                trueButtonText.text = "VRAI";
            }

            if (falseButtonText != null)
            {
                falseButtonText.text = "FAUX";
            }

            // Apply default theme styling
            ApplyDefaultButtonStyle();
        }

        /// <summary>
        /// Handles button click events.
        /// </summary>
        private void OnButtonClicked(bool selectedTrue)
        {
            if (!isInteractable) return;

            _selectedTrue = selectedTrue;
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
            if (_exerciseData == null || theme == null) return;

            CacheButtonImages();
            bool correctValue = _exerciseData.isTrue;

            // Correct answer button → success styling
            if (correctValue)
            {
                // True is the correct answer
                if (_trueButtonImage != null) _trueButtonImage.color = theme.success;
                SetButtonTextColor(true, theme.textOnDark);

                if (isCorrect)
                {
                    // Player chose correctly (True) — dim false button
                    if (_falseButtonImage != null) _falseButtonImage.color = theme.neutral50;
                    SetButtonTextColor(false, theme.textMuted);
                }
                else
                {
                    // Player chose wrong (False) — show error on false button
                    if (_falseButtonImage != null) _falseButtonImage.color = theme.error;
                    SetButtonTextColor(false, theme.textOnDark);
                }
            }
            else
            {
                // False is the correct answer
                if (_falseButtonImage != null) _falseButtonImage.color = theme.success;
                SetButtonTextColor(false, theme.textOnDark);

                if (isCorrect)
                {
                    // Player chose correctly (False) — dim true button
                    if (_trueButtonImage != null) _trueButtonImage.color = theme.neutral50;
                    SetButtonTextColor(true, theme.textMuted);
                }
                else
                {
                    // Player chose wrong (True) — show error on true button
                    if (_trueButtonImage != null) _trueButtonImage.color = theme.error;
                    SetButtonTextColor(true, theme.textOnDark);
                }
            }

            // Clear outlines on both buttons
            SetButtonOutlineColor(true, Color.clear);
            SetButtonOutlineColor(false, Color.clear);
        }

        /// <summary>
        /// Resets the panel to its initial state.
        /// </summary>
        public override void Reset()
        {
            ApplyDefaultButtonStyle();
            SetInteractable(true);
        }

        /// <summary>
        /// Applies the default theme styling to both buttons.
        /// </summary>
        private void ApplyDefaultButtonStyle()
        {
            if (theme == null)
            {
                Debug.LogWarning("TrueFalseController: UITheme is not assigned! Wire MainTheme in the Inspector.");
                return;
            }

            CacheButtonImages();

            if (_trueButtonImage != null)
                _trueButtonImage.color = theme.bgCard;
            if (_falseButtonImage != null)
                _falseButtonImage.color = theme.bgCard;

            SetButtonTextColor(true, theme.textPrimary);
            SetButtonTextColor(false, theme.textPrimary);

            SetButtonOutlineColor(true, theme.borderDefault);
            SetButtonOutlineColor(false, theme.borderDefault);

            // Update button color blocks
            foreach (var button in new[] { trueButton, falseButton })
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

        private void SetButtonTextColor(bool isTrue, Color color)
        {
            var text = isTrue ? trueButtonText : falseButtonText;
            if (text != null) text.color = color;
        }

        private void SetButtonOutlineColor(bool isTrue, Color color)
        {
            var button = isTrue ? trueButton : falseButton;
            if (button != null)
            {
                var outline = button.GetComponent<Outline>();
                if (outline != null) outline.effectColor = color;
            }
        }
    }
}
