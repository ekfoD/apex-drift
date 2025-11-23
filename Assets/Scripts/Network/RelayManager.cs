using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    
    [Header("Settings")]
    public GameObject networkPlayerPrefab;
    
    private string joinCode;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Subscribe to connection events
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    
    void OnDestroy()
    {
        // Unsubscribe
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    
    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        
        // Only server spawns NetworkPlayer objects
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnNetworkPlayer(clientId);
        }
    }
    
    void SpawnNetworkPlayer(ulong clientId)
    {
        if (networkPlayerPrefab == null)
        {
            Debug.LogError("NetworkPlayer prefab not assigned to RelayManager!");
            return;
        }
        
        // Spawn NetworkPlayer for this client
        GameObject playerObj = Instantiate(networkPlayerPrefab);
        NetworkObject netObj = playerObj.GetComponent<NetworkObject>();
        
        // Spawn with ownership of the client
        netObj.SpawnAsPlayerObject(clientId);
        
        Debug.Log($"NetworkPlayer spawned for client: {clientId}");
    }
        
    // Host creates relay allocation
    public async Task<string> CreateRelay(int maxPlayers = 4)
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            return null;
        }
        
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            Debug.Log($"Relay created with join code: {joinCode}");
            
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport component not found!");
                return null;
            }
            
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            
            NetworkManager.Singleton.StartHost();
            
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to create relay: {e.Message}");
            return null;
        }
    }
    
    public async Task<bool> JoinRelay(string joinCode)
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            return false;
        }
        
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            Debug.Log($"Joined relay with code: {joinCode}");
            
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport component not found!");
                return false;
            }
            
            transport.SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );
            
            NetworkManager.Singleton.StartClient();
            
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to join relay: {e.Message}");
            return false;
        }
    }
}
