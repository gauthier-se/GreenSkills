using Data;
using Data.Exercises;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Exercises
{
    /// <summary>
    /// Manages the shared two-zone exercise layout: dark question zone + light answer zone.
    /// Extracts question text from exercise data and applies theme colors.
    /// </summary>
    public class ExerciseLayoutController : MonoBehaviour
    {
        [Header("Question Zone")]
        [SerializeField] private Image questionZoneBackground;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Image questionImage;
        [SerializeField] private Image categoryAccentBar;

        [Header("Answer Zone")]
        [SerializeField] private Image answerZoneBackground;

        [Header("Theme")]
        [SerializeField] private UITheme theme;

        /// <summary>
        /// Configures the layout for the given exercise.
        /// Sets question text, image visibility, accent bar color, and zone backgrounds.
        /// </summary>
        public void SetupForExercise(BaseExerciseData exercise)
        {
            if (exercise == null) return;

            // Set question text
            if (questionText != null)
            {
                questionText.text = exercise.GetMainText();
            }

            // Show/hide question image
            if (questionImage != null)
            {
                if (exercise.image != null)
                {
                    questionImage.sprite = exercise.image;
                    questionImage.gameObject.SetActive(true);
                }
                else
                {
                    questionImage.gameObject.SetActive(false);
                }
            }

            // Apply theme colors
            if (theme != null)
            {
                if (categoryAccentBar != null)
                {
                    categoryAccentBar.color = theme.GetCategoryColor(exercise.category);
                }

                if (questionZoneBackground != null)
                {
                    questionZoneBackground.color = theme.bgQuestionZone;
                }

                if (answerZoneBackground != null)
                {
                    answerZoneBackground.color = theme.bgApp;
                }
            }
        }

        /// <summary>
        /// Resets the layout to a cleared state.
        /// </summary>
        public void Clear()
        {
            if (questionText != null)
            {
                questionText.text = "";
            }

            if (questionImage != null)
            {
                questionImage.gameObject.SetActive(false);
            }
        }
    }
}
