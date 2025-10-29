using UnityEngine;

public class AnimationTriggerStart : MonoBehaviour
{
    public Animator targetAnimator; // Drag your figure here in Inspector
    public string animationName = "YourAnimationName"; // Or parameter name
    
    void OnTriggerEnter(Collider other)
    {
        // Check if player/car entered
        if(other.CompareTag("Player")) // Make sure your player has "Player" tag
        {
            // Method 1: Play animation directly
            targetAnimator.Play(animationName);
            Debug.Log("animationName");
            
            // Method 2: Or use trigger parameter
            // targetAnimator.SetTrigger("StartAnim");
            
            // Method 3: Or use bool
            // targetAnimator.SetBool("isPlaying", true);
        }
    }
}
