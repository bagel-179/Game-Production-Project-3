using UnityEngine;

public class DecideEnd : MonoBehaviour
{

    public GameObject check;

    public GameObject ending;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(ending);
            Destroy(check);
        }
    }
}
