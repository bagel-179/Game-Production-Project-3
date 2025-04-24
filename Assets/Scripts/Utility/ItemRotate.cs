using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRotate : MonoBehaviour
{
    private float rotationSpeed = 20f;
    private float moveSpeed = 0.75f;
    private float moveDistance = 0.25f;

    private float initialY;

    void Start()
    {
        initialY = transform.position.y;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        float newY = initialY + Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
