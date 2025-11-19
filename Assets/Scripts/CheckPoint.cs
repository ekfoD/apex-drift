using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class CheckpointRespawn : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool matchCheckpointRotation = true;

    private KeyCode resetKey = KeyCode.R;
    private KeyCode restartSceneKey = KeyCode.P;

    private Rigidbody rb;
    private Transform lastCheckpoint;

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
        lastCheckpoint = spawnPoint != null ? spawnPoint : transform;
        totalCheckpoints = GameObject.FindGameObjectsWithTag("Checkpoint").Length;
        visitedCheckpoints = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(resetKey)) ResetToCheckpoint();
        if (Input.GetKeyDown(restartSceneKey))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Checkpoint")) return;
        if (!other.enabled) return; // already used

        lastCheckpoint = other.transform;
        other.enabled = false;      // prevent re-trigger
        visitedCheckpoints++;
    }

    private void ResetToCheckpoint()
    {
        // clear motion so speed/rotation aren't retained
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // teleport to checkpoint
        rb.position = lastCheckpoint.position;
        rb.rotation = matchCheckpointRotation
            ? lastCheckpoint.rotation
            : Quaternion.Euler(0f, lastCheckpoint.eulerAngles.y, 0f);

        rb.Sleep();
        rb.WakeUp();
    }
}
