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
        [Header("Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(0.8f, 0.8f, 1f);
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color incorrectColor = new Color(0.8f, 0.2f, 0.2f);

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

        private void Awake()
        {
            _backgroundImage = GetComponent<Image>();
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
                _backgroundImage.color = highlightColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Remove highlight
            if (_backgroundImage != null)
            {
                _backgroundImage.color = normalColor;
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
                _backgroundImage.color = normalColor;
            }
            else
            {
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
                _backgroundImage.color = normalColor;
            }
        }
    }
}
