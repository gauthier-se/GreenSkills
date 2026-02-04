using System;
using Data.Exercises;
using UnityEngine;

namespace UI.Exercises
{
    /// <summary>
    /// Base class for exercise UI controllers providing common functionality.
    /// </summary>
    public abstract class BaseExerciseController : MonoBehaviour, IExerciseController
    {
        public event Action<object> OnAnswerSubmitted;
        public event Action OnExerciseCompleted;

        [Header("Base Settings")]
        [SerializeField] protected GameObject panelRoot;
        [SerializeField] protected CanvasGroup canvasGroup;

        protected BaseExerciseData currentExercise;
        protected bool isInteractable = true;

        /// <summary>
        /// Initializes the controller with exercise data.
        /// Override this method to add type-specific initialization.
        /// </summary>
        public virtual void Initialize(BaseExerciseData exerciseData)
        {
            currentExercise = exerciseData;
            Reset();
        }

        /// <summary>
        /// Shows the exercise panel.
        /// </summary>
        public virtual void Show()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            SetInteractable(true);
        }

        /// <summary>
        /// Hides the exercise panel.
        /// </summary>
        public virtual void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// Displays feedback for the player's answer.
        /// Override this to customize feedback display.
        /// </summary>
        public abstract void ShowFeedback(bool isCorrect, object correctAnswer = null);

        /// <summary>
        /// Resets the panel to its initial state.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Enables or disables player interaction.
        /// </summary>
        public virtual void SetInteractable(bool interactable)
        {
            isInteractable = interactable;

            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
            }
        }

        /// <summary>
        /// Gets the GameObject for this controller.
        /// </summary>
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        /// <summary>
        /// Invokes the OnAnswerSubmitted event.
        /// Call this from derived classes when the player submits an answer.
        /// </summary>
        protected void RaiseAnswerSubmitted(object answer)
        {
            OnAnswerSubmitted?.Invoke(answer);
        }

        /// <summary>
        /// Invokes the OnExerciseCompleted event.
        /// </summary>
        protected void RaiseExerciseCompleted()
        {
            OnExerciseCompleted?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            // Clean up event subscriptions
            OnAnswerSubmitted = null;
            OnExerciseCompleted = null;
        }
    }
}
