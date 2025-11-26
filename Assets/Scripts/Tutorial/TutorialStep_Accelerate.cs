using UnityEngine;

public class TutorialStep_Accelerate : TutorialStep
{
    [SerializeField] private float requiredSpeed = 10f;
    [SerializeField] private float requiredTime = 2f;
    
    private Rigidbody playerRb;
    private float timeAboveSpeed = 0f;

    public override void Activate()
    {
        base.Activate();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
        }
        timeAboveSpeed = 0f;
    }

    public override bool IsCompleted()
    {
        if (playerRb == null) return false;

        float currentSpeed = playerRb.linearVelocity.magnitude;

        if (currentSpeed >= requiredSpeed)
        {
            timeAboveSpeed += Time.deltaTime;
        }
        else
        {
            timeAboveSpeed = 0f;
        }

        return timeAboveSpeed >= requiredTime;
    }
}
