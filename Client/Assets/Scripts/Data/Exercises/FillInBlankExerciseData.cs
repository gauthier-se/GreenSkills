using System.Collections.Generic;
using UnityEngine;

namespace Data.Exercises
{
    /// <summary>
    /// Exercise data for Fill in the Blank questions.
    /// Player must complete a sentence by selecting or typing the correct words.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFillInBlankExercise", menuName = "RSE/Exercises/FillInBlank")]
    public class FillInBlankExerciseData : BaseExerciseData
    {
        [TextArea(3, 10)]
        [Tooltip("The sentence with blanks marked as {0}, {1}, etc. Example: 'La RSE signifie {0} Sociétale des {1}'")]
        public string sentenceWithBlanks;

        [Tooltip("The correct answers for each blank, in order")]
        public List<string> correctAnswers;

        [Tooltip("Word options to display for selection (includes correct answers and distractors)")]
        public List<string> wordOptions;

        [Tooltip("If true, answers are case-sensitive")]
        public bool caseSensitive = false;

        private void OnEnable()
        {
            exerciseType = ExerciseType.FillInBlank;
        }

        /// <summary>
        /// Gets the number of blanks in this exercise.
        /// </summary>
        public int BlankCount => correctAnswers?.Count ?? 0;

        /// <summary>
        /// Validates if the provided answers match all blanks correctly.
        /// </summary>
        /// <param name="answer">List of strings representing the player's answers for each blank</param>
        /// <returns>True if all blanks are filled correctly</returns>
        public override bool ValidateAnswer(object answer)
        {
            if (answer is List<string> playerAnswers)
            {
                if (playerAnswers.Count != correctAnswers.Count)
                {
                    return false;
                }

                for (int i = 0; i < correctAnswers.Count; i++)
                {
                    string correct = correctAnswers[i];
                    string playerAnswer = playerAnswers[i];

                    bool matches = caseSensitive
                        ? correct == playerAnswer
                        : correct.Equals(playerAnswer, System.StringComparison.OrdinalIgnoreCase);

                    if (!matches)
                    {
                        return false;
                    }
                }

                return true;
            }

            // Support single answer for single-blank exercises
            if (answer is string singleAnswer && correctAnswers.Count == 1)
            {
                return caseSensitive
                    ? correctAnswers[0] == singleAnswer
                    : correctAnswers[0].Equals(singleAnswer, System.StringComparison.OrdinalIgnoreCase);
            }

            Debug.LogWarning($"FillInBlankExerciseData.ValidateAnswer received unexpected type: {answer?.GetType()}");
            return false;
        }

        /// <summary>
        /// Returns the sentence with blanks as the main text.
        /// </summary>
        public override string GetMainText()
        {
            return sentenceWithBlanks;
        }

        /// <summary>
        /// Gets the sentence with blanks replaced by underscores for display.
        /// </summary>
        /// <param name="underscoreCount">Number of underscores per blank</param>
        public string GetDisplaySentence(int underscoreCount = 5)
        {
            string blank = new string('_', underscoreCount);
            string result = sentenceWithBlanks;

            for (int i = 0; i < BlankCount; i++)
            {
                result = result.Replace($"{{{i}}}", blank);
            }

            return result;
        }
    }
}
