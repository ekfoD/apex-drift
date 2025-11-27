using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitButton : MonoBehaviour
{
    public void OnQuit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("ModeSelection"); // Change scene name
    }
}
