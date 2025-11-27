using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    public Button backButton;

    void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(GoToMainMenu);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMeniu-Final");
    }
}
