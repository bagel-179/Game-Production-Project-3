using UnityEngine;
using System.Collections;

public class FallReset : MonoBehaviour
{
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Transform root = other.transform.root;
            root.position = spawnPoint.position;
            root.rotation = spawnPoint.rotation;
        }
    }
}
