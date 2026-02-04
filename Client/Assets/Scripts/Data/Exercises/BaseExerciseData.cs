using UnityEngine;

namespace Data.Exercises
{
    /// <summary>
    /// Abstract base class for all exercise types in the game.
    /// Contains common properties shared across all mini-games.
    /// </summary>
    public abstract class BaseExerciseData : ScriptableObject
    {
        [Tooltip("Unique identifier for the exercise")]
        public int id;

        [Tooltip("The type of exercise (Quiz, TrueFalse, etc.)")]
        public ExerciseType exerciseType;

        [TextArea(3, 10)]
        [Tooltip("Explanation shown after answering (correct or incorrect)")]
        public string explanation;

        [Tooltip("Difficulty level of the exercise")]
        public Difficulty difficulty;

        [Tooltip("RSE category this exercise belongs to")]
        public Category category;

        [Tooltip("Optional image to display with the exercise")]
        public Sprite image;

        /// <summary>
        /// Gets a display-friendly name for this exercise type.
        /// </summary>
        public virtual string GetExerciseTypeName()
        {
            return exerciseType.ToString();
        }

        /// <summary>
        /// Validates if the provided answer is correct.
        /// Must be implemented by each exercise type.
        /// </summary>
        /// <param name="answer">The player's answer (type varies by exercise)</param>
        /// <returns>True if the answer is correct</returns>
        public abstract bool ValidateAnswer(object answer);

        /// <summary>
        /// Gets the main instruction or question text for this exercise.
        /// </summary>
        public abstract string GetMainText();
    }
}
