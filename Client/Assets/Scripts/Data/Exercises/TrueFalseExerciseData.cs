using UnityEngine;

namespace Data.Exercises
{
    /// <summary>
    /// Exercise data for True/False questions.
    /// Player must determine if a statement is true or false.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTrueFalseExercise", menuName = "RSE/Exercises/TrueFalse")]
    public class TrueFalseExerciseData : BaseExerciseData
    {
        [TextArea(3, 10)]
        [Tooltip("The statement to evaluate as true or false")]
        public string statement;

        [Tooltip("Whether the statement is true or false")]
        public bool isTrue;

        private void OnEnable()
        {
            exerciseType = ExerciseType.TrueFalse;
        }

        /// <summary>
        /// Validates if the player's answer matches the correct truth value.
        /// </summary>
        /// <param name="answer">The player's answer (bool)</param>
        /// <returns>True if the answer matches</returns>
        public override bool ValidateAnswer(object answer)
        {
            if (answer is bool playerAnswer)
            {
                return playerAnswer == isTrue;
            }

            // Also accept int (0 = false, 1 = true) for flexibility
            if (answer is int intAnswer)
            {
                bool boolAnswer = intAnswer == 1;
                return boolAnswer == isTrue;
            }

            Debug.LogWarning($"TrueFalseExerciseData.ValidateAnswer received unexpected type: {answer?.GetType()}");
            return false;
        }

        /// <summary>
        /// Returns the statement as the main text.
        /// </summary>
        public override string GetMainText()
        {
            return statement;
        }
    }
}
