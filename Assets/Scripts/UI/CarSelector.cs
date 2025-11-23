using UnityEngine;
using UnityEngine.UI;

public class CarSelector : MonoBehaviour
{
    [Header("Car Prefabs/Models")]
    public GameObject[] carPrefabs; 
    
    [Header("UI Elements")]
    public Button leftArrow;
    public Button rightArrow;
    public Transform carSpawnPoint; 
    
    private int currentCarIndex = 0;
    private GameObject currentCarInstance;
    
    void Start()
    {
        // Check if everything is assigned
        if (leftArrow == null || rightArrow == null || carSpawnPoint == null)
        {
            Debug.LogError("CarSelector: Missing references! Check Inspector.");
            return;
        }
        
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogError("CarSelector: No car prefabs assigned!");
            return;
        }
        
        leftArrow.onClick.AddListener(PreviousCar);
        rightArrow.onClick.AddListener(NextCar);
        
        ShowCar(currentCarIndex);
    }
    
    public void NextCar()
    {
        currentCarIndex++;
        if (currentCarIndex >= carPrefabs.Length)
        {
            currentCarIndex = 0; 
        }
        ShowCar(currentCarIndex);
    }
    
    public void PreviousCar()
    {
        currentCarIndex--;
        if (currentCarIndex < 0)
        {
            currentCarIndex = carPrefabs.Length - 1; 
        }
        ShowCar(currentCarIndex);
    }
    
    void ShowCar(int index)
    {
        if (currentCarInstance != null)
        {
            Destroy(currentCarInstance);
        }
        
        if (carPrefabs[index] == null)
        {
            Debug.LogError("Car prefab at index " + index + " is null!");
            return;
        }
        
        currentCarInstance = Instantiate(carPrefabs[index], carSpawnPoint.position, carSpawnPoint.rotation);
        
        currentCarInstance.transform.Rotate(0, -90, 0);
        
        Debug.Log("Spawned: " + carPrefabs[index].name);
    }
}