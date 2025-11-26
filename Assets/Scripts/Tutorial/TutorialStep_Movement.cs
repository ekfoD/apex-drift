using UnityEngine;

public class TutorialStep_Movement : TutorialStep
{
    [SerializeField] private float requiredDistance = 5f;
    private Vector3 startPosition;
    private Transform playerTransform;

    public override void Activate()
    {
        base.Activate();
        
        // Find player (adjust tag as needed)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            startPosition = playerTransform.position;
        }
    }

    public override bool IsCompleted()
    {
        if (playerTransform == null) return false;
        
        float distanceMoved = Vector3.Distance(startPosition, playerTransform.position);
        return distanceMoved >= requiredDistance;
    }
}
