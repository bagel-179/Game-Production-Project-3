using UnityEngine;

public class PlayerOnMovingObject : MonoBehaviour
{
    private Rigidbody platformRigidbody;
    private Vector3 previousPlatformPosition;
    private MovingPlatform currentPlatform;
    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (platformRigidbody == null) return;

        Vector3 platformMovement = platformRigidbody.position - previousPlatformPosition;
        playerRigidbody.position += platformMovement;
        previousPlatformPosition = platformRigidbody.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Platform")) return;

        platformRigidbody = collision.gameObject.GetComponent<Rigidbody>();
        currentPlatform = collision.transform.parent?.GetComponent<MovingPlatform>();

        if (platformRigidbody != null)
        {
            previousPlatformPosition = platformRigidbody.position;
        }

        if (currentPlatform != null)
        {
            currentPlatform.SetPlayerOnPlatform(true);
            currentPlatform.TriggerFall();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Platform")) return;

        if (currentPlatform != null)
        {
            currentPlatform.SetPlayerOnPlatform(false);
            currentPlatform.StopFalling();
        }

        platformRigidbody = null;
        currentPlatform = null;
    }
}