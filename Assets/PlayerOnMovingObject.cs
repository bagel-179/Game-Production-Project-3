using UnityEngine;

public class PlayerOnMovingObject : MonoBehaviour
{
    public Rigidbody platformRigidbody;
    private Vector3 previousPlatformPosition;
    private Collider platformCollider; 
    private bool isOnPlatform = false;

    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        platformCollider = platformRigidbody.GetComponent<Collider>(); 

        previousPlatformPosition = platformRigidbody.position;
    }

    private void FixedUpdate()
    {
        if (isOnPlatform)
        {
            Vector3 platformVelocity = platformRigidbody.position - previousPlatformPosition;

            playerRigidbody.linearVelocity += platformVelocity;

            Vector3 platformMovement = platformRigidbody.position - previousPlatformPosition;
            playerRigidbody.position += platformMovement;
        }

        previousPlatformPosition = platformRigidbody.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = true; 
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false; 
        }
    }
}
