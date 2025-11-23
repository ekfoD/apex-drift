using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarSelectorSingleplayer : MonoBehaviour
{
    [Header("Car Prefabs/Models")]
    public GameObject[] carPrefabs; 
    
    [Header("UI Elements")]
    public Button leftArrow;
    public Button rightArrow;
    public Button selectButton;
    public Button backButton;
    public Transform carSpawnPoint; 
    
    private int currentCarIndex = 0;
    private GameObject currentCarInstance;
    
    void Start()
    {
        leftArrow.onClick.AddListener(PreviousCar);
        rightArrow.onClick.AddListener(NextCar);
        selectButton.onClick.AddListener(ConfirmSelection);
        backButton.onClick.AddListener(GoBack);
        
        ShowCar(currentCarIndex);
    }
    
    public void NextCar()
    {
        currentCarIndex++;
        if (currentCarIndex >= carPrefabs.Length)
            currentCarIndex = 0;
        ShowCar(currentCarIndex);
    }
    
    public void PreviousCar()
    {
        currentCarIndex--;
        if (currentCarIndex < 0)
            currentCarIndex = carPrefabs.Length - 1;
        ShowCar(currentCarIndex);
    }
    
    void ShowCar(int index)
    {
        if (currentCarInstance != null)
            Destroy(currentCarInstance);
        
        currentCarInstance = Instantiate(carPrefabs[index], carSpawnPoint.position, carSpawnPoint.rotation);
        currentCarInstance.transform.localScale = Vector3.one * 5f;
        currentCarInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    
    void ConfirmSelection()
    {
        PlayerPrefs.SetString("GameMode", "Singleplayer");
        
        PlayerPrefs.SetInt("SelectedCarIndex", currentCarIndex);
        PlayerPrefs.SetString("SelectedCarName", carPrefabs[currentCarIndex].name);
        PlayerPrefs.Save();
        
        string selectedMap = PlayerPrefs.GetString("SelectedMap", "world_01");
        
        Debug.Log("SINGLEPLAYER mode - Going to: " + selectedMap + " with car: " + carPrefabs[currentCarIndex].name);
        
        SceneManager.LoadScene(selectedMap);
    }
    
    void GoBack()
    {
        SceneManager.LoadScene("SingleplyerMapSelection");
    }
}