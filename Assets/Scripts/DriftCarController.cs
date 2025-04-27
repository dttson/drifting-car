// DriftCarController.cs

using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DriftCarController : MonoBehaviour
{
    public bool IsDrifting => isDrifting;
    
    [Header("Movement")]
    public float acceleration = 10f;
    public float turnSpeed = 100f;
    public float driftFactor = 0.95f;
    public float maxSpeed = 20f;

    [Header("Grounding")]
    public float groundOffset = 0.5f;
    public float groundRayLength = 1.5f;
    public LayerMask groundLayer;
    public float groundSnapSpeed = 10f;
    public float maxSlopeAngle = 30f; // Only snap on gentle slopes

    [Header("AudioSource")] 
    public AudioSource sfxCarIdle;
    public AudioSource sfxCarEngine;
    public AudioSource sfxCarDrift;
    public float driftThreshold = 0.5f;  // Sideways velocity threshold to trigger drift sound
    public float minEnginePitch = 1f;
    public float maxEnginePitch = 3f;
    public float minEngineVolume = 0.3f;
    public float maxEngineVolume = 1f;

    private Rigidbody rb;
    private bool isDrifting = false;
    private bool isActivate = false;

    public void Activate()
    {
        isActivate = true;
        sfxCarIdle.Stop();
        sfxCarEngine.Play();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.centerOfMass = new Vector3(0, -1f, 0);
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;

        sfxCarDrift.loop = false;
        sfxCarIdle.loop = sfxCarEngine.loop = true;
        
        sfxCarIdle.Play();
    }

    void FixedUpdate()
    {
        if (!isActivate)
            return;
        
        HandleMovement();
        SnapToGround();
        PreventCollisionRotation();
        UpdateEngineAudio();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        Vector3 forwardForce = transform.forward * moveInput * acceleration;
        rb.AddForce(forwardForce, ForceMode.Acceleration);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / maxSpeed);
        float rotationY = turnInput * turnSpeed * speedFactor * Time.fixedDeltaTime;
        if (moveInput < 0)
            rotationY *= -1;
        
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotationY, 0));

        Vector3 vel = rb.velocity;
        float forwardVel = Vector3.Dot(vel, transform.forward);
        float sidewaysVel = Vector3.Dot(vel, transform.right);
        rb.velocity = transform.forward * forwardVel + transform.right * sidewaysVel * driftFactor;
        
        // Drift audio logic
        bool drifting = Mathf.Abs(sidewaysVel) > driftThreshold && Mathf.Abs(moveInput) > 0.1f;
        if (drifting != isDrifting)
        {
            isDrifting = drifting;
            if (isDrifting)
                sfxCarDrift.Play();
            else
                sfxCarDrift.Stop();
        }
    }

    void SnapToGround()
    {
        Vector3 rayOrigin = rb.position + Vector3.up * groundOffset;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundRayLength, groundLayer))
        {
            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope <= maxSlopeAngle)
            {
                Vector3 targetPos = hit.point + Vector3.up * groundOffset;
                Vector3 newPos = Vector3.Lerp(rb.position, targetPos, groundSnapSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);

                Vector3 forwardDir = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
                Quaternion targetRot = Quaternion.LookRotation(forwardDir, hit.normal);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, groundSnapSpeed * Time.fixedDeltaTime));
            }
        }
    }

    void PreventCollisionRotation()
    {
        // Zero out angular velocity to stop physics-driven rotation from collisions
        rb.angularVelocity = Vector3.zero;
    }
    
    void UpdateEngineAudio()
    {
        if (sfxCarEngine == null) 
            return;
        
        float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / maxSpeed);
        sfxCarEngine.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, speedFactor);
        sfxCarEngine.volume = Mathf.Lerp(minEngineVolume, maxEngineVolume, speedFactor);
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log($"==== Collision Enter {other.gameObject}");
        if (other.gameObject.CompareTag("Road") || other.gameObject.CompareTag("Terrain"))
            return;
        
        SoundManager.Instance.PlaySFX(SoundEffect.CarHitObject);
    }
}
