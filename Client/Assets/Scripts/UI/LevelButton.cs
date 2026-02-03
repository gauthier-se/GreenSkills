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
        
        [Header("Visual States")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        private int _levelId;
        private bool _isUnlocked;

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
        public void Initialize(int id, bool unlocked, System.Action<int> onClickCallback)
        {
            _levelId = id;
            _isUnlocked = unlocked;
            
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
            
            // Show/hide lock icon
            if (lockIcon != null)
            {
                lockIcon.SetActive(!unlocked);
            }
            
            // Set background color
            if (backgroundImage != null)
            {
                backgroundImage.color = unlocked ? unlockedColor : lockedColor;
            }
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
            
            if (backgroundImage != null)
            {
                backgroundImage.color = unlocked ? unlockedColor : lockedColor;
            }
        }

        /// <summary>
        /// Displays a star rating for completed levels (future feature).
        /// </summary>
        /// <param name="stars">Number of stars earned (0-3).</param>
        public void ShowStars(int stars)
        {
            // TODO: Implement star display system
            // For example: activate star GameObjects based on the count
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
