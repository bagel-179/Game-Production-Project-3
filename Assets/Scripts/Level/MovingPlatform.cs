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
    
    [Header("Fall Settings")]
    public bool canFall = false;
    public float fallDelay = 2f;
    public float verticalSpeed = 5f;
    
    [Header("Special Features")]
    public bool canShatter = false;
    public bool isFrozen = false;

    private int targetIndex = 0;
    private int direction = 1;
    private bool isWaiting = false;
    private Rigidbody platformRb;

    public bool PlayerOnPlatform { get; private set; }

    void Start()
    {
        platformRb = platform.GetComponent<Rigidbody>();
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
        if (platformRb != null)
        {
            platformRb.linearVelocity = Vector3.zero; 
        }

        Transform targetPoint = points[targetIndex];
        platform.transform.position = Vector3.MoveTowards(
            platform.transform.position, 
            targetPoint.position, 
            speed * Time.deltaTime);

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


    private void EnableShatterScript()
    {
        Shatter shatterScript = GetComponentInChildren<Shatter>();
        if (shatterScript != null)
        {
            shatterScript.EnableShattering();
        }
    }
}