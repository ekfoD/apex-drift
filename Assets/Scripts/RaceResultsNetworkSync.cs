using Unity.Netcode;

public class RaceResultsNetworkSync : NetworkBehaviour
{
    [Rpc(SendTo.Server)]
    public void ReportFinishTimeServerRpc(string playerName, float time)
    {
        // Broadcast to all clients
        AddFinishTimeClientRpc(playerName, time);
    }
    
    [Rpc(SendTo.Everyone)]
    void AddFinishTimeClientRpc(string playerName, float time)
    {
        if (RaceResultsManager.Instance != null)
        {
            RaceResultsManager.Instance.AddFinishTime(playerName, time);
        }
    }
}