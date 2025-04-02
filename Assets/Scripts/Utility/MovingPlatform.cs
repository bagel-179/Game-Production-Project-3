using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour, IFreezeable
{
    public GameObject platform; 
    public Transform[] points;
    public float speed = 2f;
    public bool isALoop = false;
    public bool isFrozen = false;
    public int[] stopPoints;
    public float stopDuration = 2f;
    public bool canFall = false;
    public float fallDelay = 2f;
    public float fallSpeed = 2f;

    public bool canShatter = false;

    private int targetIndex = 0;
    private int direction = 1;
    private bool isWaiting = false;
    private bool isFalling = false;
    private float fallTimer = 0;

    void Update()
    {
        if (canShatter)
        {
            EnableShatterScript();
        }
    }

    void FixedUpdate()
    {
        if (!isFalling)
        {
            if (isFrozen || isWaiting || points.Length < 2 || platform == null)
                return;

            MovePlatform();
        }
        else
        {
            if (fallTimer > 0)
            {
                fallTimer -= Time.fixedDeltaTime; 
            }
            else
            {
                platform.transform.position += Vector3.down * fallSpeed * Time.fixedDeltaTime; 
            }
        }
        
    }

    void MovePlatform()
    {
        Transform targetPoint = points[targetIndex];
        platform.transform.position = Vector3.MoveTowards(platform.transform.position, targetPoint.position, speed * Time.deltaTime);

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

    bool ShouldStopAtPoint(int index)
    {
        foreach (int stopIndex in stopPoints)
        {
            if (stopIndex == index) return true;
        }
        return false;
    }

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
                targetIndex = 0;
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

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
    }

    public void TriggerFall()
    {
        if (canFall && !isFalling)
        {
            fallTimer = fallDelay; 
            isFalling = true;
        }
    }

    public void StopFalling()
    {
        isFalling = false; 
    }

    private void EnableShatterScript()
    {
        Shatter shatterScript = GetComponentInChildren<Shatter>();
        if (shatterScript != null)
        {
            Debug.Log("Shatter script found! Calling EnableShattering.");
            shatterScript.EnableShattering();  // Call the EnableShattering function on the Shatter script
        }
        else
        {
            Debug.LogWarning("Shatter script not found in children!");
        }
    }
}