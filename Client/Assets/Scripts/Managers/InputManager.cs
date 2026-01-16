using UnityEngine;

public class InputManager : MonoBehaviour
{
    public void ProcessInputs()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed");
        }
    }
}
