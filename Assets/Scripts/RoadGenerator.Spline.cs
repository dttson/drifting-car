using UnityEngine;
using System.Collections.Generic;
using System.IO;
using PathCreation;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if USING_SPLINE
[RequireComponent(typeof(PathCreator))]
public class RoadGenerator : MonoBehaviour
{
    public SplineContainer splineContainer;
    
    [Header("Recording Settings")]
    public DriftCarController driver;              // Reference to the player-driven car
    public float recordInterval = 0.1f;             // Time between position samples
    public bool loop = false;                       // Close spline into a loop
    [SerializeField] private string filePath;
    
    private List<Vector3> recordedPositions = new List<Vector3>();
    private bool isRecording = false;
    private float recordTimer = 0f;
    

    void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        // splineContainer.Spline.Clear();
    }

    private void OnValidate()
    {
        filePath = Path.Combine(Application.persistentDataPath, "recordedPath.json");
    }

    void Update()
    {
        // Sample positions while recording
        if (isRecording && driver != null)
        {
            recordTimer += Time.deltaTime;
            if (recordTimer >= recordInterval)
            {
                recordedPositions.Add(driver.transform.position);
                recordTimer = 0f;
            }
        }
    }

    void OnGUI()
    {
        // Start/Stop Recording button
        string recordLabel = isRecording ? "Stop Recording" : "Start Recording";
        if (GUI.Button(new Rect(10, 10, 150, 30), recordLabel))
        {
            ToggleRecording();
        }

        // Load Recorded Path button
        // if (GUI.Button(new Rect(10, 50, 150, 30), "Load Recorded Path"))
        // {
        //     var positions = LoadRecordedPositions();
        //     if (positions != null && positions.Length > 0)
        //     {
        //         GenerateSpline(new List<Vector3>(positions));
        //     }
        // }
    }

    void ToggleRecording()
    {
        isRecording = !isRecording;
        if (!isRecording)
        {
            GenerateSpline(recordedPositions);
            SaveRecordedPositions();
            recordedPositions.Clear();
            recordTimer = 0f;
        }
    }

    // Build spline from given positions
    void GenerateSpline(List<Vector3> positions)
    {
        splineContainer.Spline.Clear();
        foreach (Vector3 pos in positions)
        {
            splineContainer.Spline.Add(new BezierKnot(pos));
        }
            
        splineContainer.Spline.Closed = loop;
    }

    // Save recorded path to JSON
    void SaveRecordedPositions()
    {
        PathData data = new PathData { positions = recordedPositions.ToArray() };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Saved recorded path to {filePath}");
    }

    // Load recorded path from JSON
    Vector3[] LoadRecordedPositions()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"No recorded path found at {filePath}");
            return null;
        }
        string json = File.ReadAllText(filePath);
        PathData data = JsonUtility.FromJson<PathData>(json);
        Debug.Log($"Loaded recorded path with {data.positions.Length} points");
        return data.positions;
    }
    
    // Visualize spline knots and indices in the editor
    void OnDrawGizmos()
    {
        if (splineContainer == null)
            splineContainer = GetComponent<SplineContainer>();

        var spline = splineContainer.Spline;
        int count = spline.Count;
        for (int i = 0; i < count; i++)
        {
            Vector3 worldPos = splineContainer.transform.TransformPoint(spline[i].Position);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(worldPos, 0.3f);

#if UNITY_EDITOR
            Handles.color = Color.white;
            Handles.Label(worldPos + Vector3.up * 0.5f, i.ToString());
#endif
        }
    }

    [System.Serializable]
    private class PathData
    {
        public Vector3[] positions;
    }
    
    [ContextMenu("Clear saved data")]
    void ClearRecordedData()
    {
        recordedPositions.Clear();
        recordTimer = 0f;
        splineContainer.Spline.Clear();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"Deleted recorded path file: {filePath}");
        }
        Debug.Log("Cleared recorded data and spline.");
    }


    [ContextMenu("Load recorded path")]
    private void LoadRecordedPath()
    {
        var positions = LoadRecordedPositions();
        if (positions != null && positions.Length > 0)
        {
            GenerateSpline(new List<Vector3>(positions));
        }
    }
    
    [ContextMenu("Smoothen Spline")]
    private void SmoothenSpline()
    {
        // Apply Auto tangent mode to all knots for smooth curves
        var range = new SplineRange(0, splineContainer.Spline.Count);
        splineContainer.Spline.SetTangentMode(range, TangentMode.AutoSmooth);
    }
}
#endif