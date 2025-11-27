using UnityEngine;

public class PanelController : MonoBehaviour
{
    public GameObject panelToHide;

    public void HidePanel()
    {
        if (panelToHide != null)
        {
            panelToHide.SetActive(false);
            Time.timeScale = 1f; // Unfreeze game
        }
    }
}
