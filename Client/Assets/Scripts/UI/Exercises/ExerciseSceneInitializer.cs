using Managers;
using UnityEngine;

namespace UI.Exercises
{
    /// <summary>
    /// Initializes the exercise UI system when the Game scene loads.
    /// Attach this to a GameObject in the Game scene alongside ExerciseUIController.
    /// </summary>
    [RequireComponent(typeof(ExerciseUIController))]
    public class ExerciseSceneInitializer : MonoBehaviour
    {
        [Header("End Game Panels")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        private ExerciseUIController _uiController;

        private void Start()
        {
            _uiController = GetComponent<ExerciseUIController>();

            if (GameManager.Instance != null)
            {
                Debug.Log("[ExerciseSceneInitializer] Registering with GameManager");
                GameManager.Instance.RegisterExerciseUIController(_uiController);
            }
            else
            {
                Debug.LogError("[ExerciseSceneInitializer] GameManager.Instance not found!");
            }
        }

        /// <summary>
        /// Shows the game over panel.
        /// </summary>
        public void ShowGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Shows the victory panel.
        /// </summary>
        public void ShowVictoryPanel()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hides all end-game panels.
        /// </summary>
        public void ResetToExerciseView()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }
    }
}
