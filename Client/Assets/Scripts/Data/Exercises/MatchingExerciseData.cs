using System.Collections.Generic;
using UnityEngine;

namespace Data.Exercises
{
    /// <summary>
    /// Represents a pair of items to be matched.
    /// </summary>
    [System.Serializable]
    public class MatchPair
    {
        [Tooltip("Item on the left side (e.g., an action)")]
        public string leftItem;

        [Tooltip("Item on the right side (e.g., an impact/result)")]
        public string rightItem;

        [Tooltip("Optional sprite for left item")]
        public Sprite leftSprite;

        [Tooltip("Optional sprite for right item")]
        public Sprite rightSprite;
    }

    /// <summary>
    /// Exercise data for Matching/Connecting exercises.
    /// Player must draw lines connecting related items from two columns.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMatchingExercise", menuName = "RSE/Exercises/Matching")]
    public class MatchingExerciseData : BaseExerciseData
    {
        [TextArea(2, 5)]
        [Tooltip("Instruction text for the player")]
        public string instruction;

        [Tooltip("Header for the left column")]
        public string leftColumnHeader = "Actions";

        [Tooltip("Header for the right column")]
        public string rightColumnHeader = "Impacts";

        [Tooltip("Pairs of items to be matched")]
        public List<MatchPair> pairs;

        [Tooltip("If true, right column items will be shuffled when displayed")]
        public bool shuffleRightColumn = true;

        private void OnEnable()
        {
            exerciseType = ExerciseType.Matching;
        }

        /// <summary>
        /// Gets the number of pairs to match.
        /// </summary>
        public int PairCount => pairs?.Count ?? 0;

        /// <summary>
        /// Validates if all pairs have been correctly matched.
        /// </summary>
        /// <param name="answer">Dictionary mapping left item index to right item index</param>
        /// <returns>True if all matches are correct</returns>
        public override bool ValidateAnswer(object answer)
        {
            // Expected format: Dictionary<int, int> where key = left index, value = right index
            // The indices refer to the original pairs list (before any shuffling)
            if (answer is Dictionary<int, int> matches)
            {
                if (matches.Count != pairs.Count)
                {
                    return false; // Not all pairs matched
                }

                // Each left item should be matched to its corresponding right item
                // In the original pairs list, index i on left matches index i on right
                foreach (var kvp in matches)
                {
                    int leftIndex = kvp.Key;
                    int rightIndex = kvp.Value;

                    if (leftIndex != rightIndex)
                    {
                        return false; // Wrong match
                    }
                }

                return true;
            }

            // Alternative format: List where index = left item, value = matched right item index
            if (answer is List<int> matchList)
            {
                if (matchList.Count != pairs.Count)
                {
                    return false;
                }

                for (int i = 0; i < pairs.Count; i++)
                {
                    if (matchList[i] != i)
                    {
                        return false;
                    }
                }

                return true;
            }

            Debug.LogWarning($"MatchingExerciseData.ValidateAnswer received unexpected type: {answer?.GetType()}");
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
        /// Gets all left column items in order.
        /// </summary>
        public List<string> GetLeftItems()
        {
            var result = new List<string>();
            foreach (var pair in pairs)
            {
                result.Add(pair.leftItem);
            }
            return result;
        }

        /// <summary>
        /// Gets all right column items in order.
        /// </summary>
        public List<string> GetRightItems()
        {
            var result = new List<string>();
            foreach (var pair in pairs)
            {
                result.Add(pair.rightItem);
            }
            return result;
        }

        /// <summary>
        /// Gets right column items with a shuffle map for display.
        /// Returns the shuffled list and a mapping from shuffled index to original index.
        /// </summary>
        /// <param name="shuffleMap">Output: maps shuffled index to original pair index</param>
        /// <returns>Shuffled list of right items</returns>
        public List<string> GetShuffledRightItems(out int[] shuffleMap)
        {
            var rightItems = GetRightItems();
            shuffleMap = new int[rightItems.Count];

            // Initialize shuffle map
            for (int i = 0; i < shuffleMap.Length; i++)
            {
                shuffleMap[i] = i;
            }

            if (!shuffleRightColumn)
            {
                return rightItems;
            }

            // Fisher-Yates shuffle
            for (int i = rightItems.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);

                // Swap items
                (rightItems[i], rightItems[j]) = (rightItems[j], rightItems[i]);
                (shuffleMap[i], shuffleMap[j]) = (shuffleMap[j], shuffleMap[i]);
            }

            return rightItems;
        }
    }
}
