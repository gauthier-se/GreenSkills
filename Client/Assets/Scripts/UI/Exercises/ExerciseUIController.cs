using System;
using System.Collections.Generic;
using Data.Exercises;
using UnityEngine;

namespace UI.Exercises
{
    /// <summary>
    /// Central controller that manages which exercise UI panel is displayed.
    /// Handles the switching between different exercise types and forwards events to GameManager.
    /// </summary>
    public class ExerciseUIController : MonoBehaviour
    {
        [Header("Exercise Panels")]
        [Tooltip("Reference to the Quiz panel controller")]
        [SerializeField] private BaseExerciseController quizController;

        [Tooltip("Reference to the True/False panel controller")]
        [SerializeField] private TrueFalseController trueFalseController;

        [Tooltip("Reference to the Fill in Blank panel controller")]
        [SerializeField] private FillInBlankController fillInBlankController;

        [Tooltip("Reference to the Sorting panel controller")]
        [SerializeField] private SortingController sortingController;

        [Tooltip("Reference to the Matching panel controller")]
        [SerializeField] private MatchingController matchingController;

        /// <summary>
        /// Event triggered when an answer is submitted by any exercise controller.
        /// Parameters: exercise data, player's answer
        /// </summary>
        public event Action<BaseExerciseData, object> OnAnswerSubmitted;

        /// <summary>
        /// Event triggered when the current exercise is completed.
        /// </summary>
        public event Action OnExerciseCompleted;

        private Dictionary<ExerciseType, IExerciseController> _controllers;
        private IExerciseController _currentController;
        private BaseExerciseData _currentExercise;

        private void Awake()
        {
            InitializeControllers();
            HideAllPanels();
        }

        /// <summary>
        /// Initializes the controller dictionary.
        /// </summary>
        private void InitializeControllers()
        {
            _controllers = new Dictionary<ExerciseType, IExerciseController>();

            // Register controllers
            if (quizController != null)
            {
                RegisterController(ExerciseType.Quiz, quizController);
            }

            if (trueFalseController != null)
            {
                RegisterController(ExerciseType.TrueFalse, trueFalseController);
            }

            if (fillInBlankController != null)
            {
                RegisterController(ExerciseType.FillInBlank, fillInBlankController);
            }

            if (sortingController != null)
            {
                RegisterController(ExerciseType.Sorting, sortingController);
            }

            if (matchingController != null)
            {
                RegisterController(ExerciseType.Matching, matchingController);
            }

            Debug.Log($"[ExerciseUIController] Registered {_controllers.Count} exercise controllers");
        }

        /// <summary>
        /// Registers a controller for an exercise type.
        /// </summary>
        private void RegisterController(ExerciseType type, IExerciseController controller)
        {
            _controllers[type] = controller;

            // Subscribe to events
            controller.OnAnswerSubmitted += (answer) => HandleAnswerSubmitted(answer);
            controller.OnExerciseCompleted += () => OnExerciseCompleted?.Invoke();
        }

        /// <summary>
        /// Displays an exercise using the appropriate controller.
        /// </summary>
        /// <param name="exercise">The exercise data to display</param>
        public void DisplayExercise(BaseExerciseData exercise)
        {
            if (exercise == null)
            {
                Debug.LogError("[ExerciseUIController] Cannot display null exercise!");
                return;
            }

            // Hide current panel
            HideCurrentPanel();

            // Get the appropriate controller
            if (!_controllers.TryGetValue(exercise.exerciseType, out var controller))
            {
                Debug.LogError($"[ExerciseUIController] No controller found for exercise type: {exercise.exerciseType}");
                return;
            }

            // Store current state
            _currentController = controller;
            _currentExercise = exercise;

            // Initialize and show the controller
            controller.Initialize(exercise);
            controller.Show();

            Debug.Log($"[ExerciseUIController] Displaying {exercise.exerciseType} exercise");
        }

        /// <summary>
        /// Handles answer submission from any controller.
        /// </summary>
        private void HandleAnswerSubmitted(object answer)
        {
            if (_currentExercise == null)
            {
                Debug.LogError("[ExerciseUIController] Answer submitted but no current exercise!");
                return;
            }

            OnAnswerSubmitted?.Invoke(_currentExercise, answer);
        }

        /// <summary>
        /// Shows feedback on the current exercise.
        /// </summary>
        /// <param name="isCorrect">Whether the answer was correct</param>
        /// <param name="correctAnswer">Optional correct answer to display</param>
        public void ShowFeedback(bool isCorrect, object correctAnswer = null)
        {
            _currentController?.ShowFeedback(isCorrect, correctAnswer);
        }

        /// <summary>
        /// Resets the current exercise panel.
        /// </summary>
        public void ResetCurrentPanel()
        {
            _currentController?.Reset();
        }

        /// <summary>
        /// Enables or disables interaction on the current panel.
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            _currentController?.SetInteractable(interactable);
        }

        /// <summary>
        /// Hides the current panel.
        /// </summary>
        public void HideCurrentPanel()
        {
            _currentController?.Hide();
            _currentController = null;
            _currentExercise = null;
        }

        /// <summary>
        /// Hides all exercise panels.
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var controller in _controllers.Values)
            {
                controller?.Hide();
            }

            _currentController = null;
            _currentExercise = null;
        }

        /// <summary>
        /// Gets the current exercise being displayed.
        /// </summary>
        public BaseExerciseData GetCurrentExercise()
        {
            return _currentExercise;
        }

        /// <summary>
        /// Gets the current exercise type.
        /// </summary>
        public ExerciseType? GetCurrentExerciseType()
        {
            return _currentExercise?.exerciseType;
        }

        /// <summary>
        /// Checks if a controller is registered for a specific exercise type.
        /// </summary>
        public bool HasControllerFor(ExerciseType type)
        {
            return _controllers.ContainsKey(type);
        }

        /// <summary>
        /// Gets statistics about registered controllers.
        /// </summary>
        public Dictionary<ExerciseType, bool> GetControllerStatus()
        {
            var status = new Dictionary<ExerciseType, bool>();

            foreach (ExerciseType type in Enum.GetValues(typeof(ExerciseType)))
            {
                status[type] = _controllers.ContainsKey(type);
            }

            return status;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            foreach (var controller in _controllers.Values)
            {
                if (controller != null)
                {
                    // Note: Events will be cleaned up by BaseExerciseController.OnDestroy
                }
            }

            OnAnswerSubmitted = null;
            OnExerciseCompleted = null;
        }
    }
}
