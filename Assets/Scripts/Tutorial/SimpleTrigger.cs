using UnityEngine;

public class ShowPanelTrigger : MonoBehaviour
{
    public GameObject panelToShow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            panelToShow.SetActive(true);
            Time.timeScale = 0f; // Freeze game
            gameObject.SetActive(false); // Disable trigger so it only works once
        }
    }
}
