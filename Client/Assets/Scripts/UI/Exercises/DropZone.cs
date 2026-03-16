using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Exercises
{
    /// <summary>
    /// Component that represents a drop zone for sorting exercises.
    /// Items can be dropped here to categorize them.
    /// </summary>
    public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Fallback Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(0.8f, 0.8f, 1f);

        /// <summary>
        /// Index of this category in the exercise data.
        /// </summary>
        public int CategoryIndex { get; set; }

        /// <summary>
        /// Reference to the controller managing this zone.
        /// </summary>
        public SortingController Controller { get; set; }

        /// <summary>
        /// Transform where dropped items should be parented.
        /// </summary>
        public Transform ItemContainer => _itemContainer != null ? _itemContainer : transform;

        [SerializeField] private Transform _itemContainer;
        private Image _backgroundImage;
        private UITheme _theme;

        private void Awake()
        {
            _backgroundImage = GetComponent<Image>();
        }

        /// <summary>
        /// Sets the UITheme for themed styling.
        /// </summary>
        public void SetTheme(UITheme uiTheme)
        {
            _theme = uiTheme;
        }

        public void OnDrop(PointerEventData eventData)
        {
            // The actual drop handling is done in DraggableItem.OnEndDrag
            // via Controller.TryDropItem
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Highlight when dragging over
            if (eventData.dragging && _backgroundImage != null)
            {
                _backgroundImage.color = _theme != null ? _theme.bgSurface : highlightColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Remove highlight
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _theme != null ? _theme.bgCard : normalColor;
            }
        }

        /// <summary>
        /// Sets the visual state to show correct/incorrect feedback.
        /// </summary>
        public void SetFeedbackState(bool? isCorrect)
        {
            if (_backgroundImage == null) return;

            if (isCorrect == null)
            {
                _backgroundImage.color = _theme != null ? _theme.bgCard : normalColor;
            }
            else
            {
                Color correctColor = _theme != null ? _theme.success : new Color(0.2f, 0.8f, 0.2f);
                Color incorrectColor = _theme != null ? _theme.error : new Color(0.8f, 0.2f, 0.2f);
                _backgroundImage.color = isCorrect.Value ? correctColor : incorrectColor;
            }
        }

        /// <summary>
        /// Resets the visual state.
        /// </summary>
        public void ResetState()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _theme != null ? _theme.bgCard : normalColor;
            }
        }
    }
}
