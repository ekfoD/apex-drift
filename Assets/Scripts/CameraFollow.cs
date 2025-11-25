using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform carTransform;

    [Range(1, 10)]
    public float followSpeed = 2f;

    [Range(1, 10)]
    public float lookSpeed = 5f;

    [Header("Drift Settings")]
    [Range(0, 10)]
    public float driftAmount = 3f;

    [Range(0, 5)]
    public float recenterSpeed = 1f;

    [Header("Angle Constraints")]
    [Range(0, 89)]
    public float maxVerticalAngle = 45f;

    [Range(0, 90)]
    public float maxHorizontalAngle = 90f;

    private Vector3 initialCameraPosition;
    private float initialCameraDistance;
    private float initialCameraHeight;

    private Vector3 previousCarPosition;
    private float currentHorizontalOffset = 0f;

    void Start()
    {
        initialCameraPosition = transform.position;

        // Calculate initial camera distance and height
        Vector3 offset = initialCameraPosition - carTransform.position;
        initialCameraDistance = new Vector2(offset.x, offset.z).magnitude;
        initialCameraHeight = offset.y;

        previousCarPosition = carTransform.position;
    }

    void FixedUpdate()
    {
        // Car movement
        Vector3 carMovement = carTransform.position - previousCarPosition;
        previousCarPosition = carTransform.position;

        // Calculate the turning rate
        float turnRate = Vector3.Dot(carTransform.right, carMovement.normalized);
        float desiredOffset = turnRate * driftAmount;

        // Control the horizontal offset to position the camera sideways
        if (Mathf.Abs(carMovement.magnitude) > 0.1f)
        {
            currentHorizontalOffset = Mathf.Lerp(currentHorizontalOffset, desiredOffset, followSpeed * Time.deltaTime);
        }
        else
        {
            // Smoothly recenter when the car is moving slowly
            currentHorizontalOffset = Mathf.Lerp(currentHorizontalOffset, 0f, recenterSpeed * Time.deltaTime);
        }

        // Clamp the horizontal offset
        float maxOffset = initialCameraDistance * Mathf.Tan(maxHorizontalAngle * Mathf.Deg2Rad);
        currentHorizontalOffset = Mathf.Clamp(currentHorizontalOffset, -maxOffset, maxOffset);

        // Calculate camera position based on the car position and offset
        Vector3 cameraPosition = carTransform.position - carTransform.forward * initialCameraDistance;
        cameraPosition += carTransform.right * currentHorizontalOffset;
        cameraPosition.y += initialCameraHeight;

        // Smoothly move the camera
        transform.position = Vector3.Lerp(transform.position, cameraPosition, followSpeed * Time.deltaTime);

        // Look at the car with clamped vertical angle
        Vector3 lookDirection = carTransform.position - transform.position;
        float horizontalDist = new Vector2(lookDirection.x, lookDirection.z).magnitude;
        float angle = Mathf.Atan2(lookDirection.y, horizontalDist) * Mathf.Rad2Deg;

        if (angle > maxVerticalAngle)
        {
            lookDirection.y = horizontalDist * Mathf.Tan(maxVerticalAngle * Mathf.Deg2Rad);
        }

        Quaternion rot = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, lookSpeed * Time.deltaTime);
    }
}