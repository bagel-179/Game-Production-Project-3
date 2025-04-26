using UnityEngine;

public class BookPickup : MonoBehaviour
{
    public GameObject visualObject; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Renderer renderer = visualObject.GetComponent<Renderer>();
            renderer.enabled = false;
        }
    }
}
