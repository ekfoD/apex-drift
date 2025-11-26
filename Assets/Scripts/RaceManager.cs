using UnityEngine;
using Unity.Netcode;
using TMPro;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance { get; private set; }
    
    [Header("Setup")]
    public SpawnPoint spawnPoint;
    
    [Header("Countdown UI")]
    public TMP_Text countdownText;
    
    [Header("Race State")]
    public bool raceStarted = false;
    
    private bool isMultiplayer = false;
    private GameObject spawnedCar; // Store reference to car
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        string gameMode = PlayerPrefs.GetString("GameMode", "Singleplayer");
        isMultiplayer = gameMode == "Multiplayer";
        
        if (isMultiplayer)
        {
            StartMultiplayerRace();
        }
        else
        {
            StartSingleplayerRace();
        }
    }
    
    void StartSingleplayerRace()
    {
        Debug.Log("Starting singleplayer race...");
        
        int carIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        spawnedCar = spawnPoint.SpawnCar(carIndex, isNetworkSpawn: false);
        
        // Disable car controls during countdown
        DisableCarControls();
        
        StartCoroutine(CountdownSequence());
    }
    
    void StartMultiplayerRace()
    {
        if (NetworkManager.Singleton == null) return;
        
        int carIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        spawnedCar = spawnPoint.SpawnCar(carIndex, isNetworkSpawn: false);
        
        // Disable car controls during countdown
        DisableCarControls();
        
        Debug.Log("Multiplayer: Spawned local car, waiting for countdown...");
        
        if (IsServer)
        {
            Invoke(nameof(StartNetworkCountdown), 1f);
        }
    }
    
    void DisableCarControls()
    {
        if (spawnedCar == null) return;
        
        // Disable car controller
        CarController carController = spawnedCar.GetComponent<CarController>();
        if (carController != null)
        {
            carController.enabled = false;
            if (carController.tireScreechSound != null) carController.tireScreechSound.Stop();
        }
        
        // Make sure rigidbody is frozen
        Rigidbody rb = spawnedCar.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
    
    void EnableCarControls()
    {
        if (spawnedCar == null) return;
        
        // Enable car controller
        CarController carController = spawnedCar.GetComponent<CarController>();
        if (carController != null)
        {
            carController.enabled = true;
            if (carController.tireScreechSound != null) carController.tireScreechSound.Play();
        }
        
        // Unfreeze rigidbody
        Rigidbody rb = spawnedCar.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
            // Keep Y rotation free but freeze other rotations for car stability
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }
    
    void StartNetworkCountdown()
    {
        StartCountdownClientRpc();
    }
    
    [Rpc(SendTo.Everyone)]
    void StartCountdownClientRpc()
    {
        StartCoroutine(CountdownSequence());
    }
    
    System.Collections.IEnumerator CountdownSequence()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            
            countdownText.text = "3";
            yield return new WaitForSeconds(1f);
            
            countdownText.text = "2";
            yield return new WaitForSeconds(1f);
            
            countdownText.text = "1";
            yield return new WaitForSeconds(1f);
            
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
            
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            // No UI, just wait
            yield return new WaitForSeconds(3.5f);
        }
        
        StartRace();
    }
    
    void StartRace()
    {
        raceStarted = true;
        
        // Enable car controls
        EnableCarControls();
        
        // Start timer
        Timer timer = FindFirstObjectByType<Timer>();
        if (timer != null)
        {
            timer.StartTimer();
        }
        
    }
    
    public bool IsMultiplayer()
    {
        return isMultiplayer;
    }
}
