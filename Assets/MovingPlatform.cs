using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
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

    void Update()
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.collider.GetComponent<Rigidbody>();
            if (playerRb != null && playerRb.GetComponent<ConfigurableJoint>() == null)
            {
                ConfigurableJoint joint = playerRb.gameObject.AddComponent<ConfigurableJoint>();
                joint.connectedBody = GetComponent<Rigidbody>();
                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            ConfigurableJoint joint = collision.collider.GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                Destroy(joint);
            }
        }
    }
}