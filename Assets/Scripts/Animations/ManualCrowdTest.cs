using UnityEngine;

public class ManualCrowdTest : MonoBehaviour
{
    private CrowdCharacter[] characters;
    
    void Start()
    {
        characters = GetComponentsInChildren<CrowdCharacter>();
        Debug.Log($"Found {characters.Length} characters for manual test");
    }
    
    void Update()
    {
        // Press SPACE to make them cheer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("🎉 SPACE pressed - Setting cheer to TRUE");
            foreach (var c in characters)
            {
                c.SetCheering(true);
            }
        }
        
        // Press C to make them stop
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("😐 C pressed - Setting cheer to FALSE");
            foreach (var c in characters)
            {
                c.SetCheering(false);
            }
        }
    }
}
