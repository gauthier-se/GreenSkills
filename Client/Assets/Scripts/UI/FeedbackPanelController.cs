using System;
using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// Controls the integrated feedback panel that appears inline within the answer zone
    /// after a player answers an exercise. Displays pedagogical feedback explaining
    /// why an answer is correct or incorrect.
    /// </summary>
    public class FeedbackPanelController : MonoBehaviour
    {
        [Header("Panel References")]
        [Tooltip("Root GameObject for the entire feedback panel")]
        [SerializeField] private GameObject panelRoot;

        [Tooltip("CanvasGroup used for fade-in/out animation")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Content References")]
        [Tooltip("Title text (e.g., 'Bonne reponse !' or 'Mauvaise reponse')")]
        [SerializeField] private TMP_Text titleText;

        [Tooltip("Body text displaying the exercise explanation")]
        [SerializeField] private TMP_Text explanationText;

        [Tooltip("Button to dismiss the panel and continue")]
        [SerializeField] private Button continueButton;

        [Tooltip("Optional icon displayed next to the title")]
        [SerializeField] private Image iconImage;

        [Header("Accent & Styling")]
        [Tooltip("Background Image of the feedback card")]
        [SerializeField] private Image panelBackground;

        [Tooltip("Vertical colored bar on the left edge of the panel")]
        [SerializeField] private Image accentBar;

        [Tooltip("Theme reference for feedback colors")]
        [SerializeField] private UITheme theme;

        [Tooltip("Continue button background image for styling")]
        [SerializeField] private Image continueButtonImage;

        [Tooltip("Continue button text for color styling")]
        [SerializeField] private TMP_Text continueButtonText;

        [Header("Correct Answer Settings")]
        [Tooltip("Title displayed when the answer is correct")]
        [SerializeField] private string correctTitle = "Bonne reponse !";

        [Tooltip("Title displayed when the answer is incorrect")]
        [SerializeField] private string incorrectTitle = "Mauvaise reponse";

        [Tooltip("Optional sprite for correct answer")]
        [SerializeField] private Sprite correctIcon;

        [Tooltip("Optional sprite for incorrect answer")]
        [SerializeField] private Sprite incorrectIcon;

        [Header("Animation Settings")]
        [Tooltip("Duration of the fade-in/out animation in seconds")]
        [SerializeField] private float fadeDuration = 0.3f;

        /// <summary>
        /// Event fired when the player dismisses the feedback panel.
        /// Subscribe to this event to resume the game flow.
        /// </summary>
        public event Action OnPopupDismissed;

        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            // Ensure panel is hidden at start
            HideImmediate();
        }

        /// <summary>
        /// Shows the feedback panel with the given text and correct/incorrect state.
        /// </summary>
        /// <param name="explanation">The explanation text from BaseExerciseData</param>
        /// <param name="wasCorrect">Whether the player's answer was correct</param>
        public void Show(string explanation, bool wasCorrect)
        {
            if (panelRoot == null)
            {
                Debug.LogError("[FeedbackPanelController] panelRoot is null!");
                return;
            }

            Color feedbackColor = GetFeedbackColor(wasCorrect);

            // Set card background color
            if (panelBackground != null && theme != null)
            {
                panelBackground.color = wasCorrect ? theme.successBg : theme.errorBg;
            }

            // Set title
            if (titleText != null)
            {
                titleText.text = wasCorrect ? correctTitle : incorrectTitle;
                titleText.color = feedbackColor;
            }

            // Set explanation body
            if (explanationText != null)
            {
                explanationText.text = explanation ?? "";
            }

            // Set accent bar color
            if (accentBar != null)
            {
                accentBar.color = feedbackColor;
            }

            // Set icon
            if (iconImage != null)
            {
                Sprite icon = wasCorrect ? correctIcon : incorrectIcon;
                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.color = feedbackColor;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            // Style continue button
            if (continueButtonImage != null && theme != null)
            {
                continueButtonImage.color = theme.primary;
            }

            if (continueButtonText != null && theme != null)
            {
                continueButtonText.color = theme.textOnDark;
            }

            // Show with fade-in
            panelRoot.SetActive(true);
            FadeIn();

            Debug.Log($"[FeedbackPanelController] Showing feedback (correct: {wasCorrect})");
        }

        /// <summary>
        /// Hides the panel with a fade-out animation and fires the OnPopupDismissed event.
        /// </summary>
        public void Hide()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeOutAndHide());
        }

        /// <summary>
        /// Checks whether the feedback panel is currently visible.
        /// </summary>
        public bool IsVisible()
        {
            return panelRoot != null && panelRoot.activeSelf;
        }

        /// <summary>
        /// Immediately hides the panel without animation.
        /// </summary>
        public void HideImmediate()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Called when the Continue button is clicked.
        /// </summary>
        private void OnContinueClicked()
        {
            Debug.Log("[FeedbackPanelController] Continue clicked, dismissing panel");
            Hide();
        }

        /// <summary>
        /// Returns the appropriate feedback color from the theme, or a fallback.
        /// </summary>
        private Color GetFeedbackColor(bool wasCorrect)
        {
            if (theme != null)
            {
                return wasCorrect ? theme.success : theme.error;
            }

            // Fallback if theme is not assigned
            return wasCorrect ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
        }

        /// <summary>
        /// Starts the fade-in animation.
        /// </summary>
        private void FadeIn()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            if (canvasGroup != null)
            {
                _fadeCoroutine = StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
            }
        }

        /// <summary>
        /// Fades out the panel, hides it, and fires the dismissed event.
        /// </summary>
        private IEnumerator FadeOutAndHide()
        {
            if (canvasGroup != null)
            {
                yield return FadeCanvasGroup(1f, 0f, fadeDuration);
            }

            HideImmediate();
            OnPopupDismissed?.Invoke();
        }

        /// <summary>
        /// Animates the CanvasGroup alpha from one value to another.
        /// </summary>
        private IEnumerator FadeCanvasGroup(float from, float to, float duration)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            canvasGroup.alpha = from;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            canvasGroup.alpha = to;
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinueClicked);
            }

            OnPopupDismissed = null;
        }
    }
}
