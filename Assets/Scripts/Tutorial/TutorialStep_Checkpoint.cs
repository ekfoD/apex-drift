using UnityEngine;

public class TutorialStep_Checkpoint : TutorialStep
{
    [SerializeField] private GameObject checkpointTrigger;
    private bool checkpointReached = false;

    public override void Activate()
    {
        base.Activate();
        checkpointReached = false;

        // Make sure checkpoint has a trigger detector
        if (checkpointTrigger != null)
        {
            TutorialCheckpointTrigger trigger = checkpointTrigger.GetComponent<TutorialCheckpointTrigger>();
            if (trigger == null)
            {
                trigger = checkpointTrigger.AddComponent<TutorialCheckpointTrigger>();
            }
            trigger.onCheckpointReached += OnCheckpointReached;
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        
        if (checkpointTrigger != null)
        {
            TutorialCheckpointTrigger trigger = checkpointTrigger.GetComponent<TutorialCheckpointTrigger>();
            if (trigger != null)
            {
                trigger.onCheckpointReached -= OnCheckpointReached;
            }
        }
    }

    void OnCheckpointReached()
    {
        checkpointReached = true;
    }

    public override bool IsCompleted()
    {
        return checkpointReached;
    }
}

// Helper component for checkpoint
public class TutorialCheckpointTrigger : MonoBehaviour
{
    public System.Action onCheckpointReached;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onCheckpointReached?.Invoke();
        }
    }
}
