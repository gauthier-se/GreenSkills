using System;
using Data.Exercises;
using UnityEngine;

namespace UI.Exercises
{
    /// <summary>
    /// Interface for all exercise UI controllers.
    /// Each mini-game type implements this interface to handle its specific UI logic.
    /// </summary>
    public interface IExerciseController
    {
        /// <summary>
        /// Event triggered when the player submits an answer.
        /// The object parameter contains the answer in a format specific to the exercise type.
        /// </summary>
        event Action<object> OnAnswerSubmitted;

        /// <summary>
        /// Event triggered when the exercise needs to show the next step (if multi-step).
        /// </summary>
        event Action OnExerciseCompleted;

        /// <summary>
        /// Initializes the controller with exercise data.
        /// </summary>
        /// <param name="exerciseData">The exercise data to display</param>
        void Initialize(BaseExerciseData exerciseData);

        /// <summary>
        /// Shows the exercise panel and prepares it for interaction.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the exercise panel.
        /// </summary>
        void Hide();

        /// <summary>
        /// Displays feedback for the player's answer.
        /// </summary>
        /// <param name="isCorrect">Whether the answer was correct</param>
        /// <param name="correctAnswer">Optional: the correct answer to display</param>
        void ShowFeedback(bool isCorrect, object correctAnswer = null);

        /// <summary>
        /// Resets the panel to its initial state for a new exercise.
        /// </summary>
        void Reset();

        /// <summary>
        /// Enables or disables player interaction with the panel.
        /// </summary>
        /// <param name="interactable">Whether the panel should be interactable</param>
        void SetInteractable(bool interactable);

        /// <summary>
        /// Gets the GameObject associated with this controller.
        /// </summary>
        GameObject GetGameObject();
    }
}
