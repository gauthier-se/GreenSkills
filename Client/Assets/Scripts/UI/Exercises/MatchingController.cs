using System.Collections.Generic;
using Data.Exercises;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Exercises
{
    /// <summary>
    /// Controller for Matching exercise UI.
    /// Players connect items from left column to right column.
    /// </summary>
    public class MatchingController : BaseExerciseController
    {
        [Header("Matching UI Elements")]
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI leftHeaderText;
        [SerializeField] private TextMeshProUGUI rightHeaderText;
        [SerializeField] private Transform leftColumnContainer;
        [SerializeField] private Transform rightColumnContainer;
        [SerializeField] private Button validateButton;
        [SerializeField] private Button clearButton;

        [Header("Line Drawing")]
        [SerializeField] private RectTransform linesContainer;
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private Color lineColor = Color.blue;
        [SerializeField] private float lineWidth = 4f;

        [Header("Prefabs")]
        [SerializeField] private GameObject matchableItemPrefab;

        [Header("Feedback Colors")]
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color incorrectColor = new Color(0.8f, 0.2f, 0.2f);

        private MatchingExerciseData _exerciseData;
        private List<MatchableItem> _leftItems = new List<MatchableItem>();
        private List<MatchableItem> _rightItems = new List<MatchableItem>();
        private List<GameObject> _connectionLines = new List<GameObject>();

        // Maps: left item index -> right item index (original indices)
        private Dictionary<int, int> _matches = new Dictionary<int, int>();

        // Shuffle map for right column
        private int[] _rightShuffleMap;

        // Currently selected item
        private MatchableItem _selectedItem;

        private void Awake()
        {
            if (validateButton != null)
            {
                validateButton.onClick.RemoveAllListeners();
                validateButton.onClick.AddListener(OnValidateClicked);
            }

            if (clearButton != null)
            {
                clearButton.onClick.RemoveAllListeners();
                clearButton.onClick.AddListener(ClearAllMatches);
            }
        }

        /// <summary>
        /// Initializes the controller with Matching exercise data.
        /// </summary>
        public override void Initialize(BaseExerciseData exerciseData)
        {
            base.Initialize(exerciseData);

            _exerciseData = exerciseData as MatchingExerciseData;

            if (_exerciseData == null)
            {
                Debug.LogError("MatchingController: Invalid exercise data type!");
                return;
            }

            // Reset state
            _matches.Clear();
            _selectedItem = null;

            // Update headers
            if (instructionText != null)
            {
                instructionText.text = _exerciseData.instruction;
            }

            if (leftHeaderText != null)
            {
                leftHeaderText.text = _exerciseData.leftColumnHeader;
            }

            if (rightHeaderText != null)
            {
                rightHeaderText.text = _exerciseData.rightColumnHeader;
            }

            // Create items
            CreateLeftItems();
            CreateRightItems();

            // Clear any existing lines
            ClearConnectionLines();

            // Update validate button state
            UpdateValidateButtonState();
        }

        /// <summary>
        /// Creates left column items.
        /// </summary>
        private void CreateLeftItems()
        {
            ClearItems(_leftItems);

            if (leftColumnContainer == null || matchableItemPrefab == null || _exerciseData == null) return;

            var leftTexts = _exerciseData.GetLeftItems();

            for (int i = 0; i < leftTexts.Count; i++)
            {
                CreateMatchableItem(leftTexts[i], i, true, leftColumnContainer, _leftItems);
            }
        }

        /// <summary>
        /// Creates right column items (shuffled).
        /// </summary>
        private void CreateRightItems()
        {
            ClearItems(_rightItems);

            if (rightColumnContainer == null || matchableItemPrefab == null || _exerciseData == null) return;

            var rightTexts = _exerciseData.GetShuffledRightItems(out _rightShuffleMap);

            for (int i = 0; i < rightTexts.Count; i++)
            {
                // The PairIndex should be the ORIGINAL index, not the shuffled one
                CreateMatchableItem(rightTexts[i], _rightShuffleMap[i], false, rightColumnContainer, _rightItems);
            }
        }

        /// <summary>
        /// Creates a matchable item.
        /// </summary>
        private void CreateMatchableItem(string text, int pairIndex, bool isLeft, Transform container, List<MatchableItem> list)
        {
            GameObject itemObj = Instantiate(matchableItemPrefab, container);
            itemObj.SetActive(true);

            MatchableItem item = itemObj.GetComponent<MatchableItem>();
            if (item == null)
            {
                item = itemObj.AddComponent<MatchableItem>();
            }

            item.PairIndex = pairIndex;
            item.IsLeftColumn = isLeft;
            item.Controller = this;

            // Set text
            TextMeshProUGUI label = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = text;
            }

            list.Add(item);
        }

        /// <summary>
        /// Handles item click events.
        /// </summary>
        public void OnItemClicked(MatchableItem clickedItem)
        {
            if (!isInteractable) return;

            // If clicking an already matched item, remove the match
            if (clickedItem.MatchedWithIndex >= 0)
            {
                RemoveMatch(clickedItem);
                return;
            }

            // If no item selected, select this one
            if (_selectedItem == null)
            {
                SelectItem(clickedItem);
                return;
            }

            // If clicking the same item, deselect
            if (_selectedItem == clickedItem)
            {
                DeselectItem();
                return;
            }

            // If clicking item in same column, switch selection
            if (_selectedItem.IsLeftColumn == clickedItem.IsLeftColumn)
            {
                DeselectItem();
                SelectItem(clickedItem);
                return;
            }

            // Clicking item in other column - create match!
            CreateMatch(_selectedItem, clickedItem);
        }

        /// <summary>
        /// Selects an item.
        /// </summary>
        private void SelectItem(MatchableItem item)
        {
            _selectedItem = item;
            item.IsSelected = true;
            item.SetVisualState(MatchableItemState.Selected);
        }

        /// <summary>
        /// Deselects the current item.
        /// </summary>
        private void DeselectItem()
        {
            if (_selectedItem != null)
            {
                _selectedItem.IsSelected = false;
                _selectedItem.SetVisualState(
                    _selectedItem.MatchedWithIndex >= 0
                        ? MatchableItemState.Matched
                        : MatchableItemState.Normal
                );
                _selectedItem = null;
            }
        }

        /// <summary>
        /// Creates a match between two items.
        /// </summary>
        private void CreateMatch(MatchableItem item1, MatchableItem item2)
        {
            MatchableItem leftItem = item1.IsLeftColumn ? item1 : item2;
            MatchableItem rightItem = item1.IsLeftColumn ? item2 : item1;

            // Store the match (left index -> right index, using original indices)
            _matches[leftItem.PairIndex] = rightItem.PairIndex;

            // Update item states
            leftItem.MatchedWithIndex = rightItem.PairIndex;
            rightItem.MatchedWithIndex = leftItem.PairIndex;

            leftItem.IsSelected = false;
            rightItem.IsSelected = false;

            leftItem.SetVisualState(MatchableItemState.Matched);
            rightItem.SetVisualState(MatchableItemState.Matched);

            _selectedItem = null;

            Debug.Log($"Matching: Connected left[{leftItem.PairIndex}] with right[{rightItem.PairIndex}]");

            // Draw connection line
            DrawConnectionLine(leftItem, rightItem);

            // Update button state
            UpdateValidateButtonState();
        }

        /// <summary>
        /// Removes a match.
        /// </summary>
        private void RemoveMatch(MatchableItem item)
        {
            int otherIndex = item.MatchedWithIndex;

            // Find the other item
            MatchableItem otherItem = null;
            List<MatchableItem> searchList = item.IsLeftColumn ? _rightItems : _leftItems;

            foreach (var searchItem in searchList)
            {
                if (searchItem.PairIndex == otherIndex)
                {
                    otherItem = searchItem;
                    break;
                }
            }

            // Remove from matches dictionary
            if (item.IsLeftColumn)
            {
                _matches.Remove(item.PairIndex);
            }
            else if (otherItem != null)
            {
                _matches.Remove(otherItem.PairIndex);
            }

            // Reset states
            item.ResetState();
            if (otherItem != null)
            {
                otherItem.ResetState();
            }

            // Redraw all lines
            RedrawConnectionLines();

            // Update button state
            UpdateValidateButtonState();
        }

        /// <summary>
        /// Clears all matches.
        /// </summary>
        private void ClearAllMatches()
        {
            if (!isInteractable) return;

            _matches.Clear();
            _selectedItem = null;

            foreach (var item in _leftItems)
            {
                item.ResetState();
            }

            foreach (var item in _rightItems)
            {
                item.ResetState();
            }

            ClearConnectionLines();
            UpdateValidateButtonState();
        }

        /// <summary>
        /// Draws a connection line between two items.
        /// </summary>
        private void DrawConnectionLine(MatchableItem leftItem, MatchableItem rightItem)
        {
            if (linesContainer == null || linePrefab == null) return;

            GameObject lineObj = Instantiate(linePrefab, linesContainer);
            lineObj.SetActive(true);

            // Get positions
            RectTransform leftRect = leftItem.GetComponent<RectTransform>();
            RectTransform rightRect = rightItem.GetComponent<RectTransform>();

            if (leftRect == null || rightRect == null) return;

            // Store references for later (for redrawing)
            lineObj.name = $"Line_{leftItem.PairIndex}_{rightItem.PairIndex}";

            // Position and rotate the line
            UpdateLinePosition(lineObj, leftRect, rightRect);

            _connectionLines.Add(lineObj);
        }

        /// <summary>
        /// Updates a line's position between two points.
        /// </summary>
        private void UpdateLinePosition(GameObject lineObj, RectTransform from, RectTransform to)
        {
            RectTransform lineRect = lineObj.GetComponent<RectTransform>();
            Image lineImage = lineObj.GetComponent<Image>();

            if (lineRect == null) return;

            // Get world positions
            Vector3 fromPos = from.position;
            Vector3 toPos = to.position;

            // Calculate midpoint and distance
            Vector3 midpoint = (fromPos + toPos) / 2f;
            float distance = Vector3.Distance(fromPos, toPos);

            // Calculate angle
            Vector3 direction = toPos - fromPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Apply
            lineRect.position = midpoint;
            lineRect.sizeDelta = new Vector2(distance, lineWidth);
            lineRect.rotation = Quaternion.Euler(0, 0, angle);

            if (lineImage != null)
            {
                lineImage.color = lineColor;
            }
        }

        /// <summary>
        /// Redraws all connection lines.
        /// </summary>
        private void RedrawConnectionLines()
        {
            ClearConnectionLines();

            foreach (var kvp in _matches)
            {
                int leftIndex = kvp.Key;
                int rightIndex = kvp.Value;

                MatchableItem leftItem = _leftItems.Find(i => i.PairIndex == leftIndex);
                MatchableItem rightItem = _rightItems.Find(i => i.PairIndex == rightIndex);

                if (leftItem != null && rightItem != null)
                {
                    DrawConnectionLine(leftItem, rightItem);
                }
            }
        }

        /// <summary>
        /// Clears all connection lines.
        /// </summary>
        private void ClearConnectionLines()
        {
            foreach (var line in _connectionLines)
            {
                if (line != null)
                {
                    Destroy(line);
                }
            }
            _connectionLines.Clear();
        }

        /// <summary>
        /// Handles validate button click.
        /// </summary>
        private void OnValidateClicked()
        {
            if (!isInteractable) return;

            // Check if all pairs are matched
            if (_matches.Count < _exerciseData.pairs.Count)
            {
                Debug.Log("Please match all items before validating!");
                return;
            }

            Debug.Log($"Matching: Submitting {_matches.Count} matches");

            // Disable further interaction
            SetInteractable(false);

            // Submit the answer
            RaiseAnswerSubmitted(new Dictionary<int, int>(_matches));
        }

        /// <summary>
        /// Updates the validate button interactability.
        /// </summary>
        private void UpdateValidateButtonState()
        {
            if (validateButton == null) return;

            bool allMatched = _matches.Count >= _exerciseData?.pairs.Count;
            validateButton.interactable = allMatched && isInteractable;
        }

        /// <summary>
        /// Shows visual feedback for the player's answer.
        /// </summary>
        public override void ShowFeedback(bool isCorrect, object correctAnswer = null)
        {
            if (_exerciseData == null) return;

            // Show feedback for each match
            foreach (var kvp in _matches)
            {
                int leftIndex = kvp.Key;
                int rightIndex = kvp.Value;

                // Correct match is when left index equals right index (original pairs)
                bool matchCorrect = leftIndex == rightIndex;

                MatchableItem leftItem = _leftItems.Find(i => i.PairIndex == leftIndex);
                MatchableItem rightItem = _rightItems.Find(i => i.PairIndex == rightIndex);

                if (leftItem != null)
                {
                    leftItem.SetVisualState(matchCorrect ? MatchableItemState.Correct : MatchableItemState.Incorrect);
                }

                if (rightItem != null)
                {
                    rightItem.SetVisualState(matchCorrect ? MatchableItemState.Correct : MatchableItemState.Incorrect);
                }
            }

            // Update line colors
            foreach (var line in _connectionLines)
            {
                Image lineImage = line?.GetComponent<Image>();
                if (lineImage != null)
                {
                    // Parse line name to get indices
                    string[] parts = line.name.Split('_');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int leftIdx) && int.TryParse(parts[2], out int rightIdx))
                    {
                        bool matchCorrect = leftIdx == rightIdx;
                        lineImage.color = matchCorrect ? correctColor : incorrectColor;
                    }
                }
            }
        }

        /// <summary>
        /// Resets the panel to its initial state.
        /// </summary>
        public override void Reset()
        {
            _matches.Clear();
            _selectedItem = null;

            foreach (var item in _leftItems)
            {
                item?.ResetState();
            }

            foreach (var item in _rightItems)
            {
                item?.ResetState();
            }

            ClearConnectionLines();
            UpdateValidateButtonState();
            SetInteractable(true);
        }

        /// <summary>
        /// Clears items from a list.
        /// </summary>
        private void ClearItems(List<MatchableItem> items)
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            items.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearItems(_leftItems);
            ClearItems(_rightItems);
            ClearConnectionLines();
        }
    }
}
