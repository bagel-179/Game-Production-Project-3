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
    [SerializeField] private float grindCooldown = 1;

    [Header("Launch Settings")]
    [SerializeField] private float endLaunchForce = 10f;
    [SerializeField] private float upwardLaunchRatio = 0.3f;
    [SerializeField] private float jumpLaunchForce = 8f;
    [SerializeField] private float jumpUpwardRatio = 0.5f; 

    private SplineContainer currentSpline;
    private float currentDistance = 0f;
    private bool isGrinding = false;
    private bool isAligning = false;
    private Rigidbody rb;
    private PlayerMovement playerMovement;
    private Vector3 grindOffset;
    private float lastGrindEndTime;
    private bool reachedEnd = false;
    private bool canGrind => Time.time > lastGrindEndTime + grindCooldown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
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
        Vector3 jumpDirection = (transform.forward + Vector3.up * jumpUpwardRatio).normalized;

        EndGrind(false);

        rb.AddForce(jumpDirection * jumpLaunchForce, ForceMode.VelocityChange);

        playerMovement.jumpCount = 0; 
        playerMovement.canGlide = true;
        playerMovement.SetMovementLock(false);

        Debug.DrawRay(transform.position, jumpDirection * 5f, Color.green, 2f);
    }

    private void CheckForSplineCollision()
    {
        if (Time.time < lastGrindEndTime) return; 

        Collider[] hits = Physics.OverlapSphere(grindAnchorPoint.position, attachRadius * 0.8f, splineLayer);

        foreach (var hit in hits)
        {
            SplineContainer spline = hit.GetComponent<SplineContainer>();
            if (spline != null && !IsNearLastKnot(spline))
            {
                reachedEnd = false; 
                StartGrinding(spline);
                break;
            }
        }
    }

    private bool IsNearLastKnot(SplineContainer spline)
    {
        float3 lastKnotPos = spline.EvaluatePosition(1f);
        return Vector3.Distance(grindAnchorPoint.position, lastKnotPos) < attachRadius * 1.5f;
    }

    private void StartGrinding(SplineContainer spline)
    {
        playerMovement.IsGrinding = true;
        playerMovement.SetMovementLock(true);

        currentSpline = spline;
        isAligning = true;

        grindOffset = grindAnchorPoint.position - transform.position;

        currentDistance = 0f;

        StartCoroutine(AlignToRail());
        rb.isKinematic = true;
    }

    private IEnumerator AlignToRail()
    {
        if (currentDistance >= 1f)
        {
            EndGrindWithLaunch();
            yield break;
        }

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
        if (reachedEnd) return; 

        currentDistance += grindSpeed * Time.fixedDeltaTime / currentSpline.CalculateLength();

        if (currentDistance >= 1f)
        {
            reachedEnd = true;
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
        float3 endPos = currentSpline.EvaluatePosition(1f);
        float3 beforeEndPos = currentSpline.EvaluatePosition(0.95f);
        Vector3 launchDirection = ((Vector3)(endPos - beforeEndPos) + Vector3.up * upwardLaunchRatio).normalized;

        EndGrind(true);

        rb.AddForce(launchDirection * endLaunchForce, ForceMode.VelocityChange);

        lastGrindEndTime = Time.time + grindCooldown;
    }


    private void EndGrind(bool launchedFromEnd)
    {
        if (!isGrinding && !isAligning) return;

        isGrinding = false;
        isAligning = false;
        currentSpline = null;

        rb.isKinematic = false;
        playerMovement.IsGrinding = false;
        playerMovement.SetMovementLock(false);

        if (!launchedFromEnd)
        {
            lastGrindEndTime = Time.time;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (grindAnchorPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(grindAnchorPoint.position, attachRadius);
        }

        if (currentSpline != null && isGrinding)
        {
            Gizmos.color = Color.red;
            float3 pos = currentSpline.EvaluatePosition(currentDistance);
            Gizmos.DrawSphere(pos, 0.2f);
            Gizmos.DrawLine(transform.position, pos);
        }
    }
}