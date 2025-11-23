using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelector : MonoBehaviour
{
    [Header("Map Selection Buttons")]
    public Button selectMap1Button;
    public Button selectMap2Button;
    public Button selectMap3Button;
    public Button selectMap4Button;
    public Button selectMap5Button;
    public Button backButton;
    
    [Header("Map Scene Names")]
    public string map1SceneName = "world_01";
    public string map2SceneName = "world_02";
    public string map3SceneName = "world_03";
    public string map4SceneName = "world_04";
    public string map5SceneName = "world_05";
    
    void Start()
    {
        if (selectMap1Button != null)
            selectMap1Button.onClick.AddListener(() => SelectMap(map1SceneName));
        
        if (selectMap2Button != null)
            selectMap2Button.onClick.AddListener(() => SelectMap(map2SceneName));
        
        if (selectMap3Button != null)
            selectMap3Button.onClick.AddListener(() => SelectMap(map3SceneName));
        
        if (selectMap4Button != null)
            selectMap4Button.onClick.AddListener(() => SelectMap(map4SceneName));
        
        if (selectMap5Button != null)
            selectMap5Button.onClick.AddListener(() => SelectMap(map5SceneName));
        
        if (backButton != null)
            backButton.onClick.AddListener(GoBackToModeSelection);
    }
    
    void SelectMap(string mapSceneName)
    {
        PlayerPrefs.SetString("SelectedMap", mapSceneName);
        PlayerPrefs.Save();
        
        Debug.Log("Selected map: " + mapSceneName);
        
        SceneManager.LoadScene("SingleplayerCarSelection");
    }
    
    void GoBackToModeSelection()
    {
        SceneManager.LoadScene("ModeSelection");
    }
    
    public static string GetSelectedMap()
    {
        return PlayerPrefs.GetString("SelectedMap", "world_01"); 
    }
}