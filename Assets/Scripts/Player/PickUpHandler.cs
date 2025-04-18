using UnityEngine;

public class PickUpHandler : MonoBehaviour
{
    public GameManager timer;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            timer.AddTime(30f);
            Destroy(other.gameObject);
        }
    }
}