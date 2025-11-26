using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform carTransform;
    public float followSpeed = 2f;
    public float lookSpeed = 5f;

    [Header("Drift Settings")]
    public float driftAmount = 3f;
    public float recenterSpeed = 1f;

    [Header("Angle Constraints")]
    public float maxVerticalAngle = 45f;
    public float maxHorizontalAngle = 90f;

    [Header("Teleport Settings")]
    public float maxDistanceBeforeTeleport = 100f;
    public float defaultCameraDistance = 10f;
    public float defaultCameraHeight = 5f;

    private float initialCameraDistance;
    private float initialCameraHeight;

    private Vector3 previousCarPosition;
    private float currentHorizontalOffset = 0f;

    private bool isInitialized = false;
    private bool justTeleported = false;

    void Start()
    {
        if (carTransform == null)
            FindCar();
    }

    void FindCar()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            carTransform = p.transform;
            StartCoroutine(DelayedInitialize());
        }
    }

    IEnumerator DelayedInitialize()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        InitializeCamera();
    }

    void InitializeCamera()
    {
        if (carTransform == null) return;

        float dist = Vector3.Distance(transform.position, carTransform.position);

        if (dist > maxDistanceBeforeTeleport)
        {
            initialCameraDistance = defaultCameraDistance;
            initialCameraHeight = defaultCameraHeight;
            TeleportCamera();
        }
        else
        {
            Vector3 offset = transform.position - carTransform.position;
            initialCameraDistance = new Vector3(offset.x, 0f, offset.z).magnitude;
            initialCameraHeight = offset.y;
        }

        previousCarPosition = carTransform.position;
        currentHorizontalOffset = 0;
        isInitialized = true;
    }

    void TeleportCamera()
    {
        Vector3 forward = carTransform.forward;
        if (forward.sqrMagnitude < 0.01f)
            forward = Vector3.forward;

        Vector3 pos = carTransform.position - forward.normalized * initialCameraDistance;
        pos.y = carTransform.position.y + initialCameraHeight;

        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(carTransform.position - pos);

        // recalc offsets
        Vector3 off = transform.position - carTransform.position;
        initialCameraDistance = new Vector3(off.x, 0f, off.z).magnitude;
        initialCameraHeight = off.y;

        currentHorizontalOffset = 0f;
        previousCarPosition = carTransform.position;
        justTeleported = true;
    }

    void FixedUpdate()
    {
        if (carTransform == null)
        {
            FindCar();
            return;
        }

        if (!isInitialized)
        {
            InitializeCamera();
            return;
        }

        float dist = Vector3.Distance(transform.position, carTransform.position);

        // absolute safety clamp
        if (dist > maxDistanceBeforeTeleport * 2f)
        {
            TeleportCamera();
            return;
        }

        if (justTeleported)
        {
            justTeleported = false;
            previousCarPosition = carTransform.position;
            return;
        }

        Vector3 carMovement = carTransform.position - previousCarPosition;
        previousCarPosition = carTransform.position;

        float turnRate = 0f;
        if (carMovement.sqrMagnitude > 0.0001f)
            turnRate = Vector3.Dot(carTransform.right, carMovement.normalized);

        float desiredOffset = turnRate * driftAmount;
        desiredOffset = Mathf.Clamp(desiredOffset, -20f, 20f);  // <-- HARD LIMIT

        if (carMovement.magnitude > 0.1f)
            currentHorizontalOffset = Mathf.Lerp(currentHorizontalOffset, desiredOffset, followSpeed * Time.deltaTime);
        else
            currentHorizontalOffset = Mathf.Lerp(currentHorizontalOffset, 0f, recenterSpeed * Time.deltaTime);

        float maxOffset = initialCameraDistance * Mathf.Tan(maxHorizontalAngle * Mathf.Deg2Rad);
        currentHorizontalOffset = Mathf.Clamp(currentHorizontalOffset, -maxOffset, maxOffset);

        Vector3 forward = carTransform.forward;
        Vector3 right = carTransform.right;

        Vector3 targetPos = carTransform.position - forward * initialCameraDistance;
        targetPos += right * currentHorizontalOffset;
        targetPos.y = carTransform.position.y + initialCameraHeight;

        // validate target
        if (float.IsNaN(targetPos.x) || float.IsInfinity(targetPos.x))
            return;

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // HARD DISTANCE FIX
        float postDist = Vector3.Distance(transform.position, carTransform.position);
        if (postDist > maxDistanceBeforeTeleport)
        {
            transform.position = carTransform.position - forward * initialCameraDistance;
            transform.position += Vector3.up * initialCameraHeight;
        }

        Vector3 lookDir = carTransform.position - transform.position;
        float horiz = new Vector2(lookDir.x, lookDir.z).magnitude;
        float angle = Mathf.Atan2(lookDir.y, horiz) * Mathf.Rad2Deg;

        if (angle > maxVerticalAngle)
            lookDir.y = horiz * Mathf.Tan(maxVerticalAngle * Mathf.Deg2Rad);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(lookDir),
            lookSpeed * Time.deltaTime
        );
    }
}