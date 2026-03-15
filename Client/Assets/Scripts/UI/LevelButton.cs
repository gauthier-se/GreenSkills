using Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// Represents a single level button in the level selection grid.
    /// Manages its visual state (locked/unlocked) and displays level information.
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private Image backgroundImage;

        [Header("Card Design")]
        [SerializeField] private Image numberBadge;
        [SerializeField] private Image categoryAccentBar;

        [Header("Visual States")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Header("Stars")]
        [SerializeField] private Image star1;
        [SerializeField] private Image star2;
        [SerializeField] private Image star3;
        [SerializeField] private Color starEarnedColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color starEmptyColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        
        private int _levelId;
        private bool _isUnlocked;
        private Category _category;
        private UITheme _theme;

        /// <summary>
        /// Gets the level ID associated with this button.
        /// </summary>
        public int LevelId => _levelId;

        /// <summary>
        /// Gets whether this level is currently unlocked.
        /// </summary>
        public bool IsUnlocked => _isUnlocked;

        /// <summary>
        /// Initializes the level button with its ID and lock state.
        /// </summary>
        /// <param name="id">The level ID (1-based).</param>
        /// <param name="unlocked">Whether this level is unlocked.</param>
        /// <param name="onClickCallback">Callback to execute when the button is clicked.</param>
        /// <param name="category">The dominant category of the level.</param>
        /// <param name="theme">Optional UITheme for category-colored backgrounds.</param>
        public void Initialize(int id, bool unlocked, System.Action<int> onClickCallback,
            Category category = Category.Environment, UITheme theme = null)
        {
            _levelId = id;
            _isUnlocked = unlocked;
            _category = category;
            _theme = theme;

            // Set the level number text
            if (levelNumberText != null)
            {
                levelNumberText.text = id.ToString();
            }

            // Configure button state
            if (button != null)
            {
                button.interactable = unlocked;

                // Clear existing listeners and add new one
                button.onClick.RemoveAllListeners();
                if (unlocked && onClickCallback != null)
                {
                    button.onClick.AddListener(() => onClickCallback(_levelId));
                }
            }

            // Show/hide lock icon and level number
            if (lockIcon != null)
            {
                lockIcon.SetActive(!unlocked);
            }

            if (levelNumberText != null)
            {
                levelNumberText.gameObject.SetActive(unlocked);
            }

            // Set background color
            ApplyBackgroundColor(unlocked);

            if (unlocked)
                ShowStars(Managers.LevelScoreManager.GetLevelStars(id));
            else
                HideStars();
        }

        /// <summary>
        /// Updates the button's visual state (useful for refreshing after progression changes).
        /// </summary>
        /// <param name="unlocked">New unlock state.</param>
        public void UpdateLockState(bool unlocked)
        {
            _isUnlocked = unlocked;

            if (button != null)
            {
                button.interactable = unlocked;
            }

            if (lockIcon != null)
            {
                lockIcon.SetActive(!unlocked);
            }

            ApplyBackgroundColor(unlocked);

            if (unlocked)
                ShowStars(Managers.LevelScoreManager.GetLevelStars(_levelId));
            else
                HideStars();
        }

        private void ApplyBackgroundColor(bool unlocked)
        {
            if (backgroundImage == null) return;

            if (_theme != null)
            {
                Color categoryColor = _theme.GetCategoryColor(_category);
                Color subtleColor = _theme.GetCategorySubtleColor(_category);

                if (unlocked)
                {
                    backgroundImage.color = subtleColor;

                    if (categoryAccentBar != null)
                    {
                        categoryAccentBar.gameObject.SetActive(true);
                        categoryAccentBar.color = categoryColor;
                    }

                    if (numberBadge != null)
                        numberBadge.color = categoryColor;

                    if (levelNumberText != null)
                        levelNumberText.color = Color.white;
                }
                else
                {
                    backgroundImage.color = _theme.neutral100;

                    if (categoryAccentBar != null)
                        categoryAccentBar.gameObject.SetActive(false);

                    if (numberBadge != null)
                        numberBadge.color = _theme.neutral300;

                    if (levelNumberText != null)
                        levelNumberText.color = _theme.neutral500;
                }
            }
            else
            {
                backgroundImage.color = unlocked ? unlockedColor : lockedColor;
            }
        }

        /// <summary>
        /// Displays a star rating for completed levels.
        /// </summary>
        /// <param name="stars">Number of stars earned (0-3).</param>
        public void ShowStars(int stars)
        {
            Image[] starImages = { star1, star2, star3 };
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;
                starImages[i].gameObject.SetActive(true);
                starImages[i].color = i < stars ? starEarnedColor : starEmptyColor;
            }
        }

        private void HideStars()
        {
            Image[] starImages = { star1, star2, star3 };
            foreach (var img in starImages)
            {
                if (img != null) img.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Plays a visual effect when the button is clicked (future feature).
        /// </summary>
        public void PlayClickAnimation()
        {
            // TODO: Implement click animation
            // For example: scale up/down, particle effect, sound
        }
    }
}
