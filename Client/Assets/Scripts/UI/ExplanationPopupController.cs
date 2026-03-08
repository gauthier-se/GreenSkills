using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// Controls the explanation popup that appears after a player answers an exercise.
    /// Displays pedagogical feedback explaining why an answer is correct or incorrect.
    /// </summary>
    public class ExplanationPopupController : MonoBehaviour
    {
        [Header("Panel References")]
        [Tooltip("Root GameObject for the entire popup (overlay + content)")]
        [SerializeField] private GameObject panelRoot;

        [Tooltip("CanvasGroup used for fade-in/out animation")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Content References")]
        [Tooltip("Title text (e.g., 'Bonne reponse !' or 'Mauvaise reponse')")]
        [SerializeField] private TMP_Text titleText;

        [Tooltip("Body text displaying the exercise explanation")]
        [SerializeField] private TMP_Text explanationText;

        [Tooltip("Button to dismiss the popup and continue")]
        [SerializeField] private Button continueButton;

        [Tooltip("Optional icon displayed next to the title")]
        [SerializeField] private Image iconImage;

        [Header("Correct Answer Settings")]
        [Tooltip("Title displayed when the answer is correct")]
        [SerializeField] private string correctTitle = "Bonne reponse !";

        [Tooltip("Title displayed when the answer is incorrect")]
        [SerializeField] private string incorrectTitle = "Mauvaise reponse";

        [Tooltip("Color for the title when correct")]
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);

        [Tooltip("Color for the title when incorrect")]
        [SerializeField] private Color incorrectColor = new Color(0.9f, 0.2f, 0.2f);

        [Tooltip("Optional sprite for correct answer")]
        [SerializeField] private Sprite correctIcon;

        [Tooltip("Optional sprite for incorrect answer")]
        [SerializeField] private Sprite incorrectIcon;

        [Header("Animation Settings")]
        [Tooltip("Duration of the fade-in/out animation in seconds")]
        [SerializeField] private float fadeDuration = 0.3f;

        /// <summary>
        /// Event fired when the player dismisses the popup.
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

            // Ensure popup is hidden at start
            HideImmediate();
        }

        /// <summary>
        /// Shows the explanation popup with the given text and correct/incorrect state.
        /// </summary>
        /// <param name="explanation">The explanation text from BaseExerciseData</param>
        /// <param name="wasCorrect">Whether the player's answer was correct</param>
        public void Show(string explanation, bool wasCorrect)
        {
            if (panelRoot == null)
            {
                Debug.LogError("[ExplanationPopupController] panelRoot is null!");
                return;
            }

            // Set title
            if (titleText != null)
            {
                titleText.text = wasCorrect ? correctTitle : incorrectTitle;
                titleText.color = wasCorrect ? correctColor : incorrectColor;
            }

            // Set explanation body
            if (explanationText != null)
            {
                explanationText.text = explanation ?? "";
            }

            // Set icon
            if (iconImage != null)
            {
                Sprite icon = wasCorrect ? correctIcon : incorrectIcon;
                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.color = wasCorrect ? correctColor : incorrectColor;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            // Show with fade-in
            panelRoot.SetActive(true);
            FadeIn();

            Debug.Log($"[ExplanationPopupController] Showing explanation (correct: {wasCorrect})");
        }

        /// <summary>
        /// Hides the popup with a fade-out animation and fires the OnPopupDismissed event.
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
        /// Checks whether the popup is currently visible.
        /// </summary>
        public bool IsVisible()
        {
            return panelRoot != null && panelRoot.activeSelf;
        }

        /// <summary>
        /// Called when the Continue button is clicked.
        /// </summary>
        private void OnContinueClicked()
        {
            Debug.Log("[ExplanationPopupController] Continue clicked, dismissing popup");
            Hide();
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
        /// Fades out the popup, hides it, and fires the dismissed event.
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

        /// <summary>
        /// Immediately hides the popup without animation.
        /// </summary>
        private void HideImmediate()
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
