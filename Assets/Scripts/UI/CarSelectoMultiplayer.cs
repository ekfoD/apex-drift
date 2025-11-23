using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarSelectorMultiplayer : MonoBehaviour
{
    [Header("Car Prefabs/Models")]
    public GameObject[] carPrefabs; 
    
    [Header("UI Elements")]
    public Button leftArrow;
    public Button rightArrow;
    public Button selectButton;
    public Transform carSpawnPoint;
    
    [Header("Available Maps")]
    public string[] mapScenes = { "world_01", "world_02", "world_03", "world_04", "world_05" };
    
    private int currentCarIndex = 0;
    private GameObject currentCarInstance;
    private string randomMap;
    
    void Start()
    {
        randomMap = mapScenes[Random.Range(0, mapScenes.Length)];
        PlayerPrefs.SetString("SelectedMap", randomMap);
        Debug.Log("Random map: " + randomMap);
        
        leftArrow.onClick.AddListener(PreviousCar);
        rightArrow.onClick.AddListener(NextCar);
        selectButton.onClick.AddListener(ConfirmSelection);
        
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
        PlayerPrefs.SetString("GameMode", "Multiplayer");
        
        PlayerPrefs.SetInt("SelectedCarIndex", currentCarIndex);
        PlayerPrefs.SetString("SelectedCarName", carPrefabs[currentCarIndex].name);
        PlayerPrefs.Save();
        
        Debug.Log("MULTIPLAYER mode - Going to: " + randomMap + " with car: " + carPrefabs[currentCarIndex].name);
        
        SceneManager.LoadScene(randomMap);
    }
}