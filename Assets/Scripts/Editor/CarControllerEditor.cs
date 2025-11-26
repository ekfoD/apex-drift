using System;
using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{
    //CAR SETUP
    [Header("CAR SETUP")]
    [Space(10)]
    [Range(20, 190)]
    public int maxSpeed = 90;
    [Range(10, 120)]
    public int maxReverseSpeed = 45;
    [Range(1, 10)]
    public int accelerationMultiplier = 2;
    [Space(10)]
    [Range(10, 45)]
    public int maxSteeringAngle = 27;
    [Range(0.1f, 1f)]
    public float steeringSpeed = 0.5f;
    [Space(10)]
    [Range(100, 600)]
    public int brakeForce = 350;
    [Range(1, 10)]
    public int decelerationMultiplier = 2;
    [Space(10)]
    public Vector3 bodyMassCenter;
    
    [Space(10)]
    [Header("Drift Settings")]
    [Range(0.5f, 3f)]
    public float driftiness = 1.5f;

    //WHEELS
    [Header("WHEELS")]
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    [Space(10)]
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    [Space(10)]
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    [Space(10)]
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    //PARTICLE SYSTEMS
    [Header("EFFECTS")]
    [Space(10)]
    public bool useEffects = false;
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    [Space(10)]
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    //SPEED TEXT (UI)
    [Header("UI")]
    [Space(10)]
    public bool useUI = false;
    public TMP_Text carSpeedText;

    //SOUNDS
    [Header("SOUNDS")]
    [Space(10)]
    public bool useSounds = false;
    public AudioSource carEngineSound;
    public AudioSource tireScreechSound;
    float initialCarEngineSoundPitch;

    //CAR DATA
    [HideInInspector]
    public float carSpeed;
    [HideInInspector]
    public bool isDrifting;

    //PRIVATE VARIABLES
    Rigidbody carRigidbody;
    float steeringAxis;
    float throttleAxis;
    float localVelocityZ;
    float localVelocityX;
    bool deceleratingCar;

    void Start()
    {
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        // Apply drift settings
        ApplyDriftSettings();

        // Save initial engine sound pitch
        if(useSounds && carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
        }

        // Setup UI
        if(useUI)
        {
            InvokeRepeating("CarSpeedUI", 0f, 0.1f);
        }
        else if(carSpeedText != null)
        {
            carSpeedText.text = "0";
        }

        // Setup sounds
        if(useSounds)
        {
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else
        {
            if(carEngineSound != null) carEngineSound.Stop();
            if(tireScreechSound != null) tireScreechSound.Stop();
        }

        // Setup effects
        if(!useEffects)
        {
            if(RLWParticleSystem != null) RLWParticleSystem.Stop();
            if(RRWParticleSystem != null) RRWParticleSystem.Stop();
            if(RLWTireSkid != null) RLWTireSkid.emitting = false;
            if(RRWTireSkid != null) RRWTireSkid.emitting = false;
        }
    }
    
    void ApplyDriftSettings()
    {
        // Make wheels more slippery based on driftiness value
        WheelFrictionCurve sidewaysFriction;
        
        // Front left
        sidewaysFriction = frontLeftCollider.sidewaysFriction;
        sidewaysFriction.extremumSlip = sidewaysFriction.extremumSlip * driftiness;
        sidewaysFriction.stiffness = sidewaysFriction.stiffness / driftiness;
        frontLeftCollider.sidewaysFriction = sidewaysFriction;
        
        // Front right
        sidewaysFriction = frontRightCollider.sidewaysFriction;
        sidewaysFriction.extremumSlip = sidewaysFriction.extremumSlip * driftiness;
        sidewaysFriction.stiffness = sidewaysFriction.stiffness / driftiness;
        frontRightCollider.sidewaysFriction = sidewaysFriction;
        
        // Rear left
        sidewaysFriction = rearLeftCollider.sidewaysFriction;
        sidewaysFriction.extremumSlip = sidewaysFriction.extremumSlip * driftiness;
        sidewaysFriction.stiffness = sidewaysFriction.stiffness / driftiness;
        rearLeftCollider.sidewaysFriction = sidewaysFriction;
        
        // Rear right
        sidewaysFriction = rearRightCollider.sidewaysFriction;
        sidewaysFriction.extremumSlip = sidewaysFriction.extremumSlip * driftiness;
        sidewaysFriction.stiffness = sidewaysFriction.stiffness / driftiness;
        rearRightCollider.sidewaysFriction = sidewaysFriction;
        
        Debug.Log($"Applied drift settings with driftiness: {driftiness}");
    }

    void Update()
    {
        //CAR DATA
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        //CAR PHYSICS - INPUT
        if(Input.GetKey(KeyCode.W))
        {
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            GoForward();
        }
        
        if(Input.GetKey(KeyCode.S))
        {
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            GoReverse();
        }

        if(Input.GetKey(KeyCode.A))
        {
            TurnLeft();
        }
        
        if(Input.GetKey(KeyCode.D))
        {
            TurnRight();
        }
        
        if(!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
        {
            ThrottleOff();
        }
        
        if(!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W) && !deceleratingCar)
        {
            InvokeRepeating("DecelerateCar", 0f, 0.1f);
            deceleratingCar = true;
        }
        
        if(!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && steeringAxis != 0f)
        {
            ResetSteeringAngle();
        }

        AnimateWheelMeshes();
    }

    public void CarSpeedUI()
    {
        if(useUI && carSpeedText != null)
        {
            try
            {
                float absoluteCarSpeed = Mathf.Abs(carSpeed);
                carSpeedText.text = Mathf.RoundToInt(absoluteCarSpeed).ToString();
            }
            catch(Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
    }

    public void CarSounds()
    {
        if(useSounds)
        {
            try
            {
                if(carEngineSound != null)
                {
                    float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.linearVelocity.magnitude) / 25f);
                    carEngineSound.pitch = engineSoundPitch;
                }
                
                if(tireScreechSound != null)
                {
                    if(isDrifting && Mathf.Abs(carSpeed) > 12f)
                    {
                        if(!tireScreechSound.isPlaying)
                        {
                            tireScreechSound.Play();
                        }
                    }
                    else
                    {
                        if(tireScreechSound.isPlaying)
                        {
                            tireScreechSound.Stop();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else
        {
            if(carEngineSound != null && carEngineSound.isPlaying) carEngineSound.Stop();
            if(tireScreechSound != null && tireScreechSound.isPlaying) tireScreechSound.Stop();
        }
    }

    //STEERING METHODS
    public void TurnLeft()
    {
        steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        if(steeringAxis < -1f)
        {
            steeringAxis = -1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void TurnRight()
    {
        steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        if(steeringAxis > 1f)
        {
            steeringAxis = 1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void ResetSteeringAngle()
    {
        if(steeringAxis < 0f)
        {
            steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        }
        else if(steeringAxis > 0f)
        {
            steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        }
        
        if(Mathf.Abs(frontLeftCollider.steerAngle) < 1f)
        {
            steeringAxis = 0f;
        }
        
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    void AnimateWheelMeshes()
    {
        try
        {
            Quaternion FLWRotation;
            Vector3 FLWPosition;
            frontLeftCollider.GetWorldPose(out FLWPosition, out FLWRotation);
            frontLeftMesh.transform.position = FLWPosition;
            frontLeftMesh.transform.rotation = FLWRotation;

            Quaternion FRWRotation;
            Vector3 FRWPosition;
            frontRightCollider.GetWorldPose(out FRWPosition, out FRWRotation);
            frontRightMesh.transform.position = FRWPosition;
            frontRightMesh.transform.rotation = FRWRotation;

            Quaternion RLWRotation;
            Vector3 RLWPosition;
            rearLeftCollider.GetWorldPose(out RLWPosition, out RLWRotation);
            rearLeftMesh.transform.position = RLWPosition;
            rearLeftMesh.transform.rotation = RLWRotation;

            Quaternion RRWRotation;
            Vector3 RRWPosition;
            rearRightCollider.GetWorldPose(out RRWPosition, out RRWRotation);
            rearRightMesh.transform.position = RRWPosition;
            rearRightMesh.transform.rotation = RRWRotation;
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    //ENGINE AND BRAKING METHODS
    public void GoForward()
    {
        if(Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        
        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if(throttleAxis > 1f)
        {
            throttleAxis = 1f;
        }
        
        if(localVelocityZ < -1f)
        {
            Brakes();
        }
        else
        {
            if(Mathf.RoundToInt(carSpeed) < maxSpeed)
            {
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void GoReverse()
    {
        if(Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        
        throttleAxis = throttleAxis - (Time.deltaTime * 3f);
        if(throttleAxis < -1f)
        {
            throttleAxis = -1f;
        }
        
        if(localVelocityZ > 1f)
        {
            Brakes();
        }
        else
        {
            if(Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
            {
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void ThrottleOff()
    {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    public void DecelerateCar()
    {
        if(Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        
        if(throttleAxis != 0f)
        {
            if(throttleAxis > 0f)
            {
                throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            }
            else if(throttleAxis < 0f)
            {
                throttleAxis = throttleAxis + (Time.deltaTime * 10f);
            }
            
            if(Mathf.Abs(throttleAxis) < 0.15f)
            {
                throttleAxis = 0f;
            }
        }
        
        carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * decelerationMultiplier)));
        
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
        
        if(carRigidbody.linearVelocity.magnitude < 0.25f)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    public void Brakes()
    {
        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;
    }

    public void DriftCarPS()
    {
        if(useEffects)
        {
            try
            {
                if(isDrifting)
                {
                    if(RLWParticleSystem != null) RLWParticleSystem.Play();
                    if(RRWParticleSystem != null) RRWParticleSystem.Play();
                }
                else
                {
                    if(RLWParticleSystem != null) RLWParticleSystem.Stop();
                    if(RRWParticleSystem != null) RRWParticleSystem.Stop();
                }
            }
            catch(Exception ex)
            {
                Debug.LogWarning(ex);
            }

            try
            {
                if(Mathf.Abs(localVelocityX) > 5f && Mathf.Abs(carSpeed) > 12f)
                {
                    if(RLWTireSkid != null) RLWTireSkid.emitting = true;
                    if(RRWTireSkid != null) RRWTireSkid.emitting = true;
                }
                else
                {
                    if(RLWTireSkid != null) RLWTireSkid.emitting = false;
                    if(RRWTireSkid != null) RRWTireSkid.emitting = false;
                }
            }
            catch(Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else
        {
            if(RLWParticleSystem != null) RLWParticleSystem.Stop();
            if(RRWParticleSystem != null) RRWParticleSystem.Stop();
            if(RLWTireSkid != null) RLWTireSkid.emitting = false;
            if(RRWTireSkid != null) RRWTireSkid.emitting = false;
        }
    }
}
