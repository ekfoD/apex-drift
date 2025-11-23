using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }
    
    [Header("UI References")]
    public TMP_InputField roomNameInput;
    public TMP_Text statusText;
    public GameObject lobbyMenuPanel;
    public GameObject roomPanel;
    public TMP_Text roomInfoText;
    public TMP_Text playersListText;
    
    [Header("Settings")]
    public int maxPlayers = 4;
    
    private Lobby currentLobby;
    private float heartbeatTimer = 0f;
    private float lobbyUpdateTimer = 0f;
    
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
        // Show lobby menu, hide room panel
        if(lobbyMenuPanel != null) lobbyMenuPanel.SetActive(true);
        if(roomPanel != null) roomPanel.SetActive(false);
    }
    
    void Update()
    {
        // Send heartbeat to keep lobby alive (host only)
        if (currentLobby != null && IsHost())
        {
            heartbeatTimer += Time.deltaTime;
            if (heartbeatTimer >= 15f)
            {
                heartbeatTimer = 0f;
                SendHeartbeat();
            }
        }
        
        // Poll lobby for updates
        if (currentLobby != null)
        {
            lobbyUpdateTimer += Time.deltaTime;
            if (lobbyUpdateTimer >= 1.1f)
            {
                lobbyUpdateTimer = 0f;
                UpdateLobby();
            }
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
    
    // Create a new room
public async void CreateRoom()
{
    string roomName = roomNameInput.text;
    if (string.IsNullOrEmpty(roomName))
    {
        roomName = "Room_" + Random.Range(1000, 9999);
    }
    
    if(statusText != null) statusText.text = "Creating room...";
    
    try
    {
        // Create relay first
        string relayJoinCode = await RelayManager.Instance.CreateRelay(maxPlayers);
        
        if(string.IsNullOrEmpty(relayJoinCode))
        {
            if(statusText != null) statusText.text = "Failed to create relay!";
            return;
        }
        
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = false,
            Player = GetPlayer(),
            Data = new Dictionary<string, DataObject>
            {
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
            }
        };
        
        currentLobby = await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, options);
        
        Debug.Log($"Lobby created: {currentLobby.Name} with relay code: {relayJoinCode}");
        
        // Switch to room panel
        if(lobbyMenuPanel != null) lobbyMenuPanel.SetActive(false);
        if(roomPanel != null) roomPanel.SetActive(true);
        
        UpdateRoomUI();
        if(statusText != null) statusText.text = "Room created!";
    }
    catch (LobbyServiceException e)
    {
        Debug.LogError($"Failed to create lobby: {e.Message}");
        if(statusText != null) statusText.text = "Failed to create room!";
    }
}
    
    // Join a random room
    public async void JoinRandomRoom()
{
    if(statusText != null) statusText.text = "Searching for room...";
    
    try
    {
        QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
        {
            Player = GetPlayer()
        };
        
        currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
        
        Debug.Log($"Joined lobby: {currentLobby.Name}");
        
        // Get relay join code from lobby
        string relayJoinCode = currentLobby.Data["RelayJoinCode"].Value;
        
        // Join relay
        bool joined = await RelayManager.Instance.JoinRelay(relayJoinCode);
        
        if(!joined)
        {
            if(statusText != null) statusText.text = "Failed to connect!";
            await LeaveRoom();
            return;
        }
        
        // Switch to room panel
        if(lobbyMenuPanel != null) lobbyMenuPanel.SetActive(false);
        if(roomPanel != null) roomPanel.SetActive(true);
        
        UpdateRoomUI();
        if(statusText != null) statusText.text = "Joined room!";
    }
    catch (LobbyServiceException e)
    {
        Debug.LogError($"Failed to join lobby: {e.Message}");
        if(statusText != null) statusText.text = "No rooms found! Create one.";
    }
}
    
    // Leave current room
    public async Task LeaveRoom()
    {
        if (currentLobby == null) return;
        
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
            
            currentLobby = null;
            
            // Switch back to lobby menu
            if(roomPanel != null) roomPanel.SetActive(false);
            if(lobbyMenuPanel != null) lobbyMenuPanel.SetActive(true);
            
            if(statusText != null) statusText.text = "Left room";
            Debug.Log("Left lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to leave lobby: {e.Message}");
        }
    }
    public async void LeaveRoomButton()
    {
        await LeaveRoom();
    }
    
    // Update lobby data
    async void UpdateLobby()
    {
        try
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            UpdateRoomUI();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to update lobby: {e.Message}");
        }
    }
    
    // Update room UI
    void UpdateRoomUI()
    {
        if (currentLobby == null) return;
        
        if(roomInfoText != null)
        {
            roomInfoText.text = $"Room: {currentLobby.Name}\nPlayers: {currentLobby.Players.Count}/{currentLobby.MaxPlayers}";
        }
        
        // List all players
        if(playersListText != null)
        {
            playersListText.text = "Players:\n";
            foreach (var player in currentLobby.Players)
            {
                string playerName = player.Data["PlayerName"].Value;
                bool isHost = player.Id == currentLobby.HostId;
                playersListText.text += $"• {playerName} {(isHost ? "(Host)" : "")}\n";
            }
        }
    }
    
    // Helper: Create player data
    Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, NetworkBootstrap.Instance.playerName) }
            }
        };
    }
    
    // Helper: Check if local player is host
    bool IsHost()
    {
        return currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    
    public Lobby GetCurrentLobby() => currentLobby;
}
