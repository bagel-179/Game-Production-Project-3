using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour, IFreezeable
{
    [Header("Movement Settings")]
    public GameObject platform;
    public Transform[] points;
    public float speed = 2f;
    public bool isALoop = false;
    public int[] stopPoints;
    public float stopDuration = 2f;

    [Header("Special Features")]
    public bool canShatter = false;
    public bool isFrozen = false;

    private int targetIndex = 0;
    private int direction = 1;
    private bool isWaiting = false;
    private Rigidbody platformRb;
    private Vector3 initialPosition;

    public bool PlayerOnPlatform { get; private set; }

    void Start()
    {
        platformRb = platform.GetComponent<Rigidbody>();
        initialPosition = platform.transform.position;

        if (platformRb != null)
        {
            platformRb.isKinematic = true;
            platformRb.interpolation = RigidbodyInterpolation.Interpolate; // For smoother movement
        }
    }

    void Update()
    {
        if (canShatter && PlayerOnPlatform && !isFrozen)
        {
            EnableShatterScript();
        }
    }

    void FixedUpdate()
    {
        if (platform == null) return;

        if (isFrozen || isWaiting || points.Length < 2)
            return;

        MovePlatform();
    }

    void MovePlatform()
    {
        Transform targetPoint = points[targetIndex];
        Vector3 newPosition = Vector3.MoveTowards(
            platform.transform.position,
            targetPoint.position,
            speed * Time.deltaTime);

        // If using Rigidbody, move it properly
        if (platformRb != null)
        {
            platformRb.MovePosition(newPosition);
        }
        else
        {
            platform.transform.position = newPosition;
        }

        // Check if we've reached the target point
        if (Vector3.Distance(platform.transform.position, targetPoint.position) < 0.01f)
        {
            if (ShouldStopAtPoint(targetIndex))
            {
                StartCoroutine(WaitAtPoint());
            }
            else
            {
                SetNextTarget();
            }
        }
    }

    bool ShouldStopAtPoint(int index) => System.Array.Exists(stopPoints, point => point == index);

    IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(stopDuration);
        isWaiting = false;
        SetNextTarget();
    }

    void SetNextTarget()
    {
        targetIndex += direction;

        if (targetIndex >= points.Length)
        {
            if (isALoop)
            {
                targetIndex = 0;
            }
            else
            {
                targetIndex = points.Length - 2;
                direction = -1;
            }
        }
        else if (targetIndex < 0)
        {
            targetIndex = 1;
            direction = 1;
        }
    }

    public void ResetPlatform()
    {
        StopAllCoroutines();
        isWaiting = false;

        if (platformRb != null)
        {
            platformRb.linearVelocity = Vector3.zero;
            platformRb.angularVelocity = Vector3.zero;
        }

        platform.transform.position = initialPosition;
        targetIndex = 0;
        direction = 1;
    }

    public void SetPlayerOnPlatform(bool onPlatform) => PlayerOnPlatform = onPlatform;

    public void SetFrozen(bool frozen) => isFrozen = frozen;

    private void EnableShatterScript()
    {
        Shatter shatterScript = GetComponentInChildren<Shatter>();
        if (shatterScript != null)
        {
            shatterScript.EnableShattering();
        }
    }
}