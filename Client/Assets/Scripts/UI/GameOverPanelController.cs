using System.Collections;
using Data;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Controls the Game Over panel with brand-styled dark background,
    /// score summary, motivational message, and navigation buttons.
    /// </summary>
    public class GameOverPanelController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image textureOverlay;

        [Header("Title")]
        [SerializeField] private TMP_Text titleText;

        [Header("Score Summary")]
        [SerializeField] private TMP_Text scoreLabel;
        [SerializeField] private TMP_Text scoreValue;

        [Header("Motivational Message")]
        [SerializeField] private TMP_Text motivationalText;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Image retryButtonImage;
        [SerializeField] private TMP_Text retryButtonText;

        [SerializeField] private Button menuButton;
        [SerializeField] private Image menuButtonImage;
        [SerializeField] private TMP_Text menuButtonText;
        [SerializeField] private Outline menuButtonOutline;

        [Header("Theme")]
        [SerializeField] private UITheme theme;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.4f;

        [Header("Content")]
        [SerializeField] private string gameOverTitle = "Game Over";
        [SerializeField] private string retryLabel = "Reessayer";
        [SerializeField] private string menuLabel = "Menu";

        private static readonly string[] MotivationalMessages =
        {
            "Chaque erreur est une lecon. Retente ta chance !",
            "La perseverance est la cle du succes !",
            "On apprend toujours de ses erreurs. Recommence !",
            "Ne lache rien, tu peux y arriver !",
            "L'important, c'est de progresser !"
        };

        /// <summary>
        /// Shows the Game Over panel with score summary and styled UI.
        /// </summary>
        /// <param name="exercisesCompleted">Number of exercises answered before game over.</param>
        /// <param name="totalExercises">Total number of exercises in the level.</param>
        public void Show(int exercisesCompleted, int totalExercises)
        {
            ApplyTheme();
            SetContent(exercisesCompleted, totalExercises);
            WireButtons();

            gameObject.SetActive(true);
            StartCoroutine(FadeIn());

            Debug.Log($"[GameOverPanelController] Showing — {exercisesCompleted}/{totalExercises} completed");
        }

        /// <summary>
        /// Hides the Game Over panel immediately.
        /// </summary>
        public void Hide()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        private void ApplyTheme()
        {
            if (theme == null) return;

            // Dark background
            if (backgroundImage != null)
                backgroundImage.color = theme.bgDark;

            // Subtle texture overlay (semi-transparent dark surface)
            if (textureOverlay != null)
            {
                Color overlayColor = theme.bgDarkSurface;
                overlayColor.a = 0.4f;
                textureOverlay.color = overlayColor;
            }

            // Title with error accent
            if (titleText != null)
                titleText.color = theme.error;

            // Score label and value
            if (scoreLabel != null)
                scoreLabel.color = theme.textOnDarkMuted;

            if (scoreValue != null)
                scoreValue.color = theme.textOnDark;

            // Motivational message
            if (motivationalText != null)
                motivationalText.color = theme.textOnDarkMuted;

            // Retry button — primary filled
            if (retryButtonImage != null)
                retryButtonImage.color = theme.primary;

            if (retryButtonText != null)
                retryButtonText.color = theme.textOnDark;

            // Menu button — outlined secondary (dark bg + light border)
            if (menuButtonImage != null)
                menuButtonImage.color = theme.bgDark;

            if (menuButton != null)
            {
                ColorBlock cb = menuButton.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
                cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                menuButton.colors = cb;
            }

            if (menuButtonText != null)
                menuButtonText.color = theme.textOnDarkMuted;

            if (menuButtonOutline != null)
            {
                menuButtonOutline.effectColor = theme.textOnDarkMuted;
                menuButtonOutline.effectDistance = new Vector2(2, -2);
            }
        }

        private void SetContent(int exercisesCompleted, int totalExercises)
        {
            if (titleText != null)
                titleText.text = gameOverTitle;

            if (scoreLabel != null)
                scoreLabel.text = "Progression";

            if (scoreValue != null)
                scoreValue.text = $"{exercisesCompleted} / {totalExercises}";

            if (motivationalText != null)
                motivationalText.text = MotivationalMessages[Random.Range(0, MotivationalMessages.Length)];

            if (retryButtonText != null)
                retryButtonText.text = retryLabel;

            if (menuButtonText != null)
                menuButtonText.text = menuLabel;
        }

        private void WireButtons()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(() =>
                {
                    Hide();
                    GameManager.Instance.RestartCurrentLevel();
                });
            }

            if (menuButton != null)
            {
                menuButton.onClick.RemoveAllListeners();
                menuButton.onClick.AddListener(() =>
                {
                    Hide();
                    GameManager.Instance.ReturnToMenu();
                });
            }
        }

        private IEnumerator FadeIn()
        {
            if (panelCanvasGroup == null)
                yield break;

            panelCanvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }

            panelCanvasGroup.alpha = 1f;
        }
    }
}
