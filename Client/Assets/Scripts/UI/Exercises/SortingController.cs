using System.Collections.Generic;
using Data;
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
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private Transform categoriesContainer;
        [SerializeField] private Button validateButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject draggableItemPrefab;
        [SerializeField] private GameObject dropZonePrefab;

        [Header("Theme")]
        [SerializeField] private UITheme theme;

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

            // Create drop zones for categories
            CreateDropZones();

            // Create draggable items
            CreateDraggableItems();

            // Apply theme styling
            ApplyDefaultItemStyle();
            ApplyDefaultDropZoneStyle();
            ApplyValidateButtonStyle();

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

                // Pass theme to the drop zone
                if (theme != null)
                {
                    dropZone.SetTheme(theme);
                }

                // Set category label
                TextMeshProUGUI label = zoneObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = categoryData.categoryName;
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

            if (itemsContainer == null || draggableItemPrefab == null || _exerciseData == null)
            {
                Debug.LogError($"SortingController.CreateDraggableItems: EARLY RETURN — itemsContainer={itemsContainer}, draggableItemPrefab={draggableItemPrefab}, _exerciseData={_exerciseData}");
                return;
            }

            Debug.Log($"SortingController: Creating {_exerciseData.items.Count} draggable items into container '{itemsContainer.name}'");

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

            Color feedbackCorrectColor = theme != null ? theme.success : new Color(0.2f, 0.8f, 0.2f);
            Color feedbackIncorrectColor = theme != null ? theme.error : new Color(0.8f, 0.2f, 0.2f);
            Color feedbackTextColor = theme != null ? theme.textOnDark : Color.white;

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
                        itemImage.color = itemCorrect ? feedbackCorrectColor : feedbackIncorrectColor;
                    }

                    // Update text color for readability on colored background
                    TextMeshProUGUI itemText = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (itemText != null)
                    {
                        itemText.color = feedbackTextColor;
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

            // Restore theme styling
            ApplyDefaultItemStyle();

            UpdateValidateButtonState();
            SetInteractable(true);
        }

        /// <summary>
        /// Applies the default theme styling to all draggable items.
        /// </summary>
        private void ApplyDefaultItemStyle()
        {
            if (theme == null)
            {
                Debug.LogWarning("SortingController: UITheme is not assigned! Wire MainTheme in the Inspector.");
                return;
            }

            foreach (var item in _draggableItems)
            {
                if (item == null) continue;

                Image itemImage = item.GetComponent<Image>();
                if (itemImage != null)
                    itemImage.color = theme.bgCard;

                TextMeshProUGUI itemText = item.GetComponentInChildren<TextMeshProUGUI>();
                if (itemText != null)
                    itemText.color = theme.textPrimary;

                Outline outline = item.GetComponent<Outline>();
                if (outline != null)
                    outline.effectColor = theme.borderDefault;
            }
        }

        /// <summary>
        /// Applies the default theme styling to all drop zones.
        /// </summary>
        private void ApplyDefaultDropZoneStyle()
        {
            if (theme == null) return;

            foreach (var zone in _dropZones)
            {
                if (zone == null) continue;

                Image zoneImage = zone.GetComponent<Image>();
                if (zoneImage != null)
                    zoneImage.color = theme.bgCard;

                Outline outline = zone.GetComponent<Outline>();
                if (outline != null)
                    outline.effectColor = theme.borderStrong;

                TextMeshProUGUI label = zone.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.color = theme.textPrimary;
            }
        }

        /// <summary>
        /// Applies the brand green styling to the validate button.
        /// </summary>
        private void ApplyValidateButtonStyle()
        {
            if (validateButton == null || theme == null) return;

            var image = validateButton.GetComponent<Image>();
            if (image != null)
                image.color = theme.primary;

            var text = validateButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.color = theme.textOnDark;

            var colors = validateButton.colors;
            colors.normalColor = theme.primary;
            colors.highlightedColor = theme.primaryLight;
            colors.pressedColor = theme.primaryDark;
            colors.disabledColor = theme.neutral300;
            validateButton.colors = colors;
        }

        /// <summary>
        /// Clears all drop zones, including any pre-existing children in the container.
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

            // Destroy any leftover children (e.g. template "New Text" ghost items)
            if (categoriesContainer != null)
                for (int i = categoriesContainer.childCount - 1; i >= 0; i--)
                {
                    var child = categoriesContainer.GetChild(i).gameObject;
                    child.SetActive(false);
                    Destroy(child);
                }
        }

        /// <summary>
        /// Clears all draggable items, including any pre-existing children in the container.
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

            // Destroy any leftover children (e.g. template "New Text" ghost items)
            if (itemsContainer != null)
                for (int i = itemsContainer.childCount - 1; i >= 0; i--)
                {
                    var child = itemsContainer.GetChild(i).gameObject;
                    child.SetActive(false);
                    Destroy(child);
                }
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
