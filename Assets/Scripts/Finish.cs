using UnityEngine;
using Unity.Netcode;

public class Finish : MonoBehaviour
{
    [Header("References")]
    public GameObject finishUI;
    
    private bool hasFinished = false;

    void Start()
    {
        if (finishUI != null)
        {
            finishUI.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasFinished)
        {
            if (RaceManager.Instance.IsMultiplayer())
            {
                NetworkObject netObj = other.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsOwner) return;
            }
            
            var checkpointRespawn = other.GetComponent<CheckpointRespawn>();
            if (checkpointRespawn != null)
            {
                CheckFinish(checkpointRespawn);
            }
        }
    }

    void CheckFinish(CheckpointRespawn checkpointRespawn)
    {
        int visited = checkpointRespawn.VisitedCheckpoints;
        int total = checkpointRespawn.TotalCheckpoints;

        if (visited >= total)
        {
            Debug.Log("ALL CHECKPOINTS CLEARED - FINISH!");
            FinishRace();
        }
        else
        {
            Debug.Log($"Can't finish! Missing {total - visited} checkpoint(s)");
        }
    }

    void FinishRace()
    {
        hasFinished = true;
        
        // FIND timer dynamically
        Timer timer = FindFirstObjectByType<Timer>();
        
        float finalTime = 0f;
        
        if (timer != null)
        {
            timer.StopTimer();
            finalTime = timer.elapsedTime;
            Debug.Log($"Timer found! Final time: {finalTime}s"); // ADD THIS LOG
        }
        else
        {
            Debug.LogError("Timer not found in scene!");
        }

        if (finishUI != null)
        {
            finishUI.SetActive(true);
        }
        
        Debug.Log($"Finished race in {finalTime}s");
        
        if (RaceManager.Instance.IsMultiplayer())
        {
            string playerName = NetworkBootstrap.Instance.playerName;
            RaceResultsManager resultsManager = FindFirstObjectByType<RaceResultsManager>();
            
            if (resultsManager != null)
            {
                Debug.Log($"Sending finish time: {playerName} - {finalTime}s");
                resultsManager.ReportFinishTime(playerName, finalTime);
            }
            else
            {
                Debug.LogError("RaceResultsManager not found in scene!");
            }
        }
    }
}
