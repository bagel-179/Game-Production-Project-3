using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.Splines;

[RequireComponent(typeof(PlayerMovement))]
public class SplineGrinder : MonoBehaviour
{
    [Header("Grinding Settings")]
    [SerializeField] private Transform grindAnchorPoint;
    [SerializeField] private float grindSpeed = 15f;
    [SerializeField] private float rotationLerpSpeed = 10f;
    [SerializeField] private float alignmentSpeed = 5f;
    [SerializeField] private float attachRadius = 0.5f;
    [SerializeField] private LayerMask splineLayer;
    [SerializeField] private float grindCooldown = 0.5f;

    [Header("Launch Settings")]
    [SerializeField] private float endLaunchForce = 10f;
    [SerializeField] private float upwardLaunchRatio = 0.3f; // 30% upward force
    [SerializeField] private float jumpLaunchForce = 8f;
    [SerializeField] private float jumpUpwardRatio = 0.5f; // 50% upward force when jumping

    private SplineContainer currentSpline;
    private float currentDistance = 0f;
    private bool isGrinding = false;
    private bool isAligning = false;
    private Rigidbody rb;
    private PlayerMovement playerMovement;
    private Vector3 grindOffset;
    private float lastGrindEndTime;
    private bool canGrind => Time.time > lastGrindEndTime + grindCooldown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        lastGrindEndTime = -grindCooldown;

        if (grindAnchorPoint == null)
        {
            grindAnchorPoint = transform;
        }
    }

    private void Update()
    {
        if (!isGrinding && !isAligning && canGrind)
        {
            CheckForSplineCollision();
        }

        // NEW: Direct jump input check
        if (isGrinding && Input.GetKeyDown(KeyCode.Space))
        {
            JumpOffGrind();
        }
    }

    private void FixedUpdate()
    {
        if (isGrinding)
        {
            MoveAlongSpline();
        }
    }

    private void JumpOffGrind()
    {
        // 1. First enable physics
        rb.isKinematic = false;
        playerMovement.SetMovementLock(false);

        // 2. Calculate jump direction (forward + upward)
        Vector3 jumpDirection = (transform.forward + Vector3.up * jumpUpwardRatio).normalized;

        // 3. Apply force
        rb.AddForce(jumpDirection * jumpLaunchForce, ForceMode.VelocityChange);

        // 4. Update states
        isGrinding = false;
        currentSpline = null;
        lastGrindEndTime = Time.time;

        // 5. Set jump count (prevents double jump)
        playerMovement.jumpCount = 1;

        Debug.DrawRay(transform.position, jumpDirection * 5f, Color.green, 2f);
    }

    private void CheckForSplineCollision()
    {
        Collider[] hits = Physics.OverlapSphere(grindAnchorPoint.position, attachRadius, splineLayer);

        foreach (var hit in hits)
        {
            SplineContainer spline = hit.GetComponent<SplineContainer>();
            if (spline != null)
            {
                StartGrinding(spline);
                break;
            }
        }
    }

    private void StartGrinding(SplineContainer spline)
    {
        playerMovement.IsGrinding = true; // <-- Add this
        playerMovement.SetMovementLock(true);

        currentSpline = spline;
        isAligning = true;

        grindOffset = grindAnchorPoint.position - transform.position;

        SplineUtility.GetNearestPoint(
            spline.Spline,
            grindAnchorPoint.position,
            out _,
            out currentDistance
        );

        StartCoroutine(AlignToRail());
        rb.isKinematic = true;
    }

    private IEnumerator AlignToRail()
    {
        float t = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float3 splinePos = currentSpline.EvaluatePosition(currentDistance);
        float3 tangent = currentSpline.EvaluateTangent(currentDistance);
        float3 upVector = currentSpline.EvaluateUpVector(currentDistance);

        Vector3 targetPosition = (Vector3)splinePos - grindOffset;
        Quaternion targetRotation = Quaternion.LookRotation(
            ((Vector3)tangent).normalized,
            ((Vector3)upVector).normalized
        );

        while (t < 1f)
        {
            t += Time.deltaTime * alignmentSpeed;
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);
            yield return null;
        }

        isAligning = false;
        isGrinding = true;
    }

    private void MoveAlongSpline()
    {
        currentDistance += grindSpeed * Time.fixedDeltaTime / currentSpline.CalculateLength();

        if (currentDistance >= 1f)
        {
            EndGrindWithLaunch();
            return;
        }

        float3 splinePos = currentSpline.EvaluatePosition(currentDistance);
        float3 tangent = currentSpline.EvaluateTangent(currentDistance);
        float3 upVector = currentSpline.EvaluateUpVector(currentDistance);

        transform.position = (Vector3)splinePos - grindOffset;

        if (!tangent.Equals(float3.zero))
        {
            Quaternion targetRotation = Quaternion.LookRotation(
                ((Vector3)tangent).normalized,
                ((Vector3)upVector).normalized
            );
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationLerpSpeed * Time.fixedDeltaTime
            );
        }
    }

    private void EndGrindWithLaunch()
    {
        // Get the spline's forward direction at the end point
        float3 endTangent = currentSpline.EvaluateTangent(1f);
        Vector3 forwardDirection = ((Vector3)endTangent).normalized;

        // Combine with upward force
        Vector3 launchDirection = (forwardDirection + Vector3.up * upwardLaunchRatio).normalized;

        EndGrind(true);
        rb.AddForce(launchDirection * endLaunchForce, ForceMode.VelocityChange);

        Debug.Log($"Launched from end with direction: {launchDirection}");
    }

    private void EndGrind(bool launchedFromEnd)
    {
        if (!isGrinding && !isAligning) return; // Already ended
        playerMovement.IsGrinding = false;

        // Reset all states
        isGrinding = false;
        isAligning = false;
        currentSpline = null;

        // Enable physics
        rb.isKinematic = false;
        playerMovement.SetMovementLock(false);

        // Set cooldown
        lastGrindEndTime = Time.time;

        Debug.Log(launchedFromEnd ? "Ended grind with launch" : "Ended grind from jump");
    }

    private void OnDrawGizmosSelected()
    {
        if (grindAnchorPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(grindAnchorPoint.position, attachRadius);
        }
    }
}