using UnityEngine;
using System.Collections.Generic;
using System.IO;

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
}

public class GhostRecorder : MonoBehaviour
{
    public Transform carTransform;
    public float recordInterval = 0.05f;
    public string ghostFileName = "ghost_data.json";
    
    private GhostData ghostData = new GhostData();
    private float timer = 0f;
    private float recordTimer = 0f;
    private bool isRecording = false;
    
    void Update()
    {
        if(isRecording)
        {
            timer += Time.deltaTime;
            recordTimer += Time.deltaTime;
            
            if(recordTimer >= recordInterval)
            {
                ghostData.frames.Add(new GhostFrame(timer, carTransform.position, carTransform.rotation));
                recordTimer = 0f;
            }
        }
        
        if(Input.GetKeyDown(KeyCode.R)) StartRecording();
        if(Input.GetKeyDown(KeyCode.T)) StopRecording();
    }
    
    public void StartRecording()
    {
        ghostData.frames.Clear();
        timer = 0f;
        recordTimer = 0f;
        isRecording = true;
        Debug.Log("Recording started");
    }
    
    public void StopRecording()
    {
        isRecording = false;
        SaveGhostData();
        Debug.Log("Recording stopped. Frames: " + ghostData.frames.Count);
    }
    
void SaveGhostData()
{
    string path = Path.Combine(Application.dataPath, "Scripts", "Ghost", ghostFileName);
    File.WriteAllText(path, JsonUtility.ToJson(ghostData, true));
    Debug.Log("Ghost saved: " + path);
}

public GhostData LoadGhostData()
{
    string path = Path.Combine(Application.dataPath, "Scripts", "Ghost", ghostFileName);
    
    if(File.Exists(path))
    {
        Debug.Log("Ghost loaded: " + path);
        return JsonUtility.FromJson<GhostData>(File.ReadAllText(path));
    }
    
    Debug.LogWarning("Ghost file not found: " + path);
    return null;
}

}
