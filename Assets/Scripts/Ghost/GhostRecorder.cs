using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GhostFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
    public GhostFrame(float t, Vector3 pos, Quaternion rot)
    {
        time = t;
        position = pos;
        rotation = rot;
    }
}

[System.Serializable]
public class GhostData
{
    public List<GhostFrame> frames = new List<GhostFrame>();
    public float bestTime = float.MaxValue;
}

public class GhostRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    public Transform carTransform;
    public float recordInterval = 0.05f;

    private string ghostFileName;
    private GhostData ghostData = new GhostData();
    private float timer = 0f;
    private float recordTimer = 0f;
    private bool isRecording = false;
    private float currentRunTime = 0f;

    void Awake()
    {
        InitializeFileName();
    }

    void Start()
    {
        if (string.IsNullOrEmpty(ghostFileName))
        {
            InitializeFileName();
        }

        StartRecording();
    }

    private void InitializeFileName()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        ghostFileName = $"{sceneName}_ghost.json";
        Debug.Log($"Ghost filename set to: {ghostFileName}");
    }

    void Update()
    {
        if (isRecording)
        {
            timer += Time.deltaTime;
            recordTimer += Time.deltaTime;
            currentRunTime += Time.deltaTime;

            if (recordTimer >= recordInterval)
            {
                if (carTransform != null)
                {
                    ghostData.frames.Add(new GhostFrame(timer, carTransform.position, carTransform.rotation));
                }
                recordTimer = 0f;
            }
        }
    }

    public void StartRecording()
    {
        // Find the car if not assigned
        if (carTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                carTransform = player.transform;
                Debug.Log("Car found and assigned for recording.");
            }
            else
            {
                Debug.LogError("Cannot start recording: Player car not found!");
                return;
            }
        }

        ghostData.frames.Clear();
        timer = 0f;
        recordTimer = 0f;
        currentRunTime = 0f;
        isRecording = true;
        Debug.Log("Recording started");
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;

        // Load existing ghost data to compare times
        GhostData existingGhost = LoadGhostData();
        float existingBestTime = (existingGhost != null) ? existingGhost.bestTime : float.MaxValue;

        Debug.Log($"Current run time: {currentRunTime:F2}s");
        Debug.Log($"Previous best time: {(existingBestTime == float.MaxValue ? "None" : existingBestTime.ToString("F2") + "s")}");

        // Save if this is a new best time or if no ghost exists
        if (currentRunTime < existingBestTime || existingGhost == null)
        {
            ghostData.bestTime = currentRunTime;
            SaveGhostData();
            Debug.Log($"NEW BEST TIME! Recording saved: {currentRunTime:F2}s (Frames: {ghostData.frames.Count})");
        }
        else
        {
            Debug.Log($"Time not improved. Current: {currentRunTime:F2}s, Best: {existingBestTime:F2}s. Recording discarded.");
        }
    }

    void SaveGhostData()
    {
        if (string.IsNullOrEmpty(ghostFileName))
        {
            InitializeFileName();
        }

        string path = Path.Combine(Application.persistentDataPath, ghostFileName);
        File.WriteAllText(path, JsonUtility.ToJson(ghostData, true));
        Debug.Log($"Ghost saved: {path}");
    }

    public GhostData LoadGhostData()
    {
        if (string.IsNullOrEmpty(ghostFileName))
        {
            InitializeFileName();
        }

        string path = Path.Combine(Application.persistentDataPath, ghostFileName);
        if (File.Exists(path))
        {
            Debug.Log($"Ghost loaded: {path}");
            return JsonUtility.FromJson<GhostData>(File.ReadAllText(path));
        }
        Debug.LogWarning($"Ghost file not found: {path}");
        return null;
    }

    public float GetBestTime()
    {
        GhostData data = LoadGhostData();
        return (data != null) ? data.bestTime : float.MaxValue;
    }

    public bool IsRecording()
    {
        return isRecording;
    }
}