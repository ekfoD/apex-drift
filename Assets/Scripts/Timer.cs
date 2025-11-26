using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    public float elapsedTime; // Made public so Finish can access it
    public bool isRunning = false; // Changed to false by default (waits for countdown)

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // ADD THIS METHOD - Called by RaceManager after countdown
    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
        Debug.Log("Timer started");
    }

    public void StopTimer()
    {
        isRunning = false;
        Debug.Log($"Timer stopped at: {elapsedTime}s");
    }
    
    // ADD THIS METHOD - Optional helper
    public float GetTime()
    {
        return elapsedTime;
    }
}