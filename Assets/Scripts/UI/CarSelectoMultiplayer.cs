using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CarSelectorMultiplayer : MonoBehaviour
{
    [Header("Car Prefabs/Models")]
    public GameObject[] carPrefabs; 
    
    [Header("UI Elements")]
    public Button leftArrow;
    public Button rightArrow;
    public Button selectButton;
    public Button backButton;
    public Transform carSpawnPoint;
    
    [Header("Modification Buttons")]
    public Button[] modificationButtons;
    
    [Header("Modification Descriptions")]
    public TextMeshProUGUI descriptionText;
    [TextArea(2, 5)]
    public string[] modificationDescriptions;
    public string defaultDescription = "Select a modification to see its details.";
    
    [Header("Map Preview")]
    public Image mapPreviewImage;
    public Sprite[] mapSprites; 
    [Header("Available Maps")]
    //public string[] mapScenes = { "world_01", "world_02", "world_03", "world_04", "world_05" };
    public string[] mapScenes = { "gaminam" };
    [Header("Selection Highlight")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.green;
    
    private int currentCarIndex = 0;
    private int currentModificationIndex = -1;
    private GameObject currentCarInstance;
    private string randomMap;
    private int randomMapIndex;
    
    void Start()
    {
        randomMapIndex = Random.Range(0, mapScenes.Length);
        randomMap = mapScenes[randomMapIndex];
        //PlayerPrefs.SetString("SelectedMap", randomMap);
        PlayerPrefs.SetString("SelectedMap", "gaminam");
        Debug.Log("Random map: " + randomMap);
        
        leftArrow.onClick.AddListener(PreviousCar);
        rightArrow.onClick.AddListener(NextCar);
        selectButton.onClick.AddListener(ConfirmSelection);
        
        if (backButton != null)
            backButton.onClick.AddListener(GoBack);
        
        for (int i = 0; i < modificationButtons.Length; i++)
        {
            int index = i;
            modificationButtons[i].onClick.AddListener(() => SelectModification(index));
        }
        
        selectButton.interactable = false;
        
        if (descriptionText != null)
            descriptionText.text = defaultDescription;
        
        ShowCar(currentCarIndex);
        UpdateModificationButtonVisuals();
        LoadMapPreview();
    }
    
    void LoadMapPreview()
    {
        if (mapPreviewImage == null || mapSprites.Length == 0)
            return;
        
        if (randomMapIndex < mapSprites.Length && mapSprites[randomMapIndex] != null)
        {
            mapPreviewImage.sprite = mapSprites[randomMapIndex];
        }
    }
    
    public void NextCar()
    {
        currentCarIndex++;
        if (currentCarIndex >= carPrefabs.Length)
            currentCarIndex = 0;
        ShowCar(currentCarIndex);
        ResetModificationSelection();
    }
    
    public void PreviousCar()
    {
        currentCarIndex--;
        if (currentCarIndex < 0)
            currentCarIndex = carPrefabs.Length - 1;
        ShowCar(currentCarIndex);
        ResetModificationSelection();
    }
    
    void ShowCar(int index)
    {
        if (currentCarInstance != null)
            Destroy(currentCarInstance);
        
        currentCarInstance = Instantiate(carPrefabs[index], carSpawnPoint.position, carSpawnPoint.rotation);
        currentCarInstance.transform.localScale = Vector3.one * 5f;
        currentCarInstance.transform.rotation = Quaternion.Euler(0, -135, 0);
    }
    
    void SelectModification(int index)
    {
        currentModificationIndex = index;
        selectButton.interactable = true;
        UpdateDescriptionText();
        UpdateModificationButtonVisuals();
        Debug.Log("Selected Modification: " + (index + 1));
    }
    
    void UpdateDescriptionText()
    {
        if (descriptionText == null)
            return;
            
        if (currentModificationIndex >= 0 && currentModificationIndex < modificationDescriptions.Length)
        {
            descriptionText.text = modificationDescriptions[currentModificationIndex];
        }
        else
        {
            descriptionText.text = defaultDescription;
        }
    }
    
    void UpdateModificationButtonVisuals()
    {
        for (int i = 0; i < modificationButtons.Length; i++)
        {
            Image buttonImage = modificationButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = (i == currentModificationIndex) ? selectedColor : normalColor;
            }
        }
    }
    
    void ResetModificationSelection()
    {
        currentModificationIndex = -1;
        selectButton.interactable = false;
        UpdateModificationButtonVisuals();
        
        if (descriptionText != null)
            descriptionText.text = defaultDescription;
    }
    
    void ConfirmSelection()
    {
        if (currentModificationIndex < 0)
        {
            Debug.LogWarning("Please select a modification first!");
            return;
        }
    
    PlayerPrefs.SetString("GameMode", "Multiplayer");
    PlayerPrefs.SetInt("SelectedCarIndex", currentCarIndex);
    PlayerPrefs.SetString("SelectedCarName", carPrefabs[currentCarIndex].name);
    PlayerPrefs.SetInt("SelectedModificationIndex", currentModificationIndex);
    PlayerPrefs.Save();
    
    Debug.Log("MULTIPLAYER mode - Going to: " + randomMap + 
                " with car: " + carPrefabs[currentCarIndex].name + 
                " and modification: " + (currentModificationIndex + 1));

    Debug.Log("Starting multiplayer matchmaking...");
    
    // Disable button to prevent multiple clicks
    selectButton.interactable = false;
    selectButton.GetComponentInChildren<TextMeshProUGUI>().text = "Searching...";
    
    // Start matchmaking
    MPMatchmaker.Instance.StartMatchmaking(randomMap, currentCarIndex, currentModificationIndex);
        
        //SceneManager.LoadScene(randomMap);
    }
    
    void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}