using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void LoadModeSelection()
    {
        SceneManager.LoadScene("ModeSelection");
    }
    
    public void LoadSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene("CreditScene");
    }
    
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit"); 
    }
}