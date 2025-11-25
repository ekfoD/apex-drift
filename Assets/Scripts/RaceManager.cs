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
        spawnPoint.SpawnCar(carIndex, isNetworkSpawn: false);
        
        StartCoroutine(CountdownSequence());
    }
    
    void StartMultiplayerRace()
    {
        if (NetworkManager.Singleton == null) return;
        
        // EACH CLIENT spawns their OWN car locally (no network spawn)
        int carIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        spawnPoint.SpawnCar(carIndex, isNetworkSpawn: false);
        
        Debug.Log("Multiplayer: Spawned local car, waiting for countdown...");
        
        // Only host triggers countdown
        if (IsServer)
        {
            Invoke(nameof(StartNetworkCountdown), 1f);
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
            Debug.Log("3...");
            yield return new WaitForSeconds(1f);
            Debug.Log("2...");
            yield return new WaitForSeconds(1f);
            Debug.Log("1...");
            yield return new WaitForSeconds(1f);
            Debug.Log("GO!");
            yield return new WaitForSeconds(0.5f);
        }
        
        StartRace();
    }
    
    void StartRace()
    {
        raceStarted = true;
        
        Timer timer = FindFirstObjectByType<Timer>();
        if (timer != null)
        {
            timer.StartTimer();
        }
        
        Debug.Log("RACE STARTED!");
    }
    
    public bool IsMultiplayer()
    {
        return isMultiplayer;
    }
}
