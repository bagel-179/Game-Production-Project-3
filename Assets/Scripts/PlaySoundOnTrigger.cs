using UnityEngine;

public class PlaySoundOnTrigger : MonoBehaviour
{
    public AudioClip pickupSound;
    public float volume = 1f;
    private bool hasPlayedSound = false; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup") && !hasPlayedSound) 
        {

            AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
            hasPlayedSound = true;

            Destroy(other.gameObject); 
        }
    }
}