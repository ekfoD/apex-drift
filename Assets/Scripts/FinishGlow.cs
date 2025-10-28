using UnityEngine;

public class FinishLineGlowController : MonoBehaviour
{
    [Header("Target - Drag the 'Finish' sign here")]
    public GameObject finishSignObject;
    
    [Header("Materials")]
    public Material normalMaterial;
    public Material glowingMaterial;
    
    [Header("Optional Effects")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    public float minIntensity = 2f;
    public float maxIntensity = 5f;
    
    [Header("Testing - Click these buttons in Play Mode")]
    public bool testGlowOn = false;
    public bool testGlowOff = false;
    
    private Renderer signRenderer;
    private bool isGlowing = false;
    private Color baseEmissionColor;
    
    void Start()
    {
        if (finishSignObject != null)
        {
            signRenderer = finishSignObject.GetComponent<Renderer>();
            if (normalMaterial != null)
            {
                signRenderer.material = normalMaterial;
            }
            
            if (glowingMaterial != null)
            {
                baseEmissionColor = glowingMaterial.GetColor("_EmissionColor");
            }
        }
    }
    
    void Update()
    {
        if (testGlowOn)
        {
            testGlowOn = false;
            StartGlowing();
        }
        if (testGlowOff)
        {
            testGlowOff = false;
            StopGlowing();
        }
        
        if (isGlowing && enablePulse && signRenderer != null)
        {
            float pulse = Mathf.Lerp(minIntensity, maxIntensity, 
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            
            Color emissionColor = baseEmissionColor * pulse;
            signRenderer.material.SetColor("_EmissionColor", emissionColor);
        }
    }
    
    public void StartGlowing()
    {
        if (signRenderer != null && glowingMaterial != null)
        {
            isGlowing = true;
            signRenderer.material = glowingMaterial;
        }
    }
    
    public void StopGlowing()
    {
        if (signRenderer != null && normalMaterial != null)
        {
            isGlowing = false;
            signRenderer.material = normalMaterial;
        }
    }
}