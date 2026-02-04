using System.Collections.Generic;
using UnityEngine;

namespace Data.Exercises
{
    /// <summary>
    /// Exercise data for Multiple Choice Quiz questions.
    /// Player must select one correct answer from multiple options.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuizExercise", menuName = "RSE/Exercises/Quiz")]
    public class QuizExerciseData : BaseExerciseData
    {
        [TextArea(3, 10)]
        [Tooltip("The question text displayed to the player")]
        public string questionText;

        [Tooltip("List of answer options")]
        public List<string> options;

        [Tooltip("Index of the correct answer (0-based)")]
        public int correctOptionIndex;

        private void OnEnable()
        {
            exerciseType = ExerciseType.Quiz;
        }

        /// <summary>
        /// Validates if the selected option index is the correct answer.
        /// </summary>
        /// <param name="answer">The index of the selected option (int)</param>
        /// <returns>True if the selected index matches the correct answer</returns>
        public override bool ValidateAnswer(object answer)
        {
            if (answer is int selectedIndex)
            {
                return selectedIndex == correctOptionIndex;
            }

            Debug.LogWarning($"QuizExerciseData.ValidateAnswer received unexpected type: {answer?.GetType()}");
            return false;
        }

        /// <summary>
        /// Returns the question text as the main text.
        /// </summary>
        public override string GetMainText()
        {
            return questionText;
        }
    }
}
