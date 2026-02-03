using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Handles player input processing for the game.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        /// <summary>
        /// Processes player inputs each frame.
        /// Called from Update loop when needed.
        /// </summary>
        public void ProcessInputs()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Space key pressed");
            }
        }
    }
}
