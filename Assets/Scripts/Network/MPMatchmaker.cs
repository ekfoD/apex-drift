using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MPMatchmaker : MonoBehaviour
{
    public static MPMatchmaker Instance { get; private set; }
    
    [Header("Settings")]
    public int maxPlayers = 4;
    public int minPlayersToStart = 2;
    
    [Header("Status")]
    public bool isSearching = false;
    public bool isInLobby = false;
    
    private Lobby currentLobby;
    private float heartbeatTimer = 0f;
    private float lobbyPollTimer = 0f;
    private string selectedMap;
    private bool raceStarted = false;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Update()
    {
        // Stop all polling once race starts
        if (raceStarted) return;
        
        // Heartbeat for host
        if (currentLobby != null && IsHost())
        {
            heartbeatTimer += Time.deltaTime;
            if (heartbeatTimer >= 15f)
            {
                heartbeatTimer = 0f;
                SendHeartbeat();
            }
        }
        
        // Poll lobby for player count
        if (currentLobby != null)
        {
            lobbyPollTimer += Time.deltaTime;
            if (lobbyPollTimer >= 1.5f)
            {
                lobbyPollTimer = 0f;
                CheckLobbyStatus();
            }
        }
    }
    
    // Main entry point - call this when player clicks "Select"
    public async void StartMatchmaking(string mapName, int carIndex, int modIndex)
    {
        if (isSearching) return;
        
        isSearching = true;
        selectedMap = mapName;
        
        // Store player's selections
        PlayerPrefs.SetString("SelectedMap", mapName);
        PlayerPrefs.SetInt("SelectedCarIndex", carIndex);
        PlayerPrefs.SetInt("SelectedModificationIndex", modIndex);
        PlayerPrefs.Save();
        
        Debug.Log("Starting matchmaking...");
        
        // Try to join existing lobby first
        bool joined = await TryQuickJoin();
        
        if (!joined)
        {
            // No lobby found, create one
            await CreateLobby();
        }
    }
    
    async Task<bool> TryQuickJoin()
    {
        try
        {
            // Get player's car selection BEFORE joining
            int carIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = GetPlayerData(carIndex) // Pass car index
            };
        
            currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
        
            Debug.Log($"Joined existing lobby: {currentLobby.Name}");
            isInLobby = true;
        
            // Get host's selected map
            if (currentLobby.Data.ContainsKey("SelectedMap"))
            {
                selectedMap = currentLobby.Data["SelectedMap"].Value;
                PlayerPrefs.SetString("SelectedMap", selectedMap);
            }
        
            // Join relay
            if (currentLobby.Data.ContainsKey("RelayJoinCode"))
            {
                string relayCode = currentLobby.Data["RelayJoinCode"].Value;
                await RelayManager.Instance.JoinRelay(relayCode);
            }
        
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"No lobby found to join: {e.Reason}");
            return false;
        }
    }

    
    async Task CreateLobby()
    {
        try
        {
            // Create relay first
            string relayCode = await RelayManager.Instance.CreateRelay(maxPlayers);
        
            if (string.IsNullOrEmpty(relayCode))
            {
                Debug.LogError("Failed to create relay!");
                isSearching = false;
                return;
            }
        
            // Get player's car selection
            int carIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        
            // Create lobby with map and relay data
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayerData(carIndex), // Pass car index
                Data = new Dictionary<string, DataObject>
                {
                    { "SelectedMap", new DataObject(DataObject.VisibilityOptions.Public, selectedMap) },
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) }
                }
            };
        
            currentLobby = await LobbyService.Instance.CreateLobbyAsync($"Race_{Random.Range(1000, 9999)}", maxPlayers, options);
        
            Debug.Log($"Created lobby: {currentLobby.Name}, waiting for players...");
            isInLobby = true;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            isSearching = false;
        }
    }

    
    async void SendHeartbeat()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Heartbeat failed: {e.Message}");
        }
    }
    
    async void CheckLobbyStatus()
    {
        // Stop checking if race already started
        if (raceStarted) return;
        
        try
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            
            int playerCount = currentLobby.Players.Count;
            Debug.Log($"Players in lobby: {playerCount}/{maxPlayers}");
            
            // Start race when minimum players reached (host only)
            if (playerCount >= minPlayersToStart && IsHost())
            {
                Debug.Log("Minimum players reached, starting race!");
                StartRace();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to check lobby: {e.Message}");
        }
    }
    
    void StartRace()
    {
        if (!IsHost())
        {
            Debug.LogWarning("StartRace called but I'm not host!");
            return;
        }
        
        // Prevent multiple calls
        if (raceStarted)
        {
            Debug.Log("Race already started, ignoring...");
            return;
        }
        
        raceStarted = true;
        Debug.Log($"HOST STARTING RACE! Loading scene: {selectedMap}");
        
        // Check if NetworkManager is ready
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null!");
            return;
        }
        
        if (NetworkManager.Singleton.SceneManager == null)
        {
            Debug.LogError("NetworkManager.SceneManager is null! Is 'Enable Scene Management' checked?");
            return;
        }
        
        // Use NetworkManager scene loading
        NetworkManager.Singleton.SceneManager.LoadScene(selectedMap, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    
    Player GetPlayerData(int carIndex)
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, NetworkBootstrap.Instance.playerName) },
                { "SelectedCar", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, carIndex.ToString()) } // ADD THIS
            }
        };
    }

    public Lobby GetCurrentLobby()
    {
        return currentLobby;
    }


    
    bool IsHost()
    {
        return currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    
    public string GetSelectedMap()
    {
        return selectedMap;
    }
}
