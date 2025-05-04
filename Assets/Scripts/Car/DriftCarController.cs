// DriftCarController.cs

using System;
using DefaultNamespace;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DriftCarController : BaseCarController
{
    public override string CarName => "YOU";
    public override bool IsMyCar => true;

    [Header("Movement")]
    public float acceleration = 10f;
    public float turnSpeed = 100f;
    public float driftFactor = 0.95f;
    public float maxSpeed = 20f;
    
    [Header("Input Smoothing")]
    [Tooltip("Time in seconds to smooth input changes")]
    public float inputSmoothTime = 0.1f;

    [Header("Grounding")]
    [LunaPlaygroundField] public float groundOffset = 0.5f;
    [LunaPlaygroundField] public float groundRayLength = 1.5f;
    [LunaPlaygroundField] public float groundSnapSpeed = 10f;
    
    public LayerMask groundLayer;
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

    [Header("VFX")] 
    public ParticleSystem smokeParticle;
    
    [Header("Input Modes")]
    public Joystick joystick;             // optional joystick
    public float joystickDeadzone = 0.1f;

    private Rigidbody rb;
    private float moveInputCurrent = 0f;
    private float turnInputCurrent = 0f;
    private float moveInputVelocity = 0f;
    private float turnInputVelocity = 0f;
    
    private bool isDrifting = false;

    public override void Activate(Action<ICarController> onFinishCallback)
    {
        base.Activate(onFinishCallback);
        
        smokeParticle.gameObject.SetActive(true);
        smokeParticle.Stop();
        
        sfxCarIdle.Stop();
        sfxCarEngine.Play();
    }

    public override void DeActivate()
    {
        base.DeActivate();
        sfxCarEngine.Stop();
        sfxCarDrift.Stop();
        sfxCarIdle.Play();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.centerOfMass = new Vector3(0, -1f, 0);
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;
        
        smokeParticle.gameObject.SetActive(false);

        sfxCarDrift.loop = true;
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
        // Raw input values
        float rawMove = 0f;
        float rawTurn = 0f;
        bool handled = false;

        // Joystick (world-aligned)
        if (joystick != null)
        {
            Vector2 dir = joystick.Direction;
            float mag = dir.magnitude;
            if (mag > joystickDeadzone)
            {
                Vector3 worldDir = new Vector3(dir.x, 0f, dir.y).normalized;
                rawMove = Vector3.Dot(worldDir, transform.forward) * mag;
                rawTurn = Vector3.Dot(worldDir, transform.right) * mag;
                handled = true;
            }
        }

        // Mouse control
        if (!handled && Input.GetMouseButton(0))
        {
            rawMove = 1f;
            rawTurn = (Input.mousePosition.x < Screen.width * 0.5f) ? -1f : 1f;
            handled = true;
        }

        // Keyboard fallback
        if (!handled)
        {
            rawMove = Input.GetAxis("Vertical");
            rawTurn = Input.GetAxis("Horizontal");
        }

        // Smooth inputs
        moveInputCurrent = Mathf.SmoothDamp(moveInputCurrent, rawMove, ref moveInputVelocity, inputSmoothTime);
        turnInputCurrent = Mathf.SmoothDamp(turnInputCurrent, rawTurn, ref turnInputVelocity, inputSmoothTime);

        // Apply acceleration
        Vector3 forwardForce = transform.forward * moveInputCurrent * acceleration;
        rb.AddForce(forwardForce, ForceMode.Acceleration);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        // Apply steering
        float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / maxSpeed);
        float yaw = turnInputCurrent * turnSpeed * speedFactor * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, yaw, 0f));

        // Drift physics
        Vector3 vel = rb.velocity;
        float forwardVel = Vector3.Dot(vel, transform.forward);
        float sidewaysVel = Vector3.Dot(vel, transform.right);
        rb.velocity = transform.forward * forwardVel + transform.right * sidewaysVel * driftFactor;
        
        // Drift audio logic
        bool drifting = Mathf.Abs(sidewaysVel) > driftThreshold;
        if (drifting != isDrifting)
        {
            isDrifting = drifting;
            if (isDrifting)
            {
                sfxCarDrift.Play();
                smokeParticle.Play();
            }
            else
            {
                sfxCarDrift.Stop();
                smokeParticle.Stop();
            }
                
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
        if (other.gameObject.CompareTag("Road") || other.gameObject.CompareTag("Terrain"))
            return;
        
        SoundManager.Instance.PlaySFX(SoundEffect.CarHitObject);
    }
    
    void OnCollisionStay(Collision collision)
    {
        // Prevent sticking: remove velocity into collision surfaces
        Vector3 avgNormal = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
            avgNormal += contact.normal;
        avgNormal.Normalize();

        Vector3 vel = rb.velocity;
        Vector3 projected = Vector3.ProjectOnPlane(vel, avgNormal);
        rb.velocity = projected;
    }
}
