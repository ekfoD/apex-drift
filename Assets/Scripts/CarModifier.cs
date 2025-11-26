using UnityEngine;

public class CarModifier : MonoBehaviour
{
    [System.Serializable]
    public class Modification
    {
        public string name;
        [Header("Stats")]
        [Range(0.5f, 2f)] public float speedMultiplier = 1f;
        [Range(0.5f, 2f)] public float accelerationMultiplier = 1f;
        [Range(0.5f, 2f)] public float weightMultiplier = 1f;
        [Range(0.5f, 2f)] public float handlingMultiplier = 1f;
        [Range(0.5f, 2f)] public float driftinessMultiplier = 1f;
        [Range(0.5f, 2f)] public float brakeMultiplier = 1f;
    }
    
    [Header("Available Modifications")]
    public Modification[] modifications = new Modification[]
    {
        // Engine Upgrade: Power + Weight, Less Stable
        new Modification 
        { 
            name = "Engine Upgrade",
            speedMultiplier = 1.3f,           // +30% top speed
            accelerationMultiplier = 1.25f,   // +25% acceleration
            weightMultiplier = 1.15f,         // +15% weight (heavier engine)
            handlingMultiplier = 0.95f,       // -5% handling (harder to turn)
            driftinessMultiplier = 1.1f,      // Slightly more slidey
            brakeMultiplier = 0.95f           // -5% braking (more weight to stop)
        },
        
        // Grip Tires: Less Drift + Braking, Slight Speed Loss
        new Modification 
        { 
            name = "Grip Tires",
            speedMultiplier = 0.95f,          // -5% top speed (rolling resistance)
            accelerationMultiplier = 1.05f,   // +5% acceleration (better traction)
            weightMultiplier = 1.0f,          // No weight change
            handlingMultiplier = 1.15f,       // +15% handling (better cornering)
            driftinessMultiplier = 0.7f,      // Much less drifty
            brakeMultiplier = 1.25f           // +25% braking (more grip)
        },
        
        // Weight Reduction: Agile, Slidey, Fragile Feel
        new Modification 
        { 
            name = "Weight Reduction",
            speedMultiplier = 1.1f,           // +10% top speed (less to push)
            accelerationMultiplier = 1.3f,    // +30% acceleration (light = fast)
            weightMultiplier = 0.75f,         // -25% weight
            handlingMultiplier = 1.2f,        // +20% handling (nimble)
            driftinessMultiplier = 1.25f,     // More drifty (light rear end)
            brakeMultiplier = 1.1f            // +10% braking effectiveness
        }
    };
    
    void Start()
    {
        ApplyModification();
    }
    
    void ApplyModification()
    {
        int modIndex = PlayerPrefs.GetInt("SelectedModificationIndex", -1);
        
        if (modIndex < 0 || modIndex >= modifications.Length)
        {
            Debug.LogWarning("No valid modification selected");
            return;
        }
        
        Modification mod = modifications[modIndex];
        Debug.Log($"Applying modification: {mod.name}");
        
        // Get car controller
        CarController carController = GetComponent<CarController>();
        if (carController == null)
        {
            Debug.LogError("CarController not found!");
            return;
        }
        
        // Apply multipliers to car stats
        carController.maxSpeed = Mathf.RoundToInt(carController.maxSpeed * mod.speedMultiplier);
        carController.maxReverseSpeed = Mathf.RoundToInt(carController.maxReverseSpeed * mod.speedMultiplier);
        carController.accelerationMultiplier = Mathf.RoundToInt(carController.accelerationMultiplier * mod.accelerationMultiplier);
        carController.maxSteeringAngle = Mathf.RoundToInt(carController.maxSteeringAngle * mod.handlingMultiplier);
        carController.brakeForce = Mathf.RoundToInt(carController.brakeForce * mod.brakeMultiplier);
        
        // Apply driftiness
        if (carController.GetType().GetField("driftiness") != null)
        {
            carController.driftiness = carController.driftiness * mod.driftinessMultiplier;
        }
        
        // Apply weight
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = rb.mass * mod.weightMultiplier;
        }
        
        Debug.Log($"Modified stats:");
        Debug.Log($"  Speed: {carController.maxSpeed}");
        Debug.Log($"  Acceleration: {carController.accelerationMultiplier}");
        Debug.Log($"  Handling: {carController.maxSteeringAngle}");
        Debug.Log($"  Weight: {rb?.mass}");
        Debug.Log($"  Braking: {carController.brakeForce}");
    }
}
