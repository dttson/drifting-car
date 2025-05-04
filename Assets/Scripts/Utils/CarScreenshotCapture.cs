using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CarScreenshotCapture : MonoBehaviour
{
    [Header("Capture Settings")]
    public Camera targetCamera;                  // Camera that will render the car
    public KeyCode captureKey = KeyCode.P;       // Key to trigger screenshot
    public int captureWidth = 1024;              // Resolution width
    public int captureHeight = 768;              // Resolution height

    private string folderPath;

    void Awake()
    {
        // Assign camera if not set
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        // Define path inside project Assets
        folderPath = Path.Combine(Application.dataPath, "Images/Screenshots");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Created screenshot folder at: " + folderPath);
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        // Only capture with key during Play mode
        if (!Application.isPlaying)
            return;
#endif
        if (Input.GetKeyDown(captureKey))
        {
            CaptureCarImage();
        }
    }

    [ContextMenu("Capture Screenshot")]
    public void CaptureCarImage()
    {
        // Backup original camera settings
        RenderTexture originalRT = targetCamera.targetTexture;
        CameraClearFlags originalClear = targetCamera.clearFlags;
        Color originalBG = targetCamera.backgroundColor;

        // Create a transparent RenderTexture
        RenderTexture rt = new RenderTexture(captureWidth, captureHeight, 24, RenderTextureFormat.ARGB32);
        rt.Create();
        targetCamera.targetTexture = rt;

        // Set camera to clear with transparent background
        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = new Color(0, 0, 0, 0);

        // Render and read pixels
        targetCamera.Render();
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);
        screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        screenShot.Apply();

        // Reset camera and cleanup
        targetCamera.targetTexture = originalRT;
        targetCamera.clearFlags = originalClear;
        targetCamera.backgroundColor = originalBG;
        RenderTexture.active = null;
        rt.Release();
        DestroyImmediate(rt);

        // Encode to PNG (supports transparency)
        byte[] bytes = screenShot.EncodeToPNG();
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = Path.Combine(folderPath, $"Car_{timestamp}.png");
        File.WriteAllBytes(filename, bytes);
        Debug.Log($"Saved transparent screenshot to: {filename}");

#if UNITY_EDITOR
        // Refresh AssetDatabase to show the new file
        AssetDatabase.Refresh();
#endif
    }
}