using Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Exercises
{
    /// <summary>
    /// Represents a matchable item in the matching exercise.
    /// </summary>
    public class MatchableItem : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Index of this item (in original pairs list).
        /// </summary>
        public int PairIndex { get; set; }

        /// <summary>
        /// Whether this item is on the left (true) or right (false) column.
        /// </summary>
        public bool IsLeftColumn { get; set; }

        /// <summary>
        /// Whether this item is currently selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// The index of the matched item (-1 if not matched).
        /// </summary>
        public int MatchedWithIndex { get; set; } = -1;

        /// <summary>
        /// Reference to the controller.
        /// </summary>
        public MatchingController Controller { get; set; }

        private UITheme _theme;
        private Image _backgroundImage;
        private TextMeshProUGUI _label;

        // Fallback colors when theme is not assigned
        private static readonly Color FallbackNormal = Color.white;
        private static readonly Color FallbackSelected = new Color(0.7f, 0.7f, 1f);
        private static readonly Color FallbackMatched = new Color(0.7f, 1f, 0.7f);
        private static readonly Color FallbackCorrect = new Color(0.2f, 0.8f, 0.2f);
        private static readonly Color FallbackIncorrect = new Color(0.8f, 0.2f, 0.2f);

        private void Awake()
        {
            _backgroundImage = GetComponent<Image>();
            _label = GetComponentInChildren<TextMeshProUGUI>();
        }

        /// <summary>
        /// Sets the UITheme for theme-driven styling.
        /// </summary>
        public void SetTheme(UITheme theme)
        {
            _theme = theme;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Controller?.OnItemClicked(this);
        }

        /// <summary>
        /// Sets the visual state of the item.
        /// </summary>
        public void SetVisualState(MatchableItemState state)
        {
            if (_backgroundImage == null) return;

            if (_theme != null)
            {
                _backgroundImage.color = state switch
                {
                    MatchableItemState.Normal => _theme.bgCard,
                    MatchableItemState.Selected => _theme.primarySubtle,
                    MatchableItemState.Matched => new Color(_theme.primaryLight.r, _theme.primaryLight.g, _theme.primaryLight.b, 0.3f),
                    MatchableItemState.Correct => _theme.success,
                    MatchableItemState.Incorrect => _theme.error,
                    _ => _theme.bgCard
                };

                if (_label != null)
                {
                    bool useWhiteText = state == MatchableItemState.Correct || state == MatchableItemState.Incorrect;
                    _label.color = useWhiteText ? _theme.textOnDark : _theme.textPrimary;
                }
            }
            else
            {
                _backgroundImage.color = state switch
                {
                    MatchableItemState.Normal => FallbackNormal,
                    MatchableItemState.Selected => FallbackSelected,
                    MatchableItemState.Matched => FallbackMatched,
                    MatchableItemState.Correct => FallbackCorrect,
                    MatchableItemState.Incorrect => FallbackIncorrect,
                    _ => FallbackNormal
                };
            }
        }

        /// <summary>
        /// Resets the item state.
        /// </summary>
        public void ResetState()
        {
            IsSelected = false;
            MatchedWithIndex = -1;
            SetVisualState(MatchableItemState.Normal);
        }
    }

    public enum MatchableItemState
    {
        Normal,
        Selected,
        Matched,
        Correct,
        Incorrect
    }
}
