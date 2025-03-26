using UnityEngine;

public class PlayerOnMovingObject : MonoBehaviour
{
    public Rigidbody platformRigidbody;
    private Vector3 previousPlatformPosition;
    private Collider platformCollider;  // Reference to the platform's collider
    private bool isOnPlatform = false;

    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();

        platformCollider = platformRigidbody.GetComponent<Collider>(); // Get the platform's collider
        previousPlatformPosition = platformRigidbody.position;
    }

    private void Update()
    {
        if (isOnPlatform)
        {
            // Calculate the platform's movement
            Vector3 platformVelocity = platformRigidbody.position - previousPlatformPosition;

            // Apply the platform's velocity to the player's Rigidbody
            playerRigidbody.linearVelocity += platformVelocity;

            // Optional: Ensure the player stays in contact with the platform
            // You can use this if you want to prevent the player from floating off the platform
            Vector3 platformMovement = platformRigidbody.position - previousPlatformPosition;
            playerRigidbody.position += platformMovement;
        }

        previousPlatformPosition = platformRigidbody.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the player collided with something tagged "Platform"
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = true;  // Player is now on the platform
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Check if the player stopped colliding with the platform
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;  // Player is no longer on the platform
        }
    }
}
