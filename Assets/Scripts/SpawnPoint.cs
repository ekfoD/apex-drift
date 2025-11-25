using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Available Cars")]
    public GameObject[] carPrefabs;
    
    public GameObject SpawnCar(int carIndex, bool isNetworkSpawn = false, ulong clientId = 0)
    {
        if (carIndex >= carPrefabs.Length)
        {
            Debug.LogWarning($"Invalid car index {carIndex}, defaulting to 0");
            carIndex = 0;
        }
        
        GameObject carPrefab = carPrefabs[carIndex];
        GameObject spawnedCar = Instantiate(carPrefab, transform.position, transform.rotation);
        
        // No network spawning - purely local
        
        // Set spawn point for respawn system
        CheckpointRespawn respawn = spawnedCar.GetComponent<CheckpointRespawn>();
        if (respawn != null)
        {
            respawn.SetSpawnPoint(transform.position, transform.rotation);
        }
        
        Debug.Log($"Spawned LOCAL car: {carPrefab.name}");
        return spawnedCar;
    }
}
