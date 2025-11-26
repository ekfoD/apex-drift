using UnityEngine;

public class GhostPlayer : MonoBehaviour
{
    [Header("Playback Settings")]
    public GhostRecorder ghostRecorder;
    public GameObject ghostCarPrefab;

    [Header("Visibility Settings")]
    public KeyCode toggleVisibilityKey = KeyCode.N;

    private GhostData ghostData;
    private GameObject ghostCar;
    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = false;
    private bool isGhostVisible = true;

    void Start()
    {
        LoadAndInitializeGhost();
    }

    void Update()
    {
        // Recording controls
        if (Input.GetKeyDown(KeyCode.V))
        {
            ghostRecorder.StartRecordingManually();
            Debug.Log("Recording started.");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ghostRecorder.StopRecordingManually();
            Debug.Log("Recording stopped.");

            // Reload the ghost after recording
            LoadAndInitializeGhost();
        }

        // Toggle ghost visibility
        if (Input.GetKeyDown(toggleVisibilityKey))
        {
            ToggleGhostVisibility();
        }

        // Update playback
        if (isPlaying && ghostData != null && ghostCar != null)
        {
            UpdatePlayback();
        }
    }

    private void LoadAndInitializeGhost()
    {
        // Clean up existing ghost
        if (ghostCar != null)
        {
            Destroy(ghostCar);
        }

        // Load ghost data
        if (ghostRecorder != null)
        {
            ghostData = ghostRecorder.LoadGhostData();

            if (ghostData == null || ghostData.frames.Count == 0)
            {
                Debug.LogWarning("No ghost data available.");
                return;
            }

            // Initialize ghost car
            if (ghostCarPrefab != null)
            {
                ghostCar = Instantiate(ghostCarPrefab);
                ghostCar.name = "Ghost Car";
                ghostCar.SetActive(isGhostVisible);
                StartPlayback();
                Debug.Log($"Ghost initialized with {ghostData.frames.Count} frames.");
            }
            else
            {
                Debug.LogError("Ghost car prefab is missing!");
            }
        }
        else
        {
            Debug.LogError("GhostRecorder reference is missing!");
        }
    }

    private void UpdatePlayback()
    {
        timer += Time.deltaTime;

        // Find the current frame pair for interpolation
        while (currentFrame < ghostData.frames.Count - 1)
        {
            GhostFrame currentFrameData = ghostData.frames[currentFrame];
            GhostFrame nextFrameData = ghostData.frames[currentFrame + 1];

            if (timer >= currentFrameData.time && timer <= nextFrameData.time)
            {
                // Interpolate between frames
                float frameDuration = nextFrameData.time - currentFrameData.time;
                float t = frameDuration > 0 ? (timer - currentFrameData.time) / frameDuration : 0f;

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
            else if (timer > nextFrameData.time)
            {
                currentFrame++;
            }
            else
            {
                break;
            }
        }

        // Loop playback or stop at the end
        if (currentFrame >= ghostData.frames.Count - 1)
        {
            // Restart playback (loop)
            currentFrame = 0;
            timer = 0f;
            Debug.Log("Ghost playback looped.");
        }
    }

    private void StartPlayback()
    {
        timer = 0f;
        currentFrame = 0;
        isPlaying = true;
        Debug.Log("Ghost playback started.");
    }

    public void ToggleGhostVisibility()
    {
        if (ghostCar != null)
        {
            isGhostVisible = !isGhostVisible;
            ghostCar.SetActive(isGhostVisible);
            Debug.Log($"Ghost visibility: {(isGhostVisible ? "ON" : "OFF")}");
        }
        else
        {
            Debug.LogWarning("No ghost car to toggle.");
        }
    }

    public void ShowGhost()
    {
        if (ghostCar != null)
        {
            isGhostVisible = true;
            ghostCar.SetActive(true);
            Debug.Log("Ghost shown.");
        }
    }

    public void HideGhost()
    {
        if (ghostCar != null)
        {
            isGhostVisible = false;
            ghostCar.SetActive(false);
            Debug.Log("Ghost hidden.");
        }
    }
}