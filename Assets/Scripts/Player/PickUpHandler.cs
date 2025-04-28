using UnityEngine;

public class PickUpHandler : MonoBehaviour
{
    public GameManager timer;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            if (timer != null)
            {
                timer.AddTime(30f);
            }
            else
            {
                Debug.LogWarning("Timer reference is missing on " + gameObject.name);
            }

            Destroy(other.gameObject);
        }
    }
}