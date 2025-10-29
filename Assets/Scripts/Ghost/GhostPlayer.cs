using UnityEngine;

public class GhostPlayer : MonoBehaviour
{
    [Header("Playback Settings")]
    public GhostRecorder ghostRecorder;
    public GameObject ghostCarPrefab;
    
    private GhostData ghostData;
    private GameObject ghostCar;
    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;
    
    void Start()
    {
        if(ghostRecorder != null)
        {
            ghostData = ghostRecorder.LoadGhostData();
        }
        
        if(ghostCarPrefab != null && ghostData != null && ghostData.frames.Count > 0)
        {
            ghostCar = Instantiate(ghostCarPrefab);
            StartPlayback();
        }
        else
        {
            Debug.LogWarning("Ghost playback failed: Missing prefab or ghost data");
        }
    }
    
    void Update()
    {
        if(!isPlaying || ghostData == null || ghostCar == null) return;
        
        timer += Time.deltaTime;
        
        // Find the two frames we're between
        while(currentFrame < ghostData.frames.Count - 1)
        {
            GhostFrame currentFrameData = ghostData.frames[currentFrame];
            GhostFrame nextFrameData = ghostData.frames[currentFrame + 1];
            
            // Are we between these two frames?
            if(timer >= currentFrameData.time && timer <= nextFrameData.time)
            {
                // Calculate interpolation value (0 to 1)
                float frameDuration = nextFrameData.time - currentFrameData.time;
                float t = (timer - currentFrameData.time) / frameDuration;
                
                // Smoothly interpolate position and rotation
                ghostCar.transform.position = Vector3.Lerp(
                    currentFrameData.position,
                    nextFrameData.position,
                    t
                );
                
                ghostCar.transform.rotation = Quaternion.Slerp(
                    currentFrameData.rotation,
                    nextFrameData.rotation,
                    t
                );
                
                break;
            }
            else if(timer > nextFrameData.time)
            {
                // Move to next frame
                currentFrame++;
            }
            else
            {
                break;
            }
        }
        
        // Stop at end
        if(currentFrame >= ghostData.frames.Count - 2)
        {
            isPlaying = false;
            Debug.Log("Ghost playback finished");
        }
    }
    
    void StartPlayback()
    {
        Debug.Log("Starting ghost playback...");
        timer = 0f;
        currentFrame = 0;
        isPlaying = true;
    }
}
