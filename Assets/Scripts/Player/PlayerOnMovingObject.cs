using UnityEngine;

public class PlayerOnMovingObject : MonoBehaviour
{
    private Rigidbody platformRigidbody;
    private Vector3 previousPlatformPosition;
    private Collider platformCollider; 
    private bool isOnPlatform = false;

    private Rigidbody playerRigidbody;
    private MovingPlatform currentPlatformScript;

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
            platformRigidbody = collision.gameObject.GetComponent<Rigidbody>();

            currentPlatformScript = collision.transform.parent?.GetComponent<MovingPlatform>();

            isOnPlatform = true;
            if (platformRigidbody != null)
            {
                previousPlatformPosition = platformRigidbody.position;
            }

            if (currentPlatformScript != null)
            {
                currentPlatformScript.TriggerFall();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;
            platformRigidbody = null;
            currentPlatformScript = null;

            Debug.Log("Left platform");

            // Stop the platform from falling when the player leaves the platform
            if (currentPlatformScript != null)
            {
                currentPlatformScript.StopFalling();
            }
        }
    }
}