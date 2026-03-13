using System.Collections;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Controls the end-of-level summary screen with animated star reveal,
    /// score count-up, gamification rewards display, and navigation buttons.
    /// </summary>
    public class LevelSummaryController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panel")]
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Stars")]
        [SerializeField] private Image star1;
        [SerializeField] private Image star2;
        [SerializeField] private Image star3;
        [SerializeField] private Color starEarnedColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color starEmptyColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

        [Header("Score")]
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Gamification Rewards")]
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private GameObject levelUpBadge;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private GameObject perfectBadge;

        [Header("Navigation Buttons")]
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button menuButton;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.4f;
        [SerializeField] private float starRevealDelay = 0.3f;
        [SerializeField] private float starAnimationDuration = 0.3f;
        [SerializeField] private float scoreCountUpDuration = 0.8f;

        #endregion

        #region State

        private Image[] _stars;
        private int _starsEarned;
        private int _targetScore;
        private LevelRewards _rewards;

        #endregion

        #region Public API

        /// <summary>
        /// Shows the summary screen with animated reveal of results.
        /// </summary>
        /// <param name="score">The player's score for this level.</param>
        /// <param name="starsEarned">Number of stars earned (1-3).</param>
        /// <param name="hasNextLevel">Whether there's a next level available.</param>
        /// <param name="rewards">Gamification rewards earned from this level.</param>
        public void Show(int score, int starsEarned, bool hasNextLevel, LevelRewards rewards)
        {
            _stars = new[] { star1, star2, star3 };
            _starsEarned = starsEarned;
            _targetScore = score;
            _rewards = rewards;

            // Reset state before animating
            ResetDisplay();

            // Wire navigation buttons
            WireButtons(hasNextLevel);

            // Show the panel and start animations
            gameObject.SetActive(true);
            StartCoroutine(AnimateSummary());

            Debug.Log($"[LevelSummaryController] Showing summary — Score: {score}, Stars: {starsEarned}");
        }

        /// <summary>
        /// Hides the summary screen.
        /// </summary>
        public void Hide()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        #endregion

        #region Animation

        /// <summary>
        /// Orchestrates the full summary reveal animation sequence.
        /// </summary>
        private IEnumerator AnimateSummary()
        {
            // Phase 1: Fade in the panel
            yield return StartCoroutine(FadeInPanel());

            // Phase 2: Count up the score
            yield return StartCoroutine(CountUpScore());

            // Phase 3: Reveal stars one by one
            yield return StartCoroutine(RevealStars());

            // Phase 4: Show gamification rewards
            ShowRewards();
        }

        /// <summary>
        /// Fades the panel in from transparent to fully opaque.
        /// </summary>
        private IEnumerator FadeInPanel()
        {
            if (panelCanvasGroup == null)
            {
                yield break;
            }

            panelCanvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }

            panelCanvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Animates the score text counting up from 0 to the target score.
        /// </summary>
        private IEnumerator CountUpScore()
        {
            if (scoreText == null)
            {
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < scoreCountUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / scoreCountUpDuration);
                // Ease-out curve for a satisfying deceleration
                float easedT = 1f - (1f - t) * (1f - t);
                int displayScore = Mathf.RoundToInt(easedT * _targetScore);
                scoreText.text = $"{displayScore}";
                yield return null;
            }

            scoreText.text = $"{_targetScore}";
        }

        /// <summary>
        /// Reveals stars sequentially with a pop scale animation.
        /// </summary>
        private IEnumerator RevealStars()
        {
            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] == null)
                {
                    continue;
                }

                if (i < _starsEarned)
                {
                    yield return StartCoroutine(AnimateStarPop(_stars[i], true));
                    yield return new WaitForSeconds(starRevealDelay);
                }
            }
        }

        /// <summary>
        /// Animates a single star with a scale pop effect (scale up then settle).
        /// </summary>
        private IEnumerator AnimateStarPop(Image starImage, bool earned)
        {
            starImage.color = earned ? starEarnedColor : starEmptyColor;

            Transform starTransform = starImage.transform;
            starTransform.localScale = Vector3.zero;

            float elapsed = 0f;
            float overshoot = 1.2f;

            // Scale up with overshoot
            float halfDuration = starAnimationDuration * 0.6f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                float scale = t * overshoot;
                starTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            // Settle back to normal scale
            elapsed = 0f;
            float settleDuration = starAnimationDuration * 0.4f;
            while (elapsed < settleDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / settleDuration);
                float scale = Mathf.Lerp(overshoot, 1f, t);
                starTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            starTransform.localScale = Vector3.one;
        }

        #endregion

        #region Display Helpers

        /// <summary>
        /// Resets all display elements to their initial state before animating.
        /// </summary>
        private void ResetDisplay()
        {
            // Reset canvas group
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
            }

            // Reset score
            if (scoreText != null)
            {
                scoreText.text = "0";
            }

            // Reset stars to empty and zero scale
            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    _stars[i].color = starEmptyColor;
                    _stars[i].transform.localScale = i < _starsEarned ? Vector3.zero : Vector3.one;
                }
            }

            // Hide reward elements
            if (xpText != null) xpText.text = "";
            if (coinsText != null) coinsText.text = "";
            if (levelUpBadge != null) levelUpBadge.SetActive(false);
            if (perfectBadge != null) perfectBadge.SetActive(false);
        }

        /// <summary>
        /// Shows gamification rewards after star animation.
        /// </summary>
        private void ShowRewards()
        {
            if (xpText != null)
            {
                xpText.text = $"+{_rewards.xpEarned} XP";
            }

            if (coinsText != null)
            {
                coinsText.text = $"+{_rewards.coinsEarned}";
            }

            if (levelUpBadge != null)
            {
                levelUpBadge.SetActive(_rewards.didLevelUp);
            }

            if (levelUpText != null && _rewards.didLevelUp)
            {
                levelUpText.text = $"Niveau {_rewards.newPlayerLevel} !";
            }

            if (perfectBadge != null)
            {
                perfectBadge.SetActive(_rewards.isPerfect);
            }
        }

        /// <summary>
        /// Wires up navigation buttons with click handlers.
        /// </summary>
        private void WireButtons(bool hasNextLevel)
        {
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(hasNextLevel);
                nextLevelButton.onClick.RemoveAllListeners();
                nextLevelButton.onClick.AddListener(() =>
                {
                    Hide();
                    GameManager.Instance.LoadNextLevel();
                });
            }

            if (replayButton != null)
            {
                replayButton.onClick.RemoveAllListeners();
                replayButton.onClick.AddListener(() =>
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

        #endregion
    }
}
