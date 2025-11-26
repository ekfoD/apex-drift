using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public Button backButton;

    void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(GoToModeSelection);
    }

    void GoToModeSelection()
    {
        SceneManager.LoadScene("ModeSelection");
    }
}