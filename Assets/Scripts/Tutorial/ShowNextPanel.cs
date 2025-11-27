using UnityEngine;

public class ShowNextPanel : MonoBehaviour
{
    public GameObject currentPanel;
    public GameObject nextPanel;

    public void ShowNext()
    {
        currentPanel.SetActive(false);
        nextPanel.SetActive(true);
        Time.timeScale = 0f; // Keep frozen
    }
}
