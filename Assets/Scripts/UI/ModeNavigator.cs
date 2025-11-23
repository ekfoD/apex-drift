using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuNavigator : MonoBehaviour
{
    [Header("Mode Selection Buttons")]
    public Button singleplayerButton;
    public Button multiplayerButton;
    public Button backButton;
    
    void Start()
    {
        if (singleplayerButton != null)
            singleplayerButton.onClick.AddListener(GoToSingleplayerMapSelection);
        
        if (multiplayerButton != null)
            multiplayerButton.onClick.AddListener(GoToMultiplayerCarSelection);
        
        if (backButton != null)
            backButton.onClick.AddListener(GoBackToMainMenu);
    }
    
    public void GoToSingleplayerMapSelection()
    {
        SceneManager.LoadScene("SingleplyerMapSelection");
    }
    
    public void GoToMultiplayerCarSelection()
    {
        SceneManager.LoadScene("MultiplayerCarSelection");
    }
    
    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMeniu-Final");
    }
}