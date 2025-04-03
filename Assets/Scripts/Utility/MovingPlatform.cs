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
    public float fallSpeed = 2f;
    
    [Header("Special Features")]
    public bool canShatter = false;
    public bool isFrozen = false;

    private int targetIndex = 0;
    private int direction = 1;
    private bool isWaiting = false;
    private bool isFalling = false;
    private float fallTimer = 0;
    
    public bool PlayerOnPlatform { get; private set; }

    void Update()
    {
        if (canShatter && PlayerOnPlatform && !isFrozen)
        {
            EnableShatterScript();
        }
    }

    void FixedUpdate()
    {
        if (isFalling)
        {
            HandleFalling();
            return;
        }

        if (isFrozen || isWaiting || points.Length < 2 || platform == null)
            return;

        MovePlatform();
    }

    void HandleFalling()
    {
        if (fallTimer > 0)
        {
            fallTimer -= Time.fixedDeltaTime; 
        }
        else if (PlayerOnPlatform)
        {
            platform.transform.position += Vector3.down * fallSpeed * Time.fixedDeltaTime; 
        }
    }

    void MovePlatform()
    {
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

    public void StopFalling() => isFalling = false;

    public void TriggerFall()
    {
        if (canFall && !isFalling)
        {
            fallTimer = fallDelay; 
            isFalling = true;
        }
    }

    private void EnableShatterScript()
    {
        Shatter shatterScript = GetComponentInChildren<Shatter>();
        if (shatterScript != null)
        {
            shatterScript.EnableShattering();
        }
    }
}