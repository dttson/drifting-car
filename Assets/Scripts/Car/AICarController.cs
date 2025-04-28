// AIBehavior.cs

using System;
using DefaultNamespace;using UnityEngine;
using PathCreation;

public class AICarController : BaseCarController
{
    public override bool IsMyCar => false;

    [Header("Path & Movement")]
    public PathCreator pathCreator;
    public EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop;
    public float speed = 15f;

    [Header("Overtake Settings")]
    public Transform playerCar;
    public float overtakeDistance = 10f;
    public float overtakeLateralOffset = 2f;
    public float overtakeSpeedBoost = 1.2f;

    private float distanceTravelled = 0f;
    
    void Update()
    {
        if (!isActivate)
            return;
        
        // AI and player positions
        Vector3 aiPos = transform.position;
        Vector3 playerPos = playerCar.position;
        float distToPlayer = Vector3.Distance(aiPos, playerPos);

        // Determine current tangent for overtaking check
        Vector3 tangent = pathCreator.path.GetDirectionAtDistance(distanceTravelled, endOfPathInstruction).normalized;
        float forwardDot = Vector3.Dot(tangent, playerPos - aiPos);

        bool isOvertaking = distToPlayer < overtakeDistance && forwardDot > 0f;
        float currentSpeed = isOvertaking ? speed * overtakeSpeedBoost : speed;

        // Advance along path
        distanceTravelled += currentSpeed * Time.deltaTime;
        if (endOfPathInstruction == EndOfPathInstruction.Loop)
            distanceTravelled %= pathCreator.path.length;
        else
            distanceTravelled = Mathf.Clamp(distanceTravelled, 0f, pathCreator.path.length);

        // Evaluate position and tangent
        Vector3 point = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
        tangent = pathCreator.path.GetDirectionAtDistance(distanceTravelled, endOfPathInstruction).normalized;

        // Compute lateral offset using world normal
        Vector3 normal = Vector3.Cross(Vector3.up, tangent).normalized;
        float offset = isOvertaking ? overtakeLateralOffset : 0f;
        Vector3 finalPos = point + normal * offset;

        // Apply position and rotation
        transform.position = finalPos;
        transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
    }
}
