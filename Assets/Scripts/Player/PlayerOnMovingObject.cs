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
        if (collision.gameObject.CompareTag("Platform"))
        {
            currentPlatform = collision.transform.parent.GetComponent<MovingPlatform>();
            if (currentPlatform != null)
            {
                currentPlatform.SetPlayerOnPlatform(true);
                currentPlatform.TriggerFall(); 
            }
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