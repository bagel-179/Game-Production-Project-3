using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour, IFreezeable
{
    [Header("Movement")]
    public GameObject platform;
    public Transform[] points;
    public float speed = 2f;
    
    public bool isALoop = false;
    public int[] stopPoints;
    public float stopDuration = 2f;
    public bool isMovingVertical = false;
    
    [Header("Rotation")]
    public float rotationSpeed = 30f;
    public bool rotatePlatform = false;
    public Vector3 rotationAxis = Vector3.up;

    [Header("Special Features")]
    public bool canShatter = false;
    public bool isFrozen = false;

    private int targetIndex = 0;
    private int direction = 1;
    private bool isWaiting = false;
    private Rigidbody platformRb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public bool PlayerOnPlatform { get; private set; }

    void Start()
    {
        platformRb = platform.GetComponent<Rigidbody>();
        platformRb.isKinematic = true;
        originalPosition = platform.transform.position;
        originalRotation = platform.transform.rotation;
    }

    void FixedUpdate()
    {
        if (canShatter && PlayerOnPlatform && !isFrozen)
        {
            EnableShatterScript();
        }

        if (platform == null || isFrozen || isWaiting || points.Length < 2)
            return;

        MovePlatform();
    }

    void MovePlatform()
    {
        Transform targetPoint = points[targetIndex];
        Vector3 targetPosition = isMovingVertical ? new Vector3(originalPosition.x, targetPoint.position.y, originalPosition.z) : targetPoint.position;

        platform.transform.position = Vector3.MoveTowards(platform.transform.position, targetPosition, speed * Time.deltaTime);

        if (rotatePlatform)
        {
            platform.transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
        }

        if (Vector3.Distance(platform.transform.position, targetPosition) < 0.01f)
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
            targetIndex = isALoop ? 0 : points.Length - 2;
            direction = isALoop ? direction : -1;
        }
        else if (targetIndex < 0)
        {
            targetIndex = 1;
            direction = 1;
        }
    }

    public void SetPlayerOnPlatform(bool onPlatform) => PlayerOnPlatform = onPlatform;

    public void SetFrozen(bool frozen) => isFrozen = frozen;

    public Rigidbody GetPlatformRigidbody() => platformRb;

    private void EnableShatterScript()
    {
        Shatter shatterScript = GetComponentInChildren<Shatter>();
        if (shatterScript != null)
        {
            shatterScript.EnableShattering();
        }
    }
}