using Managers;
using UnityEngine;

namespace UI
{
    public class MainMenuUi : MonoBehaviour
    {
        // Method called when the Play button is clicked
        public void OnPlayClick()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
            else
            {
                Debug.LogError("Error: GameManager instance is null.");
            }
        }
    }
}