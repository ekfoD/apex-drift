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
    private bool hasFinished = false;

    void Start()
    {
        // Check references
        if (ghostRecorder == null)
        {
            Debug.LogError("GhostRecorder reference is missing! Please assign it in the inspector.");
        }

        if (ghostCarPrefab == null)
        {
            Debug.LogError("Ghost car prefab is missing! Please assign it in the inspector.");
        }

        LoadAndInitializeGhost();
    }

    void Update()
    {
        // Toggle ghost visibility
        if (Input.GetKeyDown(toggleVisibilityKey))
        {
            ToggleGhostVisibility();
        }

        // Update playback
        if (isPlaying && !hasFinished && ghostData != null && ghostCar != null)
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
            ghostCar = null;
        }

        // Check references before attempting to load
        if (ghostRecorder == null)
        {
            Debug.LogError("GhostRecorder reference is missing!");
            return;
        }

        if (ghostCarPrefab == null)
        {
            Debug.LogError("Ghost car prefab is missing!");
            return;
        }

        // Load ghost data
        ghostData = ghostRecorder.LoadGhostData();

        if (ghostData == null || ghostData.frames.Count == 0)
        {
            Debug.LogWarning("No ghost data available. Record a ghost first by pressing V to start and B to stop.");
            return;
        }

        // Initialize ghost car
        ghostCar = Instantiate(ghostCarPrefab);
        ghostCar.name = "Ghost Car";
        ghostCar.SetActive(isGhostVisible);
        StartPlayback();
        Debug.Log($"Ghost initialized with {ghostData.frames.Count} frames.");
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

        // Stop at the end instead of looping
        if (currentFrame >= ghostData.frames.Count - 1)
        {
            // Set to final frame position
            GhostFrame finalFrame = ghostData.frames[ghostData.frames.Count - 1];
            ghostCar.transform.position = finalFrame.position;
            ghostCar.transform.rotation = finalFrame.rotation;

            hasFinished = true;
            isPlaying = false;
            Debug.Log("Ghost playback finished.");
        }
    }

    private void StartPlayback()
    {
        timer = 0f;
        currentFrame = 0;
        isPlaying = true;
        hasFinished = false;
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
            Debug.LogWarning("No ghost car instantiated. Make sure you have recorded a ghost first (V to start, B to stop) and the ghost car prefab is assigned.");
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