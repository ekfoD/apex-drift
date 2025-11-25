using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerSessionUI : MonoBehaviour
{
    public Button hostGameButton;

    void Start()
    {
        if (hostGameButton != null)
            hostGameButton.onClick.AddListener(HostGame);
    }

    void HostGame()
    {
        SceneManager.LoadScene("MultiplayerCarSelection");
    }
}