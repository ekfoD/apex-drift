using UnityEngine;
using System.Collections;

public class CrowdCharacter : MonoBehaviour
{
    private Animator animator;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private static readonly int IsCheering = Animator.StringToHash("IsCheering");
    
    [SerializeField] private float maxRandomDelay = 0.5f;
    [SerializeField] private float minRandomDelay = 0.0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        // Save starting position to prevent drift
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        
        // Random animation start offset so they're not all synchronized
        if (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(state.fullPathHash, 0, Random.Range(0f, 1f));
        }
    }

    private void LateUpdate()
    {
        // Keep character in place (prevents animation drift)
        transform.localPosition = startPosition;
        transform.localRotation = startRotation;
    }

    public void SetCheering(bool cheering)
    {
        if (animator == null) return;
        
        // Add random delay for more natural crowd behavior
        float delay = Random.Range(minRandomDelay, maxRandomDelay);
        StartCoroutine(SetCheeringDelayed(cheering, delay));
    }

    private IEnumerator SetCheeringDelayed(bool cheering, float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetBool(IsCheering, cheering);
    }
}
