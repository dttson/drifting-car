using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class AICarController : MonoBehaviour
{
    [Header("Spline & Movement")]
    public SplineContainer splineContainer;
    public bool loop = true;
    public float speed = 15f;

    [Header("Overtake Settings")]
    public Transform playerCar;
    public float overtakeDistance = 10f;
    public float overtakeLateralOffset = 2f;
    public float overtakeSpeedBoost = 1.2f;

    private float travel = 0f;
    private float totalLength;

    void Start()
    {
        totalLength = splineContainer.Spline.CalculateLength(transform.localToWorldMatrix);
        Debug.Log($"==== TOTAL LENGTH = {totalLength}====");
    }

    void Update()
    {
        // Calculate normalized t for current travel
        float tNorm = totalLength > 0f ? travel / totalLength : 0f;

        // Get current tangent for direction and overtaking check
        float3 tan3 = splineContainer.Spline.EvaluateTangent(tNorm);
        Vector3 tangent = new Vector3(tan3.x, tan3.y, tan3.z).normalized;

        // AI and player positions
        Vector3 aiPos = transform.position;
        Vector3 playerPos = playerCar.position;
        float dist = Vector3.Distance(aiPos, playerPos);
        float forwardDot = Vector3.Dot(tangent, playerPos - aiPos);

        // Determine overtake
        bool isOvertaking = dist < overtakeDistance && forwardDot > 0f;
        float currentSpeed = isOvertaking ? speed * overtakeSpeedBoost : speed;

        // Advance travel distance
        travel += currentSpeed * Time.deltaTime;
        if (loop)
            travel %= totalLength;
        else
            travel = Mathf.Clamp(travel, 0f, totalLength);

        // Recalculate normalized t after update
        tNorm = totalLength > 0f ? travel / totalLength : 0f;

        // Evaluate position and up vector
        float3 pos3 = splineContainer.Spline.EvaluatePosition(tNorm);
        float3 up3 = splineContainer.Spline.EvaluateUpVector(tNorm);
        Vector3 pos = new Vector3(pos3.x, pos3.y, pos3.z);
        Vector3 up = new Vector3(up3.x, up3.y, up3.z);

        // Apply transform
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(tangent, up);

        // Lateral offset for the visual model (child 0)
        if (transform.childCount > 0)
        {
            Transform model = transform.GetChild(0);
            float offset = isOvertaking ? overtakeLateralOffset : 0f;
            Vector3 targetLocal = Vector3.right * offset;
            model.localPosition = Vector3.Lerp(model.localPosition, targetLocal, Time.deltaTime * 5f);
        }
    }
}
