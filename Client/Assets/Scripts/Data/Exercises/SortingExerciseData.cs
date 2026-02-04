using System.Collections.Generic;
using UnityEngine;

namespace Data.Exercises
{
    /// <summary>
    /// Represents an item that can be sorted into a category.
    /// </summary>
    [System.Serializable]
    public class SortableItem
    {
        [Tooltip("Display name of the item")]
        public string itemName;

        [Tooltip("Optional sprite for visual representation")]
        public Sprite itemSprite;

        [Tooltip("Index of the correct category (0-based)")]
        public int correctCategoryIndex;
    }

    /// <summary>
    /// Represents a category/container for sorting items.
    /// </summary>
    [System.Serializable]
    public class SortingCategory
    {
        [Tooltip("Display name of the category")]
        public string categoryName;

        [Tooltip("Optional sprite/icon for the category")]
        public Sprite categoryIcon;

        [Tooltip("Color for visual identification")]
        public Color categoryColor = Color.white;
    }

    /// <summary>
    /// Exercise data for Sorting/Categorization exercises.
    /// Player must drag and drop items into the correct categories.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSortingExercise", menuName = "RSE/Exercises/Sorting")]
    public class SortingExerciseData : BaseExerciseData
    {
        [TextArea(2, 5)]
        [Tooltip("Instruction text for the player")]
        public string instruction;

        [Tooltip("Available categories/containers")]
        public List<SortingCategory> categories;

        [Tooltip("Items to be sorted")]
        public List<SortableItem> items;

        private void OnEnable()
        {
            exerciseType = ExerciseType.Sorting;
        }

        /// <summary>
        /// Gets the number of items to sort.
        /// </summary>
        public int ItemCount => items?.Count ?? 0;

        /// <summary>
        /// Gets the number of categories.
        /// </summary>
        public int CategoryCount => categories?.Count ?? 0;

        /// <summary>
        /// Validates if all items have been sorted into their correct categories.
        /// </summary>
        /// <param name="answer">Dictionary mapping item index to category index</param>
        /// <returns>True if all items are in their correct categories</returns>
        public override bool ValidateAnswer(object answer)
        {
            // Expected format: Dictionary<int, int> where key = item index, value = category index
            if (answer is Dictionary<int, int> sortedItems)
            {
                if (sortedItems.Count != items.Count)
                {
                    return false; // Not all items have been sorted
                }

                foreach (var kvp in sortedItems)
                {
                    int itemIndex = kvp.Key;
                    int placedCategoryIndex = kvp.Value;

                    if (itemIndex < 0 || itemIndex >= items.Count)
                    {
                        return false; // Invalid item index
                    }

                    if (items[itemIndex].correctCategoryIndex != placedCategoryIndex)
                    {
                        return false; // Item in wrong category
                    }
                }

                return true;
            }

            // Alternative format: List of category indices matching item order
            if (answer is List<int> categoryPlacements)
            {
                if (categoryPlacements.Count != items.Count)
                {
                    return false;
                }

                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].correctCategoryIndex != categoryPlacements[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            Debug.LogWarning($"SortingExerciseData.ValidateAnswer received unexpected type: {answer?.GetType()}");
            return false;
        }

        /// <summary>
        /// Returns the instruction as the main text.
        /// </summary>
        public override string GetMainText()
        {
            return instruction;
        }

        /// <summary>
        /// Gets items belonging to a specific category (for displaying solutions).
        /// </summary>
        /// <param name="categoryIndex">The category index</param>
        /// <returns>List of items that belong to this category</returns>
        public List<SortableItem> GetItemsForCategory(int categoryIndex)
        {
            var result = new List<SortableItem>();

            foreach (var item in items)
            {
                if (item.correctCategoryIndex == categoryIndex)
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}
