using UnityEngine;

public class Finish : MonoBehaviour
{
    private bool hasFinished = false;
    public GhostRecorder ghostRecorder;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasFinished)
        {
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

        ghostRecorder.StopRecording();

        // Get timer
        Timer timer = FindFirstObjectByType<Timer>();
        float finalTime = 0f;
        
        if (timer != null)
        {
            timer.StopTimer();
            finalTime = timer.elapsedTime;
            Debug.Log($"Final time: {finalTime}s");
        }
        else
        {
            Debug.LogError("Timer not found in scene!");
        }
        
        // Get player name
        string playerName = "Player";
        if (RaceManager.Instance.IsMultiplayer())
        {
            playerName = NetworkBootstrap.Instance.playerName;
        }
        
        // Send to RaceResultsManager (works for both SP and MP!)
        RaceResultsManager resultsManager = FindFirstObjectByType<RaceResultsManager>();
        if (resultsManager != null)
        {
            resultsManager.ReportFinishTime(playerName, finalTime);
        }
        else
        {
            Debug.LogError("RaceResultsManager not found!");
        }
    }
}