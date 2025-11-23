using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    // Network variables - synced across all clients
    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> hasFinished = new NetworkVariable<bool>(false);
    public NetworkVariable<float> finishTime = new NetworkVariable<float>(0f);
    
    // Local variables
    [HideInInspector] public string playerName;
    [HideInInspector] public ulong clientId;
    
    public override void OnNetworkSpawn()
    {
        // Called when this object spawns on the network
        clientId = OwnerClientId;
        
        // Set player name from NetworkBootstrap
        if (IsOwner)
        {
            playerName = NetworkBootstrap.Instance.playerName;
            Debug.Log($"NetworkPlayer spawned for: {playerName}");
        }
        
        // Register this player with RaceController (we'll create this next)
        // RaceController.Instance.RegisterPlayer(this);
    }
    
    public override void OnNetworkDespawn()
    {
        // Called when player disconnects
        Debug.Log($"Player disconnected: {playerName}");
        
        // Mark as DNF if race is active
        // RaceController.Instance.MarkPlayerDNF(this);
    }
    
    // Called when local player finishes race
    public void FinishRace(float time)
    {
        if (!IsOwner) return; // Only the owner can finish their own race
        
        FinishRaceServerRpc(time);
    }
    
    [Rpc(SendTo.Server)]
    void FinishRaceServerRpc(float time)
    {
        // Server updates the finish data
        hasFinished.Value = true;
        finishTime.Value = time;
        
        Debug.Log($"{playerName} finished with time: {time}");
        
        // Notify all clients
        NotifyPlayerFinishedClientRpc(playerName, time);
    }
    
    [Rpc(SendTo.Everyone)]
    void NotifyPlayerFinishedClientRpc(string name, float time)
    {
        // All clients receive this and can update UI
        Debug.Log($"Player finished notification: {name} - {time}s");
        
        //RaceController.Instance.OnPlayerFinished(name, time);
    }
}
