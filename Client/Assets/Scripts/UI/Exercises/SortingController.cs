using System.Collections.Generic;
using Data.Exercises;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Exercises
{
    /// <summary>
    /// Controller for Sorting/Categorization exercise UI.
    /// Manages drag and drop of items into category zones.
    /// </summary>
    public class SortingController : BaseExerciseController
    {
        [Header("Sorting UI Elements")]
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private Transform categoriesContainer;
        [SerializeField] private Button validateButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject draggableItemPrefab;
        [SerializeField] private GameObject dropZonePrefab;

        [Header("Feedback Colors")]
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color incorrectColor = new Color(0.8f, 0.2f, 0.2f);

        private SortingExerciseData _exerciseData;
        private List<DraggableItem> _draggableItems = new List<DraggableItem>();
        private List<DropZone> _dropZones = new List<DropZone>();
        private Dictionary<int, int> _itemPlacements = new Dictionary<int, int>(); // itemIndex -> categoryIndex

        private void Awake()
        {
            if (validateButton != null)
            {
                validateButton.onClick.RemoveAllListeners();
                validateButton.onClick.AddListener(OnValidateClicked);
            }
        }

        /// <summary>
        /// Initializes the controller with Sorting exercise data.
        /// </summary>
        public override void Initialize(BaseExerciseData exerciseData)
        {
            base.Initialize(exerciseData);

            _exerciseData = exerciseData as SortingExerciseData;

            if (_exerciseData == null)
            {
                Debug.LogError("SortingController: Invalid exercise data type!");
                return;
            }

            // Reset state
            _itemPlacements.Clear();

            // Update instruction
            if (instructionText != null)
            {
                instructionText.text = _exerciseData.instruction;
            }

            // Create drop zones for categories
            CreateDropZones();

            // Create draggable items
            CreateDraggableItems();

            // Update validate button state
            UpdateValidateButtonState();
        }

        /// <summary>
        /// Creates drop zones for each category.
        /// </summary>
        private void CreateDropZones()
        {
            ClearDropZones();

            if (categoriesContainer == null || dropZonePrefab == null || _exerciseData == null) return;

            for (int i = 0; i < _exerciseData.categories.Count; i++)
            {
                var categoryData = _exerciseData.categories[i];

                GameObject zoneObj = Instantiate(dropZonePrefab, categoriesContainer);
                zoneObj.SetActive(true);

                DropZone dropZone = zoneObj.GetComponent<DropZone>();
                if (dropZone == null)
                {
                    dropZone = zoneObj.AddComponent<DropZone>();
                }

                dropZone.CategoryIndex = i;
                dropZone.Controller = this;

                // Set category label
                TextMeshProUGUI label = zoneObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = categoryData.categoryName;
                }

                // Set category color
                Image bgImage = zoneObj.GetComponent<Image>();
                if (bgImage != null && categoryData.categoryColor != Color.white)
                {
                    Color color = categoryData.categoryColor;
                    color.a = 0.3f; // Make it semi-transparent
                    bgImage.color = color;
                }

                _dropZones.Add(dropZone);
            }
        }

        /// <summary>
        /// Creates draggable items for each sortable item.
        /// </summary>
        private void CreateDraggableItems()
        {
            ClearDraggableItems();

            if (itemsContainer == null || draggableItemPrefab == null || _exerciseData == null) return;

            // Shuffle items for variety
            List<int> indices = new List<int>();
            for (int i = 0; i < _exerciseData.items.Count; i++)
            {
                indices.Add(i);
            }
            ShuffleList(indices);

            foreach (int i in indices)
            {
                var itemData = _exerciseData.items[i];

                GameObject itemObj = Instantiate(draggableItemPrefab, itemsContainer);
                itemObj.SetActive(true);

                // Ensure it has a DraggableItem component
                DraggableItem draggable = itemObj.GetComponent<DraggableItem>();
                if (draggable == null)
                {
                    draggable = itemObj.AddComponent<DraggableItem>();
                }

                // Ensure it has a CanvasGroup for drag operations
                if (itemObj.GetComponent<CanvasGroup>() == null)
                {
                    itemObj.AddComponent<CanvasGroup>();
                }

                draggable.ItemIndex = i;
                draggable.ItemName = itemData.itemName;
                draggable.Controller = this;

                // Set item label
                TextMeshProUGUI label = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = itemData.itemName;
                }

                // Set item sprite if available
                if (itemData.itemSprite != null)
                {
                    Image itemImage = itemObj.GetComponent<Image>();
                    if (itemImage != null)
                    {
                        itemImage.sprite = itemData.itemSprite;
                    }
                }

                _draggableItems.Add(draggable);
            }
        }

        /// <summary>
        /// Attempts to drop an item into a category.
        /// Called by DraggableItem when drag ends.
        /// </summary>
        /// <param name="item">The item being dropped</param>
        /// <param name="eventData">The pointer event data</param>
        /// <returns>True if the drop was successful</returns>
        public bool TryDropItem(DraggableItem item, PointerEventData eventData)
        {
            if (!isInteractable) return false;

            // Find which drop zone we're over
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                DropZone dropZone = result.gameObject.GetComponent<DropZone>();
                if (dropZone == null)
                {
                    dropZone = result.gameObject.GetComponentInParent<DropZone>();
                }

                if (dropZone != null && _dropZones.Contains(dropZone))
                {
                    // Valid drop!
                    PlaceItemInCategory(item, dropZone);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Places an item in a category zone.
        /// </summary>
        private void PlaceItemInCategory(DraggableItem item, DropZone zone)
        {
            // Update placement tracking
            _itemPlacements[item.ItemIndex] = zone.CategoryIndex;
            item.CurrentCategoryIndex = zone.CategoryIndex;

            // Move item to the zone's container
            item.transform.SetParent(zone.ItemContainer);

            Debug.Log($"Sorting: Placed '{item.ItemName}' in category {zone.CategoryIndex}");

            // Update validate button state
            UpdateValidateButtonState();
        }

        /// <summary>
        /// Handles validate button click.
        /// </summary>
        private void OnValidateClicked()
        {
            if (!isInteractable) return;

            // Check if all items are placed
            if (_itemPlacements.Count < _exerciseData.items.Count)
            {
                Debug.Log("Please sort all items before validating!");
                return;
            }

            Debug.Log($"Sorting: Submitting placements for {_itemPlacements.Count} items");

            // Disable further interaction
            SetInteractable(false);

            // Submit the answer
            RaiseAnswerSubmitted(new Dictionary<int, int>(_itemPlacements));
        }

        /// <summary>
        /// Updates the validate button interactability.
        /// </summary>
        private void UpdateValidateButtonState()
        {
            if (validateButton == null) return;

            // Enable validate only when all items are placed
            bool allPlaced = _itemPlacements.Count >= _exerciseData?.items.Count;
            validateButton.interactable = allPlaced && isInteractable;
        }

        /// <summary>
        /// Shows visual feedback for the player's answer.
        /// </summary>
        public override void ShowFeedback(bool isCorrect, object correctAnswer = null)
        {
            if (_exerciseData == null) return;

            // Show feedback for each item
            foreach (var item in _draggableItems)
            {
                if (_itemPlacements.TryGetValue(item.ItemIndex, out int placedCategory))
                {
                    int correctCategory = _exerciseData.items[item.ItemIndex].correctCategoryIndex;
                    bool itemCorrect = placedCategory == correctCategory;

                    // Color the item based on correctness
                    Image itemImage = item.GetComponent<Image>();
                    if (itemImage != null)
                    {
                        itemImage.color = itemCorrect ? correctColor : incorrectColor;
                    }
                }
            }

            // Show feedback on zones
            foreach (var zone in _dropZones)
            {
                // Check if all items in this zone are correct
                bool? zoneCorrect = null;
                foreach (var kvp in _itemPlacements)
                {
                    if (kvp.Value == zone.CategoryIndex)
                    {
                        bool itemCorrect = _exerciseData.items[kvp.Key].correctCategoryIndex == kvp.Value;
                        if (zoneCorrect == null)
                        {
                            zoneCorrect = itemCorrect;
                        }
                        else if (!itemCorrect)
                        {
                            zoneCorrect = false;
                        }
                    }
                }

                zone.SetFeedbackState(zoneCorrect);
            }
        }

        /// <summary>
        /// Resets the panel to its initial state.
        /// </summary>
        public override void Reset()
        {
            _itemPlacements.Clear();

            // Move all items back to the items container
            foreach (var item in _draggableItems)
            {
                if (item != null)
                {
                    item.ResetPosition(itemsContainer);

                    // Reset color
                    Image itemImage = item.GetComponent<Image>();
                    if (itemImage != null)
                    {
                        itemImage.color = Color.white;
                    }
                }
            }

            // Reset zone states
            foreach (var zone in _dropZones)
            {
                if (zone != null)
                {
                    zone.ResetState();
                }
            }

            UpdateValidateButtonState();
            SetInteractable(true);
        }

        /// <summary>
        /// Clears all drop zones.
        /// </summary>
        private void ClearDropZones()
        {
            foreach (var zone in _dropZones)
            {
                if (zone != null)
                {
                    Destroy(zone.gameObject);
                }
            }
            _dropZones.Clear();
        }

        /// <summary>
        /// Clears all draggable items.
        /// </summary>
        private void ClearDraggableItems()
        {
            foreach (var item in _draggableItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _draggableItems.Clear();
        }

        /// <summary>
        /// Shuffles a list using Fisher-Yates algorithm.
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearDropZones();
            ClearDraggableItems();
        }
    }
}
