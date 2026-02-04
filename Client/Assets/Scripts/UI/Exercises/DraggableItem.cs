using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Exercises
{
    /// <summary>
    /// Component that makes a UI element draggable.
    /// Attach this to items that should be drag-and-droppable.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Settings")]
        [SerializeField] private float dragAlpha = 0.6f;

        /// <summary>
        /// Index of this item in the exercise data.
        /// </summary>
        public int ItemIndex { get; set; }

        /// <summary>
        /// The name/content of this item.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// The category index where this item is currently placed (-1 if not placed).
        /// </summary>
        public int CurrentCategoryIndex { get; set; } = -1;

        /// <summary>
        /// Reference to the controller managing this item.
        /// </summary>
        public SortingController Controller { get; set; }

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector3 _originalPosition;
        private Transform _originalParent;
        private Canvas _canvas;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Store original position and parent
            _originalPosition = _rectTransform.position;
            _originalParent = transform.parent;

            // Make the item render on top
            transform.SetParent(_canvas.transform);

            // Make item semi-transparent and allow raycasts through
            _canvasGroup.alpha = dragAlpha;
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Move with the pointer
            _rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Restore visual state
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

            // Check if dropped on a valid zone
            bool validDrop = Controller != null && Controller.TryDropItem(this, eventData);

            if (!validDrop)
            {
                // Return to original position
                transform.SetParent(_originalParent);
                _rectTransform.position = _originalPosition;
            }
        }

        /// <summary>
        /// Resets the item to its original state.
        /// </summary>
        public void ResetPosition(Transform parent)
        {
            transform.SetParent(parent);
            CurrentCategoryIndex = -1;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}
