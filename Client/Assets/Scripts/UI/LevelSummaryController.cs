using System;
using System.Collections;
using Data;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Controls the end-of-level summary screen with brand-styled dark header,
    /// animated star reveal, score count-up, gamification rewards, and navigation buttons.
    /// </summary>
    public class LevelSummaryController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Panel")]
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image textureOverlay;
        [SerializeField] private Image categoryAccentBar;

        [Header("Stars")]
        [SerializeField] private Image star1;
        [SerializeField] private Image star2;
        [SerializeField] private Image star3;

        [Header("Score")]
        [SerializeField] private TMP_Text scoreLabel;
        [SerializeField] private TMP_Text scoreText;

        [Header("Gamification Rewards")]
        [SerializeField] private Image xpBadgeImage;
        [SerializeField] private TMP_Text xpText;
        [SerializeField] private Image coinsBadgeImage;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private GameObject levelUpBadge;
        [SerializeField] private TMP_Text levelUpText;
        [SerializeField] private GameObject perfectBadge;
        [SerializeField] private TMP_Text perfectText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Image nextLevelButtonImage;
        [SerializeField] private TMP_Text nextLevelButtonText;

        [SerializeField] private Button replayButton;
        [SerializeField] private Image replayButtonImage;
        [SerializeField] private TMP_Text replayButtonText;
        [SerializeField] private Outline replayButtonOutline;

        [SerializeField] private Button menuButton;
        [SerializeField] private TMP_Text menuButtonText;

        [Header("Theme")]
        [SerializeField] private UITheme theme;

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
        /// <param name="levelTheme">Optional level theme name for category accent coloring.</param>
        public void Show(int score, int starsEarned, bool hasNextLevel, LevelRewards rewards,
            string levelTheme = null)
        {
            _stars = new[] { star1, star2, star3 };
            _starsEarned = starsEarned;
            _targetScore = score;
            _rewards = rewards;

            ResetDisplay();
            ApplyTheme(levelTheme);
            WireButtons(hasNextLevel);

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

        #region Theme

        private void ApplyTheme(string levelTheme)
        {
            if (theme == null) return;

            // Dark background
            if (backgroundImage != null)
                backgroundImage.color = theme.bgDark;

            // Subtle texture overlay
            if (textureOverlay != null)
            {
                Color overlayColor = theme.bgDarkSurface;
                overlayColor.a = 0.4f;
                textureOverlay.color = overlayColor;
            }

            // Category accent bar
            if (categoryAccentBar != null)
            {
                Color accentColor = theme.primary;
                if (!string.IsNullOrEmpty(levelTheme)
                    && Enum.TryParse<Category>(levelTheme, true, out Category category))
                {
                    accentColor = theme.GetCategoryColor(category);
                }

                categoryAccentBar.color = accentColor;
            }

            // Score text
            if (scoreLabel != null)
                scoreLabel.color = theme.textOnDarkMuted;

            if (scoreText != null)
                scoreText.color = theme.textOnDark;

            // Reward texts
            if (xpText != null)
                xpText.color = theme.textOnDark;

            if (coinsText != null)
                coinsText.color = theme.textOnDark;

            // XP badge — brand green
            if (xpBadgeImage != null)
                xpBadgeImage.color = theme.success;

            // Coins badge — gold
            if (coinsBadgeImage != null)
                coinsBadgeImage.color = theme.warning;

            if (levelUpText != null)
                levelUpText.color = theme.textOnDark;

            if (perfectText != null)
                perfectText.color = theme.warning;

            // Next Level button — primary filled
            if (nextLevelButtonImage != null)
                nextLevelButtonImage.color = theme.primary;

            if (nextLevelButtonText != null)
                nextLevelButtonText.color = theme.textOnDark;

            // Replay button — outlined secondary
            if (replayButtonImage != null)
                replayButtonImage.color = theme.bgDark;

            if (replayButtonText != null)
                replayButtonText.color = theme.textOnDarkMuted;

            if (replayButtonOutline != null)
            {
                replayButtonOutline.effectColor = theme.textOnDarkMuted;
                replayButtonOutline.effectDistance = new Vector2(2, -2);
            }

            // Menu button — text only
            if (menuButtonText != null)
                menuButtonText.color = theme.textOnDarkMuted;
        }

        #endregion

        #region Animation

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

        private IEnumerator FadeInPanel()
        {
            if (panelCanvasGroup == null)
                yield break;

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

        private IEnumerator CountUpScore()
        {
            if (scoreText == null)
                yield break;

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

        private IEnumerator RevealStars()
        {
            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] == null)
                    continue;

                if (i < _starsEarned)
                {
                    yield return StartCoroutine(AnimateStarPop(_stars[i], true));
                    yield return new WaitForSeconds(starRevealDelay);
                }
            }
        }

        private IEnumerator AnimateStarPop(Image starImage, bool earned)
        {
            Color earnedColor = theme != null ? theme.warning : new Color(1f, 0.84f, 0f);
            Color emptyColor = theme != null ? theme.neutral300 : new Color(0.4f, 0.4f, 0.4f, 0.5f);

            starImage.color = earned ? earnedColor : emptyColor;

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

        private void ResetDisplay()
        {
            if (panelCanvasGroup != null)
                panelCanvasGroup.alpha = 0f;

            if (scoreText != null)
                scoreText.text = "0";

            Color emptyColor = theme != null ? theme.neutral300 : new Color(0.4f, 0.4f, 0.4f, 0.5f);

            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    _stars[i].color = emptyColor;
                    _stars[i].transform.localScale = i < _starsEarned ? Vector3.zero : Vector3.one;
                }
            }

            if (xpText != null) xpText.text = "";
            if (coinsText != null) coinsText.text = "";
            if (levelUpBadge != null) levelUpBadge.SetActive(false);
            if (perfectBadge != null) perfectBadge.SetActive(false);
        }

        private void ShowRewards()
        {
            if (xpText != null)
                xpText.text = $"+{_rewards.xpEarned} XP";

            if (coinsText != null)
                coinsText.text = $"+{_rewards.coinsEarned}";

            if (levelUpBadge != null)
                levelUpBadge.SetActive(_rewards.didLevelUp);

            if (levelUpText != null && _rewards.didLevelUp)
                levelUpText.text = $"Niveau {_rewards.newPlayerLevel} !";

            if (perfectBadge != null)
                perfectBadge.SetActive(_rewards.isPerfect);
        }

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
