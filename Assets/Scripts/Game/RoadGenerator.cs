using UnityEngine;
using System.Collections.Generic;
using System.IO;
using PathCreation;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PathCreator))]
public class RoadGenerator : MonoBehaviour
{
    public PathCreator pathCreator;
    
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
        if (pathCreator == null)
            pathCreator = GetComponent<PathCreator>();
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(filePath))
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
            GeneratePath(recordedPositions);
            SaveRecordedPositions();
            recordedPositions.Clear();
            recordTimer = 0f;
        }
    }

    // Build spline from given positions
    void GeneratePath(List<Vector3> positions)
    {
        var bezierPath = new BezierPath(positions)
        {
            GlobalNormalsAngle = 90f
        };
        pathCreator.bezierPath = bezierPath;
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
        pathCreator.bezierPath = null;
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
            GeneratePath(new List<Vector3>(positions));
        }
    }
}
