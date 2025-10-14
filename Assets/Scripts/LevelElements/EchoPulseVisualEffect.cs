using UnityEngine;

/// <summary>
/// Visual effect for the Echo Pulse - creates an expanding ring that shows the pulse radius.
/// Attach this to the player or create as a separate GameObject in the scene.
/// </summary>
public class EchoPulseVisualEffect : MonoBehaviour
{
    [Header("Ring Visual Settings")]
    [SerializeField] private LineRenderer ringRenderer;
    [SerializeField] private int ringSegments = 64;
    [SerializeField] private Color ringColor = new Color(0.3f, 0.8f, 1f, 0.8f); // Cyan
    [SerializeField] private AnimationCurve ringOpacityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private float ringWidth = 0.1f;
    
    [Header("Animation Settings")]
    [SerializeField] private float pulseExpandSpeed = 25f;
    [SerializeField] private float maxPulseRadius = 10f;
    [SerializeField] private bool usePlayerSettings = true; // Get settings from PlayerMovement
    
    [Header("Additional Effects")]
    [SerializeField] private ParticleSystem pulseParticles;
    [SerializeField] private bool createParticleRing = true;
    [SerializeField] private AudioClip pulseSoundEffect;
    
    private PlayerMovement playerMovement;
    private AudioSource audioSource;
    private bool isPulsing = false;
    private float currentRadius = 0f;
    
    void Awake()
    {
        // Get player movement reference
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pulseSoundEffect != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.6f;
        }
        
        // Setup line renderer for ring
        SetupRingRenderer();
        
        // Setup particle system
        SetupParticleSystem();
    }
    
    /// <summary>
    /// Setup the LineRenderer for the expanding ring
    /// </summary>
    void SetupRingRenderer()
    {
        if (ringRenderer == null)
        {
            // Create LineRenderer component
            ringRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configure LineRenderer
        ringRenderer.material = CreateRingMaterial();
        ringRenderer.startColor = ringColor;
        ringRenderer.endColor = ringColor;
        ringRenderer.startWidth = ringWidth;
        ringRenderer.endWidth = ringWidth;
        ringRenderer.positionCount = ringSegments + 1; // +1 to close the circle
        ringRenderer.useWorldSpace = false;
        ringRenderer.loop = true;
        ringRenderer.enabled = false; // Start hidden
        
        // Generate circle points
        UpdateRingPoints(0f);
    }
    
    /// <summary>
    /// Create or get material for the ring
    /// </summary>
    Material CreateRingMaterial()
    {
        // Try to use built-in sprite material, or create a simple one
        Material ringMat = new Material(Shader.Find("Sprites/Default"));
        ringMat.color = ringColor;
        return ringMat;
    }
    
    /// <summary>
    /// Setup particle system for additional effects
    /// </summary>
    void SetupParticleSystem()
    {
        if (pulseParticles == null && createParticleRing)
        {
            // Create a simple particle system
            GameObject particleObject = new GameObject("PulseParticles");
            particleObject.transform.SetParent(transform);
            particleObject.transform.localPosition = Vector3.zero;
            
            pulseParticles = particleObject.AddComponent<ParticleSystem>();
            
            // Configure particle system
            var main = pulseParticles.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            main.startSize = 0.1f;
            main.startColor = ringColor;
            main.maxParticles = 50;
            
            var emission = pulseParticles.emission;
            emission.enabled = false; // We'll control emission manually
            
            var shape = pulseParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;
            
            pulseParticles.Stop();
        }
    }
    
    /// <summary>
    /// Start the echo pulse visual effect
    /// </summary>
    public void TriggerPulseEffect(Vector3 pulseCenter)
    {
        if (isPulsing) return;
        
        // Get settings from player if available
        if (usePlayerSettings && playerMovement != null)
        {
            // We'll need to add public getters to PlayerMovement for these
            maxPulseRadius = 10f; // Default fallback
            pulseExpandSpeed = 25f; // Default fallback
        }
        
        // Start pulse animation
        StartCoroutine(AnimatePulse(pulseCenter));
        
        // Play sound effect
        if (audioSource != null && pulseSoundEffect != null)
        {
            audioSource.PlayOneShot(pulseSoundEffect);
        }
    }
    
    /// <summary>
    /// Animate the expanding pulse ring
    /// </summary>
    System.Collections.IEnumerator AnimatePulse(Vector3 center)
    {
        isPulsing = true;
        currentRadius = 0f;
        
        // Position the effect at pulse center
        transform.position = center;
        
        // Enable visual elements
        if (ringRenderer != null)
        {
            ringRenderer.enabled = true;
        }
        
        // Start particle emission
        if (pulseParticles != null)
        {
            pulseParticles.Play();
        }
        
        // Animate expansion
        float duration = maxPulseRadius / pulseExpandSpeed;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Update radius
            currentRadius = Mathf.Lerp(0f, maxPulseRadius, t);
            
            // Update ring visuals
            UpdateRingPoints(currentRadius);
            UpdateRingOpacity(t);
            
            // Update particle system
            if (pulseParticles != null)
            {
                var shape = pulseParticles.shape;
                shape.radius = currentRadius;
            }
            
            yield return null;
        }
        
        // Cleanup
        if (ringRenderer != null)
        {
            ringRenderer.enabled = false;
        }
        
        if (pulseParticles != null)
        {
            pulseParticles.Stop();
        }
        
        isPulsing = false;
    }
    
    /// <summary>
    /// Update the ring LineRenderer points
    /// </summary>
    void UpdateRingPoints(float radius)
    {
        if (ringRenderer == null) return;
        
        Vector3[] points = new Vector3[ringSegments + 1];
        
        for (int i = 0; i <= ringSegments; i++)
        {
            float angle = (float)i / ringSegments * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            points[i] = new Vector3(x, y, 0f);
        }
        
        ringRenderer.SetPositions(points);
    }
    
    /// <summary>
    /// Update ring opacity based on animation progress
    /// </summary>
    void UpdateRingOpacity(float normalizedTime)
    {
        if (ringRenderer == null) return;
        
        float opacity = ringOpacityCurve.Evaluate(normalizedTime);
        Color newColor = ringColor;
        newColor.a = opacity;
        ringRenderer.startColor = newColor;
        ringRenderer.endColor = newColor;
    }
    
    /// <summary>
    /// Public method to get current pulse radius (for other systems)
    /// </summary>
    public float GetCurrentPulseRadius()
    {
        return isPulsing ? currentRadius : 0f;
    }
    
    /// <summary>
    /// Check if pulse effect is currently running
    /// </summary>
    public bool IsPulsing => isPulsing;
    
    /// <summary>
    /// Stop current pulse effect
    /// </summary>
    public void StopPulseEffect()
    {
        if (isPulsing)
        {
            StopAllCoroutines();
            isPulsing = false;
            currentRadius = 0f;
            
            if (ringRenderer != null)
            {
                ringRenderer.enabled = false;
            }
            
            if (pulseParticles != null)
            {
                pulseParticles.Stop();
            }
        }
    }
    
    /// <summary>
    /// Configure pulse settings manually
    /// </summary>
    public void ConfigurePulse(float expandSpeed, float maxRadius, Color color)
    {
        pulseExpandSpeed = expandSpeed;
        maxPulseRadius = maxRadius;
        ringColor = color;
        
        if (ringRenderer != null)
        {
            ringRenderer.startColor = color;
            ringRenderer.endColor = color;
        }
    }
}