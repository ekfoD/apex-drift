using UnityEngine;

public class Finish : MonoBehaviour
{
    [Header("References")]
    public GameObject finishUI; 
    public Timer timer; 
    
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            CheckFinish();
        }
    }
    
    void CheckFinish()
    {
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        
        if(checkpoints.Length == 0)
        {
            Debug.Log("ALL CHECKPOINTS CLEARED - FINISH!");
            FinishRace();
        }
        else
        {
            Debug.Log("Can't finish! Missing " + checkpoints.Length + " checkpoint(s)");
        }
    }
    
    void FinishRace()
    {
        // Stop timer
        if(timer != null)
        {
            timer.StopTimer();
        }
        
        // Show finish UI
        if(finishUI != null)
        {
            finishUI.SetActive(true);
        }
    }
}
