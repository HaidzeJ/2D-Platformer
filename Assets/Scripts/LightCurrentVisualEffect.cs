using UnityEngine;

/// <summary>
/// Visual effect component for Light Currents.
/// Creates flowing particle effects and animations to show the current's direction and activity.
/// </summary>
public class LightCurrentVisualEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] ParticleSystem particleEffect;
    [SerializeField] bool createParticleSystemIfMissing = true;
    
    [Header("Flow Animation")]
    [SerializeField] bool animateFlow = true;
    [SerializeField] float flowSpeed = 2f;
    [SerializeField] AnimationCurve flowCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    [Header("Intensity Settings")]
    [SerializeField] float idleIntensity = 0.3f;
    [SerializeField] float activeIntensity = 1f;
    [SerializeField] float transitionSpeed = 3f;
    
    [Header("Color Settings")]
    [SerializeField] Color currentColor = Color.cyan;
    [SerializeField] bool pulseColor = true;
    [SerializeField] float pulseDuration = 2f;
    
    private LightCurrent lightCurrent;
    private bool isActive = false;
    private float currentIntensity;
    private float flowTime;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    private ParticleSystem.EmissionModule emissionModule;
    
    void Awake()
    {
        // Get or create particle system
        if (particleEffect == null)
        {
            particleEffect = GetComponent<ParticleSystem>();
            
            if (particleEffect == null && createParticleSystemIfMissing)
            {
                CreateDefaultParticleSystem();
            }
        }
        
        // Get the light current component
        lightCurrent = GetComponent<LightCurrent>();
        
        if (particleEffect != null)
        {
            SetupParticleModules();
            ConfigureForLightCurrent();
        }
        
        currentIntensity = idleIntensity;
    }
    
    void CreateDefaultParticleSystem()
    {
        GameObject psObject = new GameObject("ParticleSystem");
        psObject.transform.SetParent(transform);
        psObject.transform.localPosition = Vector3.zero;
        
        particleEffect = psObject.AddComponent<ParticleSystem>();
        
        // Configure basic settings
        var main = particleEffect.main;
        main.startLifetime = 2f;
        main.startSpeed = 1f;
        main.startSize = 0.1f;
        main.startColor = currentColor;
        main.maxParticles = 50;
        
        var emission = particleEffect.emission;
        emission.rateOverTime = 20f;
        
        var shape = particleEffect.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(1f, 1f, 1f);
    }
    
    void SetupParticleModules()
    {
        mainModule = particleEffect.main;
        velocityModule = particleEffect.velocityOverLifetime;
        emissionModule = particleEffect.emission;
        
        // Enable velocity over lifetime for directional flow
        velocityModule.enabled = true;
        velocityModule.space = ParticleSystemSimulationSpace.World;
    }
    
    void ConfigureForLightCurrent()
    {
        if (lightCurrent == null) return;
        
        // Set particle direction based on light current direction
        Vector2 currentDirection = lightCurrent.NormalizedDirection;
        
        velocityModule.x = new ParticleSystem.MinMaxCurve(currentDirection.x * flowSpeed);
        velocityModule.y = new ParticleSystem.MinMaxCurve(currentDirection.y * flowSpeed);
        
        // Set initial color
        mainModule.startColor = currentColor;
    }
    
    void Update()
    {
        if (particleEffect == null) return;
        
        UpdateIntensity();
        UpdateFlow();
        UpdateColor();
    }
    
    void UpdateIntensity()
    {
        float targetIntensity = isActive ? activeIntensity : idleIntensity;
        currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, transitionSpeed * Time.deltaTime);
        
        // Apply intensity to emission rate
        var emission = particleEffect.emission;
        emission.rateOverTime = Mathf.Lerp(5f, 30f, currentIntensity);
        
        // Apply intensity to particle alpha
        Color baseColor = currentColor;
        baseColor.a *= currentIntensity;
        mainModule.startColor = baseColor;
    }
    
    void UpdateFlow()
    {
        if (!animateFlow || lightCurrent == null) return;
        
        flowTime += Time.deltaTime * flowSpeed;
        float flowValue = flowCurve.Evaluate((flowTime % pulseDuration) / pulseDuration);
        
        // Modulate the velocity based on flow animation
        Vector2 currentDirection = lightCurrent.NormalizedDirection;
        float modulator = 0.5f + (flowValue * 0.5f); // Range from 0.5 to 1.0
        
        velocityModule.x = new ParticleSystem.MinMaxCurve(currentDirection.x * flowSpeed * modulator);
        velocityModule.y = new ParticleSystem.MinMaxCurve(currentDirection.y * flowSpeed * modulator);
    }
    
    void UpdateColor()
    {
        if (!pulseColor) return;
        
        float pulse = Mathf.Sin(Time.time * (2f * Mathf.PI / pulseDuration)) * 0.5f + 0.5f;
        Color pulsedColor = Color.Lerp(currentColor * 0.7f, currentColor, pulse);
        pulsedColor.a *= currentIntensity;
        
        mainModule.startColor = pulsedColor;
    }
    
    /// <summary>
    /// Activate the visual effect (called when player enters current)
    /// </summary>
    public void Activate()
    {
        isActive = true;
        
        if (particleEffect != null && !particleEffect.isPlaying)
        {
            particleEffect.Play();
        }
    }
    
    /// <summary>
    /// Deactivate the visual effect (called when player exits current)
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
        
        // Don't stop completely, just reduce intensity for ambient effect
    }
    
    /// <summary>
    /// Update the effect to match current properties
    /// </summary>
    public void UpdateForCurrentProperties()
    {
        ConfigureForLightCurrent();
    }
    
    /// <summary>
    /// Set custom color for this light current
    /// </summary>
    public void SetColor(Color color)
    {
        currentColor = color;
        if (particleEffect != null)
        {
            mainModule.startColor = currentColor;
        }
    }
    
    void OnValidate()
    {
        // Update settings when changed in inspector
        if (Application.isPlaying && particleEffect != null)
        {
            ConfigureForLightCurrent();
        }
    }
}