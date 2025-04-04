using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour, IFreezeable
{
    public Transform platform; 
    public Transform[] points;
    public float speed = 2f;
    public bool isALoop = false;
    public bool isFrozen = false;
    public int[] stopPoints;
    public float stopDuration = 2f;

    private int targetIndex = 0;
    private int direction = 1;
    private bool isWaiting = false;

    void FixedUpdate()
    {
        if (isFrozen || isWaiting || points.Length < 2 || platform == null) return;

        MovePlatform();
    }

    void MovePlatform()
    {
        Transform targetPoint = points[targetIndex];
        platform.position = Vector3.MoveTowards(platform.position, targetPoint.position, speed * Time.deltaTime);

        if (Vector3.Distance(platform.position, targetPoint.position) < 0.01f)
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

        if (isFrozen)
        {
            Debug.Log($"{gameObject.name} is frozen.");
        }
        else
        {
            Debug.Log($"{gameObject.name} is unfrozen.");
        }
    }
}