using UnityEngine;

public class PlayerOnMovingObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float raycastDistance = 0.2f;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private Vector3 raycastOffset = new Vector3(0, 0.1f, 0);

    private Rigidbody platformRigidbody;
    private Vector3 previousPlatformPosition;
    private Quaternion previousPlatformRotation;
    private MovingPlatform currentPlatform;
    private Rigidbody playerRigidbody;
    private bool isOnPlatform = false;
    private Transform platformTransform;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        CheckPlatformBelow();
        MoveWithPlatform();
    }

    private void CheckPlatformBelow()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + raycastOffset;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastDistance, platformLayer))
        {
            if (!isOnPlatform && hit.collider.CompareTag("Platform"))
            {
                AttachToPlatform(hit.collider);
            }
        }
        else if (isOnPlatform)
        {
            DetachFromPlatform();
        }
    }

    private void AttachToPlatform(Collider platformCollider)
    {
        currentPlatform = platformCollider.GetComponentInParent<MovingPlatform>();
        if (currentPlatform != null)
        {
            isOnPlatform = true;
            currentPlatform.SetPlayerOnPlatform(true);
            platformRigidbody = currentPlatform.GetPlatformRigidbody();
            platformTransform = currentPlatform.transform;

            if (platformRigidbody != null)
            {
                previousPlatformPosition = platformRigidbody.position;
                previousPlatformRotation = platformRigidbody.rotation;
            }
        }
    }

    private void DetachFromPlatform()
    {
        isOnPlatform = false;
        if (currentPlatform != null)
        {
            currentPlatform.SetPlayerOnPlatform(false);
            currentPlatform = null;
        }
        platformRigidbody = null;
        platformTransform = null;
    }

    private void MoveWithPlatform()
    {
        if (!isOnPlatform || platformRigidbody == null) return;

        Vector3 positionDelta = platformRigidbody.position - previousPlatformPosition;
        playerRigidbody.MovePosition(playerRigidbody.position + positionDelta);

        Quaternion rotationDelta = platformRigidbody.rotation * Quaternion.Inverse(previousPlatformRotation);
        Vector3 newPosition = rotationDelta * (playerRigidbody.position - platformRigidbody.position) + platformRigidbody.position;
        Quaternion newRotation = rotationDelta * playerRigidbody.rotation;

        playerRigidbody.MovePosition(newPosition);
        playerRigidbody.MoveRotation(newRotation);

        previousPlatformPosition = platformRigidbody.position;
        previousPlatformRotation = platformRigidbody.rotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 rayOrigin = transform.position + raycastOffset;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * raycastDistance);
    }
}