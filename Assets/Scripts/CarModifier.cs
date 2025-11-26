using UnityEngine;

public class CarModifier : MonoBehaviour
{
    [System.Serializable]
    public class Modification
    {
        public string name;
        [Range(0.5f, 2f)] public float speedMultiplier = 1f;
        [Range(0.5f, 2f)] public float accelerationMultiplier = 1f;
        [Range(0.5f, 2f)] public float weightMultiplier = 1f;
        [Range(0.5f, 2f)] public float handlingMultiplier = 1f;
    }
    
    [Header("Available Modifications")]
    public Modification[] modifications = new Modification[]
    {
        new Modification { name = "Engine", speedMultiplier = 1.3f, accelerationMultiplier = 1.1f, weightMultiplier = 0.9f, handlingMultiplier = 0.95f },
        new Modification { name = "Tires", speedMultiplier = 1.1f, accelerationMultiplier = 1.4f, weightMultiplier = 1.0f, handlingMultiplier = 1.05f },
        new Modification { name = "Weight", speedMultiplier = 1.15f, accelerationMultiplier = 1.15f, weightMultiplier = 0.95f, handlingMultiplier = 1.1f }
    };
    
    void Start()
    {
        ApplyModification();
    }
    
    void ApplyModification()
    {
        // Get selected modification
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
        
        // Apply weight
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = rb.mass * mod.weightMultiplier;
        }
        
        Debug.Log($"Modified stats - Speed: {carController.maxSpeed}, Accel: {carController.accelerationMultiplier}, Weight: {rb?.mass}");
    }
}
