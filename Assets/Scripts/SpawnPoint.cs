using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject myCar;
    void Start()
    {
        myCar.transform.position = transform.position;
    }

}
