using Unity.Netcode;

public class RaceManagerNetwork : NetworkBehaviour
{
    [Rpc(SendTo.Everyone)]
    public void StartCountdownClientRpc()
    {
        RaceManager.Instance.StartCountdown();
    }
}