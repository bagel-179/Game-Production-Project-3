using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Axis to rotate around (1 = rotate, 0 = don't)")]
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 20f;

    [Header("Bobbing Settings")]
    public bool enableBobbing = true;
    public float bobSpeed = 0.75f;
    public float bobDistance = 0.25f;

    [Tooltip("Apply random offset to the bobbing")]
    public bool useRandomPhaseOffset = true;

    private float initialY;
    private float phaseOffset;

    void Start()
    {
        initialY = transform.position.y;
        phaseOffset = useRandomPhaseOffset ? Random.Range(0f, 2f * Mathf.PI) : 0f;
    }

    void Update()
    {
        transform.Rotate(rotationAxis.normalized, rotationSpeed * Time.deltaTime);

        if (enableBobbing)
        {
            float newY = initialY + Mathf.Sin(Time.time * bobSpeed + phaseOffset) * bobDistance;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}