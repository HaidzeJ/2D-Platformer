using UnityEngine;
using System.Collections;

/// <summary>
/// Applies a gentle floating effect directly to the GameObject itself.
/// Perfect for objects in stasis, magical items, or tutorial states.
/// Automatically stops when the object becomes active/physics-enabled.
/// </summary>
public class FloatingParticleEffect : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatIntensity = 0.1f;
    [SerializeField] private Vector3 floatDirection = Vector3.up;
    [SerializeField] private bool randomPhase = true;
    [SerializeField] private bool randomizeDirection = true;
    
    [Header("Randomization")]
    [SerializeField] private bool addRandomVariation = true;
    [SerializeField] private float randomIntensityRange = 0.02f;
    [SerializeField] private float randomSpeedRange = 0.3f; // More reasonable speed variation
    [SerializeField] private float directionVariation = 45f; // Max degrees to vary from base direction
    [SerializeField] private bool fullyRandomDirection = false; // Complete random directions instead of variation
    [SerializeField] private bool useRandomWaveTypes = true; // Use different wave functions for variety
    
    [Header("Auto-Stop Conditions")]
    [SerializeField] private bool stopOnPlayerMovement = true;
    [SerializeField] private bool stopOnRigidbodyActive = false; // Disabled by default
    [SerializeField] private float velocityThreshold = 0.5f; // Higher threshold
    [SerializeField] private bool enableAutoStop = true; // Master toggle for auto-stop
    [SerializeField] private float autoStopDelay = 1f; // Delay before auto-stop checks begin
    
    // Components and state
    private bool effectActive = false;
    private Coroutine effectCoroutine;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private Vector3 originalPosition;
    private Vector3 lastPosition;
    private float floatPhase;
    
    // Randomization values
    private float randomIntensityMultiplier;
    private float randomSpeedMultiplier;
    private Vector3 randomizedDirection;
    private Vector3 randomizedSecondaryDirection;
    private Vector3 randomizedTertiaryDirection;
    private float randomPhaseOffset;
    private float randomSecondaryPhaseOffset;
    private float randomTertiaryPhaseOffset;
    
    // Wave function types for variety
    private int primaryWaveType;
    private int secondaryWaveType;
    private int tertiaryWaveType;
    
    // Speed multipliers for each layer
    private float secondarySpeedMultiplier;
    private float tertiarySpeedMultiplier;
    
    // Timing
    private float effectStartTime;
    private float uniqueTimeOffset;
    
    void Awake()
    {
        // Get components for auto-stop detection
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
        lastPosition = transform.position;
        
        // Use object instance ID and position to ensure unique randomization per object
        Random.InitState(GetInstanceID() + (int)(transform.position.x * 1000f) + (int)(transform.position.y * 1000f));
        
        // Initialize random values with truly unique seeds
        floatPhase = randomPhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
        randomIntensityMultiplier = addRandomVariation ? Random.Range(1f - randomIntensityRange, 1f + randomIntensityRange) : 1f;
        randomSpeedMultiplier = addRandomVariation ? Random.Range(1f - randomSpeedRange, 1f + randomSpeedRange) : 1f;
        
        // Initialize random phase offsets for each movement layer
        randomPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
        randomSecondaryPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
        randomTertiaryPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
        
        // Initialize random wave types for variety
        primaryWaveType = Random.Range(0, 4); // 0=sin, 1=cos, 2=triangle, 3=smooth step
        secondaryWaveType = Random.Range(0, 4);
        tertiaryWaveType = Random.Range(0, 4);
        
        // Initialize unique speed multipliers for each movement layer - more reasonable ranges
        secondarySpeedMultiplier = Random.Range(0.7f, 1.3f);
        tertiarySpeedMultiplier = Random.Range(0.8f, 1.4f);
        
        // Initialize randomized directions
        InitializeRandomDirections();
        
        // Create a unique time offset based on object properties - reasonable range
        uniqueTimeOffset = Random.Range(0f, 50f);
        
        // Debug info
        Debug.Log($"🎲 FloatingEffect on '{name}' initialized - Direction: {randomizedDirection}, Wave: {primaryWaveType}, Speed: {randomSpeedMultiplier:F2}, TimeOffset: {uniqueTimeOffset:F2}, SecSpeed: {secondarySpeedMultiplier:F2}");
    }
    
    /// <summary>
    /// Initialize randomized movement directions for organic variation
    /// </summary>
    void InitializeRandomDirections()
    {
        if (randomizeDirection)
        {
            if (fullyRandomDirection)
            {
                // Completely random directions
                randomizedDirection = GetRandomDirection();
                randomizedSecondaryDirection = GetRandomDirection();
                randomizedTertiaryDirection = GetRandomDirection();
            }
            else
            {
                // Randomize primary direction within specified variation
                randomizedDirection = RandomizeDirection(floatDirection, directionVariation);
                
                // Create random perpendicular directions for secondary movement
                randomizedSecondaryDirection = RandomizeDirection(GetPerpendicularDirection(randomizedDirection), directionVariation * 0.5f);
                randomizedTertiaryDirection = RandomizeDirection(GetAnotherPerpendicularDirection(randomizedDirection), directionVariation * 0.3f);
            }
        }
        else
        {
            // Use original directions
            randomizedDirection = floatDirection;
            randomizedSecondaryDirection = Vector3.right;
            randomizedTertiaryDirection = Vector3.forward;
        }
    }
    
    /// <summary>
    /// Randomize a direction vector within specified angle variation
    /// </summary>
    Vector3 RandomizeDirection(Vector3 baseDirection, float maxAngleDegrees)
    {
        if (maxAngleDegrees <= 0f) return baseDirection.normalized;
        
        // Create random rotation within the specified angle
        float randomAngle = Random.Range(-maxAngleDegrees, maxAngleDegrees);
        
        // For 2D games, rotate around Z-axis
        Quaternion randomRotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
        return randomRotation * baseDirection.normalized;
    }
    
    /// <summary>
    /// Get a perpendicular direction to the given vector
    /// </summary>
    Vector3 GetPerpendicularDirection(Vector3 direction)
    {
        // For 2D, get perpendicular in the XY plane
        return new Vector3(-direction.y, direction.x, direction.z).normalized;
    }
    
    /// <summary>
    /// Get another perpendicular direction (for 3D depth)
    /// </summary>
    Vector3 GetAnotherPerpendicularDirection(Vector3 direction)
    {
        // Cross product with up vector to get another perpendicular
        Vector3 cross = Vector3.Cross(direction.normalized, Vector3.up);
        if (cross.magnitude < 0.1f)
        {
            // If direction is parallel to up, use right vector
            cross = Vector3.Cross(direction.normalized, Vector3.right);
        }
        return cross.normalized;
    }
    
    /// <summary>
    /// Get a completely random direction in 2D space
    /// </summary>
    Vector3 GetRandomDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad);
        float y = Mathf.Sin(randomAngle * Mathf.Deg2Rad);
        return new Vector3(x, y, 0f).normalized;
    }
    
    /// <summary>
    /// Evaluate different wave functions for variety - all smooth and continuous
    /// </summary>
    float EvaluateWave(int waveType, float time)
    {
        switch (waveType)
        {
            case 0: return Mathf.Sin(time); // Standard sine wave
            case 1: return Mathf.Cos(time); // Cosine wave (90° phase shift)
            case 2: // Smoother sine variant with different frequency
                return Mathf.Sin(time * 0.7f) * 0.8f;
            case 3: // Sine with harmonic for gentle complexity
                return Mathf.Sin(time) + Mathf.Sin(time * 2f) * 0.3f;
            default: return Mathf.Sin(time);
        }
    }
    
    void Start()
    {
        StartEffect();
    }
    
    void Update()
    {
        if (effectActive && enableAutoStop && Time.time - effectStartTime > autoStopDelay)
        {
            CheckAutoStopConditions();
        }
    }
    
    /// <summary>
    /// Start the floating effect on this GameObject
    /// </summary>
    public void StartEffect()
    {
        if (effectActive) return;
        
        effectActive = true;
        originalPosition = transform.position;
        effectStartTime = Time.time;
        
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }
        
        effectCoroutine = StartCoroutine(FloatingEffectCoroutine());
        
        Debug.Log($"✨ Floating effect started on {name}");
    }
    
    /// <summary>
    /// Stop the floating effect and return to original position
    /// </summary>
    public void StopEffect()
    {
        if (!effectActive) return;
        
        effectActive = false;
        
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }
        
        // Smoothly return to original position
        StartCoroutine(ReturnToOriginalPosition());
        
        Debug.Log($"💫 Floating effect stopped on {name}");
    }
    
    /// <summary>
    /// Update the original position (useful if object was moved before floating started)
    /// </summary>
    public void UpdateOriginalPosition()
    {
        originalPosition = transform.position;
    }
    
    IEnumerator FloatingEffectCoroutine()
    {
        float time = uniqueTimeOffset; // Start with unique time offset
        
        while (effectActive)
        {
            time += Time.deltaTime;
            
            // Apply randomized speed and phase with much more dramatic variation
            float effectiveSpeed = floatSpeed * randomSpeedMultiplier;
            float effectiveIntensity = floatIntensity * randomIntensityMultiplier;
            
            // Use different wave functions for completely unique movement patterns
            float primaryTime = time * effectiveSpeed + floatPhase + randomPhaseOffset;
            float primaryFloat = useRandomWaveTypes ? EvaluateWave(primaryWaveType, primaryTime) : Mathf.Sin(primaryTime);
            Vector3 floatOffset = randomizedDirection * (primaryFloat * effectiveIntensity);
            
            // Add subtle secondary movement for more organic feel
            if (addRandomVariation)
            {
                // Each layer gets its own unique timing, direction, and wave function
                float secondaryTime = time * effectiveSpeed * secondarySpeedMultiplier + randomSecondaryPhaseOffset;
                float tertiaryTime = time * effectiveSpeed * tertiarySpeedMultiplier + randomTertiaryPhaseOffset;
                
                float secondaryFloat = useRandomWaveTypes ? EvaluateWave(secondaryWaveType, secondaryTime) : Mathf.Sin(secondaryTime);
                float tertiaryFloat = useRandomWaveTypes ? EvaluateWave(tertiaryWaveType, tertiaryTime) : Mathf.Sin(tertiaryTime);
                
                Vector3 secondaryOffset = randomizedSecondaryDirection * secondaryFloat * (effectiveIntensity * 0.3f);
                Vector3 tertiaryOffset = randomizedTertiaryDirection * tertiaryFloat * (effectiveIntensity * 0.2f);
                
                floatOffset += secondaryOffset + tertiaryOffset;
            }
            
            // Apply the floating movement to this GameObject
            transform.position = originalPosition + floatOffset;
            
            yield return null;
        }
    }
    
    IEnumerator ReturnToOriginalPosition()
    {
        Vector3 currentPosition = transform.position;
        float returnDuration = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < returnDuration)
        {
            float progress = elapsedTime / returnDuration;
            transform.position = Vector3.Lerp(currentPosition, originalPosition, progress);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we're exactly at the original position
        transform.position = originalPosition;
    }
    
    void CheckAutoStopConditions()
    {
        bool shouldStop = false;
        
        // Check if player is no longer in pre-tutorial mode
        if (stopOnPlayerMovement && playerMovement != null)
        {
            if (!playerMovement.IsInPreTutorial)
            {
                shouldStop = true;
                Debug.Log($"🛑 Stopping floating effect on {name} - Player no longer in pre-tutorial");
            }
        }
        
        // Check if rigidbody is moving with significant external velocity
        if (stopOnRigidbodyActive && rb != null)
        {
            // Only stop if the rigidbody has significant velocity that's not from our floating effect
            if (rb.linearVelocity.magnitude > velocityThreshold)
            {
                shouldStop = true;
                Debug.Log($"🛑 Stopping floating effect on {name} - Rigidbody velocity: {rb.linearVelocity.magnitude}");
            }
        }
        
        // Check if object has been moved significantly from its original position
        // (not counting our own floating movement)
        float distanceFromOriginal = Vector3.Distance(transform.position, originalPosition);
        if (distanceFromOriginal > (floatIntensity * 2f + velocityThreshold))
        {
            shouldStop = true;
            Debug.Log($"🛑 Stopping floating effect on {name} - Distance from original: {distanceFromOriginal}");
        }
        
        if (shouldStop)
        {
            StopEffect();
        }
        
        lastPosition = transform.position;
    }
    
    /// <summary>
    /// Manually set whether the effect should be active
    /// </summary>
    public void SetEffectActive(bool active)
    {
        if (active && !effectActive)
        {
            StartEffect();
        }
        else if (!active && effectActive)
        {
            StopEffect();
        }
    }
    
    /// <summary>
    /// Check if the effect is currently active
    /// </summary>
    public bool IsEffectActive => effectActive;
    
    void OnDrawGizmosSelected()
    {
        // Draw float direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, floatDirection.normalized * floatIntensity);
        
        // Draw original position when effect is active
        if (effectActive && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(originalPosition, 0.1f);
            Gizmos.DrawLine(originalPosition, transform.position);
        }
    }
}