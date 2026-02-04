using UnityEngine;

public class DontDestroyOnLoadManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
