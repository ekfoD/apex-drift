using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class CheckpointRespawn : MonoBehaviour
{
    [SerializeField] private bool matchCheckpointRotation = true;

    private KeyCode resetKey = KeyCode.R;
    private KeyCode restartSceneKey = KeyCode.P;

    private Rigidbody rb;
    private Transform lastCheckpoint;
    private Vector3 initialSpawnPos;
    private Quaternion initialSpawnRot;
    private bool spawnPointSet = false;

    // Progress tracking
    private int totalCheckpoints;
    private int visitedCheckpoints;

    public int VisitedCheckpoints => visitedCheckpoints;
    public int TotalCheckpoints   => totalCheckpoints;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // If spawn point wasn't set externally, use current position
        if (!spawnPointSet)
        {
            initialSpawnPos = transform.position;
            initialSpawnRot = transform.rotation;
            CreateSpawnReference();
        }
        
        totalCheckpoints = GameObject.FindGameObjectsWithTag("Checkpoint").Length;
        visitedCheckpoints = 0;
    }

    // Called by SpawnPoint when car is spawned
    public void SetSpawnPoint(Vector3 position, Quaternion rotation)
    {
        initialSpawnPos = position;
        initialSpawnRot = rotation;
        spawnPointSet = true;
        CreateSpawnReference();
        
        Debug.Log($"Spawn point set to: {position}");
    }
    
    void CreateSpawnReference()
    {
        GameObject tempGO = new GameObject("SpawnReference");
        tempGO.transform.position = initialSpawnPos;
        tempGO.transform.rotation = initialSpawnRot;
        lastCheckpoint = tempGO.transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(resetKey)) ResetToCheckpoint();
        
        // Disable scene restart in MP
        if (Input.GetKeyDown(restartSceneKey))
        {
            string gameMode = PlayerPrefs.GetString("GameMode", "Singleplayer");
            if (gameMode == "Singleplayer")
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Checkpoint")) return;
        if (!other.enabled) return;

        lastCheckpoint = other.transform;
        other.enabled = false;
        visitedCheckpoints++;
    }

    private void ResetToCheckpoint()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = lastCheckpoint.position;
        rb.rotation = matchCheckpointRotation
            ? lastCheckpoint.rotation
            : Quaternion.Euler(0f, lastCheckpoint.eulerAngles.y, 0f);

        rb.Sleep();
        rb.WakeUp();
    }
}
