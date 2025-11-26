using UnityEngine;

public class CrowdTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    private CrowdCharacter[] crowdCharacters;

    private void Awake()
    {
        // Look at parent (Crowd) and get all children
        Transform parent = transform.parent;
        crowdCharacters = parent.GetComponentsInChildren<CrowdCharacter>();
    }

    private void OnTriggerEnter(Collider other)
    {   
        if (other.CompareTag(playerTag))
        {
            SetCrowdCheering(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            SetCrowdCheering(false);
        }
    }

    private void SetCrowdCheering(bool cheering)
    {
        foreach (var character in crowdCharacters)
        {
            character.SetCheering(cheering);
        }
    }
}
