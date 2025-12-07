using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuNavigator : MonoBehaviour
{
    [Header("Mode Selection Buttons")]
    public Button singleplayerButton;
    public Button multiplayerButton;
    public Button backButton;
    public Button leaderboardButton;
    
    void Start()
    {
        if (singleplayerButton != null)
            singleplayerButton.onClick.AddListener(GoToSingleplayerMapSelection);
        
        if (multiplayerButton != null)
            multiplayerButton.onClick.AddListener(GoToMultiplayerSession);
        
        if (backButton != null)
            backButton.onClick.AddListener(GoBackToMainMenu);
        
        if (leaderboardButton != null)
            leaderboardButton.onClick.AddListener(GoToLeaderboard);
    }
    
    public void GoToSingleplayerMapSelection()
    {
        SceneManager.LoadScene("SingleplyerMapSelection");
    }
    
    public void GoToMultiplayerSession() 
    {
        SceneManager.LoadScene("MultiplayerCarSelection");
    }
    
    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMeniu-Final");
    }
    
    public void GoToLeaderboard()
    {
        SceneManager.LoadScene("Leaderboard"); 
    }

    public void GoToTutorial()
    {
        SceneManager.LoadScene("world_tutorial");
    }
}