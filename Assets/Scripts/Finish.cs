using UnityEngine;

public class Finish : MonoBehaviour
{
    [Header("References")]
    public GameObject finishUI; 
    public Timer timer; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var checkpointRespawn = other.GetComponent<CheckpointRespawn>();
            if (checkpointRespawn != null)
            {
                CheckFinish(checkpointRespawn);
            }
            else
            {
                Debug.LogWarning("Player has no CheckpointRespawn script!");
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
        // Stop timer
        if (timer != null)
        {
            timer.StopTimer();
        }

        // Show finish UI
        if (finishUI != null)
        {
            finishUI.SetActive(true);
        }
    }
}
