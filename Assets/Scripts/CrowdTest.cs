using UnityEngine;

public class CrowdTest : MonoBehaviour
{
    private Animator animator;
    
    [Header("Manual Animation Control")]
    public bool isIdle = true;
    public bool isCheer = false;
    
    private bool previousIdle = true;
    private bool previousCheer = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("IsCheering", false);
    }
    
    void Update()
    {
        // Check if Idle checkbox changed
        if (isIdle != previousIdle)
        {
            if (isIdle)
            {
                // Set to Idle
                animator.SetBool("IsCheering", false);
                isCheer = false;
                Debug.Log(gameObject.name + " is now IDLE!");
            }
            previousIdle = isIdle;
        }
        
        // Check if Cheer checkbox changed
        if (isCheer != previousCheer)
        {
            if (isCheer)
            {
                // Set to Cheering
                animator.SetBool("IsCheering", true);
                isIdle = false;
                Debug.Log(gameObject.name + " is now CHEERING!");
            }
            previousCheer = isCheer;
        }
    }
}