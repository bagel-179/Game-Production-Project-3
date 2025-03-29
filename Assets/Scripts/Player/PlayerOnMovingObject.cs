using UnityEngine;

public class PlayerOnMovingObject : MonoBehaviour
{
    private Rigidbody platformRigidbody;
    private Vector3 previousPlatformPosition;
    private Collider platformCollider; 
    private bool isOnPlatform = false;

    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();        
    }

    private void FixedUpdate()
    {
        if (isOnPlatform && platformRigidbody != null)
        {
            Vector3 platformMovement = platformRigidbody.position - previousPlatformPosition;

            playerRigidbody.position += platformMovement;
        }

        if (platformRigidbody != null)
        {
            previousPlatformPosition = platformRigidbody.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            Rigidbody detectedRigidbody = collision.gameObject.GetComponent<Rigidbody>();

            platformRigidbody = detectedRigidbody;
            isOnPlatform = true;
            previousPlatformPosition = platformRigidbody.position;
        }
            
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;
            platformRigidbody = null;
        }
    }
}
