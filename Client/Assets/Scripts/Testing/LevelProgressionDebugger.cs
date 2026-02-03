using UnityEngine;
using Managers;

namespace Testing
{
    /// <summary>
    /// Debug utility for testing level progression without completing levels.
    /// Attach this to a GameObject in the MainMenu scene for quick testing.
    /// </summary>
    public class LevelProgressionDebugger : MonoBehaviour
    {
        [Header("Debug Controls")]
        [Tooltip("Press this key to unlock the next level")]
        [SerializeField] private KeyCode unlockNextLevelKey = KeyCode.U;

        [Tooltip("Press this key to reset all progress")]
        [SerializeField] private KeyCode resetProgressKey = KeyCode.R;

        [Tooltip("Press this key to unlock all levels")]
        [SerializeField] private KeyCode unlockAllLevelsKey = KeyCode.A;

        [SerializeField] private int totalLevelsInGame = 10;

        private void Update()
        {
            if (Input.GetKeyDown(unlockNextLevelKey))
            {
                UnlockNextLevel();
            }

            if (Input.GetKeyDown(resetProgressKey))
            {
                ResetAllProgress();
            }

            if (Input.GetKeyDown(unlockAllLevelsKey))
            {
                UnlockAllLevels();
            }
        }

        /// <summary>
        /// Unlocks the next level in the progression.
        /// </summary>
        private void UnlockNextLevel()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found!");
                return;
            }

            int currentHighest = GameManager.Instance.GetHighestLevelUnlocked();
            int nextLevel = currentHighest + 1;

            if (nextLevel > totalLevelsInGame)
            {
                Debug.LogWarning($"All levels are already unlocked! (Max: {totalLevelsInGame})");
                return;
            }

            PlayerPrefs.SetInt("HighestLevelUnlocked", nextLevel);
            PlayerPrefs.Save();

            Debug.Log($"DEBUG: Level {nextLevel} manually unlocked!");

            RefreshMainMenu();
        }

        /// <summary>
        /// Resets all progression to level 1.
        /// </summary>
        private void ResetAllProgress()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found!");
                return;
            }

            GameManager.Instance.ResetProgress();
            Debug.Log("DEBUG: Progress reset!");

            RefreshMainMenu();
        }

        /// <summary>
        /// Unlocks all levels instantly (for testing end-game features).
        /// </summary>
        private void UnlockAllLevels()
        {
            PlayerPrefs.SetInt("HighestLevelUnlocked", totalLevelsInGame);
            PlayerPrefs.Save();

            Debug.Log($"DEBUG: All {totalLevelsInGame} levels unlocked!");

            RefreshMainMenu();
        }

        /// <summary>
        /// Refreshes the main menu to show updated button states.
        /// </summary>
        private void RefreshMainMenu()
        {
            MainMenuManager menuManager = FindFirstObjectByType<MainMenuManager>();

            if (menuManager != null)
            {
                menuManager.RefreshLevelButtons();
                Debug.Log("Main menu refreshed!");
            }
            else
            {
                Debug.LogWarning("MainMenuManager not found in scene. Buttons will not be updated automatically.");
            }
        }

        private void OnGUI()
        {
            // Display debug info in the top-right corner
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.UpperRight;

            string debugInfo = $"DEBUG KEYS:\n" +
                               $"[{unlockNextLevelKey}] Unlock Next Level\n" +
                               $"[{resetProgressKey}] Reset Progress\n" +
                               $"[{unlockAllLevelsKey}] Unlock All Levels\n\n";

            if (GameManager.Instance != null)
            {
                int highest = GameManager.Instance.GetHighestLevelUnlocked();
                debugInfo += $"Current Progress: Level {highest}/{totalLevelsInGame}";
            }

            GUI.Label(new Rect(Screen.width - 300, 10, 290, 150), debugInfo, style);
        }
    }
}
