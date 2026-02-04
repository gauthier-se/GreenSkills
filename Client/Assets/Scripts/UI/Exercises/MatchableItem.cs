using System.Collections.Generic;
using Data.Exercises;
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

        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(0.7f, 0.7f, 1f);
        [SerializeField] private Color matchedColor = new Color(0.7f, 1f, 0.7f);
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color incorrectColor = new Color(0.8f, 0.2f, 0.2f);

        private Image _backgroundImage;

        private void Awake()
        {
            _backgroundImage = GetComponent<Image>();
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

            _backgroundImage.color = state switch
            {
                MatchableItemState.Normal => normalColor,
                MatchableItemState.Selected => selectedColor,
                MatchableItemState.Matched => matchedColor,
                MatchableItemState.Correct => correctColor,
                MatchableItemState.Incorrect => incorrectColor,
                _ => normalColor
            };
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
