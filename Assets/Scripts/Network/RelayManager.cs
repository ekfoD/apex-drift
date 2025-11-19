using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    
    private string joinCode;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    // Host creates relay allocation
    public async Task<string> CreateRelay(int maxPlayers = 4)
    {
        try
        {
            // Create relay allocation (maxPlayers - 1 because host counts as one)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            
            // Get join code
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            Debug.Log($"Relay created with join code: {joinCode}");
            
            // Set up transport - FIXED
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            
            // Start as host
            NetworkManager.Singleton.StartHost();
            
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to create relay: {e.Message}");
            return null;
        }
    }
    
    // Client joins relay
    public async Task<bool> JoinRelay(string joinCode)
    {
        try
        {
            // Join allocation
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            Debug.Log($"Joined relay with code: {joinCode}");
            
            // Set up transport - FIXED
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );
            
            // Start as client
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
