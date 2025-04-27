using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SmokeParticle : MonoBehaviour
{
    public DriftCarController driftController;
    
    private ParticleSystem smokePS;

    void Awake()
    {
        // Create child GameObject for smoke
        // var psGO = new GameObject("DriftSmoke");
        // psGO.transform.SetParent(transform);
        // psGO.transform.localPosition = new Vector3(0f, 0.1f, -1f);

        // Add and configure ParticleSystem
        // smokePS = GetComponent<ParticleSystem>();
        // var main = smokePS.main;
        // main.loop = true;
        // main.startLifetime = 0.5f;
        // main.startSpeed = 1f;
        // main.startSize = 0.5f;
        // main.startColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        // main.simulationSpace = ParticleSystemSimulationSpace.World;
        //
        // var emission = smokePS.emission;
        // emission.rateOverTime = 0f;
        //
        // var shape = smokePS.shape;
        // shape.shapeType = ParticleSystemShapeType.Cone;
        // shape.angle = 25f;
        // shape.radius = 0.1f;

        // var renderer = smokePS.GetComponent<ParticleSystemRenderer>();
        // renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        // renderer.renderMode = ParticleSystemRenderMode.Billboard;

        smokePS = GetComponent<ParticleSystem>();
        smokePS.Stop();
    }

    void Update()
    {
        if (driftController == null) return;
        // Toggle smoke based on drift state
        if (driftController.IsDrifting && !smokePS.isPlaying)
            smokePS.Play();
        else if (!driftController.IsDrifting && smokePS.isPlaying)
            smokePS.Stop();
    }
}