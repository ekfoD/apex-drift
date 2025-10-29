using UnityEngine;

public class AnimationTriggerEnd : MonoBehaviour
{
    public Transform animatedGroup; // Drag the parent empty object here
    public string animationName = "Idle";
    
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // Get all Animators in children
            Animator[] animators = animatedGroup.GetComponentsInChildren<Animator>();
            
            // Play animation on all of them
            foreach(Animator anim in animators)
            {
                anim.Play(animationName);
            }
        }
    }
}