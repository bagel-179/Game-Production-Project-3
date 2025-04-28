using UnityEngine;

public class PlaySoundOnTrigger : MonoBehaviour
{
    public GameObject pickupSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickupSound.SetActive(true);
        }
    }
}