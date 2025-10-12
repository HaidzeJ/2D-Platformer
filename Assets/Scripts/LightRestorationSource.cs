using UnityEngine;
using System.Collections;

/// <summary>
/// Light Restoration Source - Objects that can restore the player's light resource.
/// Examples: Light crystals, campfires, sunbeams, healing fountains, etc.
/// </summary>
public class LightRestorationSource : MonoBehaviour
{
    [Header("Restoration Settings")]
    [SerializeField] private float restorationAmount = 20f;
    [SerializeField] private float restorationPerSecond = 10f;
    [SerializeField] private bool isContinuousHealing = true;
    [SerializeField] private bool consumeOnUse = false; // Single-use or permanent
    [SerializeField] private float rechargeTime = 30f; // Time to recharge after consumption
    
    [Header("Activation")]
    [SerializeField] private bool autoActivate = true; // Activate on proximity
    [SerializeField] private bool requiresInteraction = false; // Needs input to activate
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float activationRadius = 2f;
    
    [Header("Visual Effects")]
    [SerializeField] private Light restorationLight;
    [SerializeField] private ParticleSystem healingEffect;
    [SerializeField] private ParticleSystem ambientEffect; // Always playing
    [SerializeField] private Color healingGlowColor = Color.cyan;
    [SerializeField] private AnimationCurve healingPulse = AnimationCurve.EaseInOut(0, 0.5f, 1, 1.5f);
    [SerializeField] private float pulseSpeed = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip healingSound;
    [SerializeField] private AudioClip depletedSound;
    [SerializeField] private bool loopHealingSound = true;
    
    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    
    private AudioSource audioSource;
    private bool isPlayerInRange = false;
    private bool isActive = true;
    private bool isHealing = false;
    private LightResourceHealth playerHealth;
    private Coroutine healingCoroutine;
    private Coroutine rechargeCoroutine;
    private float baseLightIntensity;
    private float baseLightRange;
    
    // Visual state tracking
    private SpriteRenderer spriteRenderer;
    private Color originalSpriteColor;
    
    void Awake()
    {
        SetupComponents();
        InitializeVisuals();
    }
    
    void Start()
    {
        // Start ambient effects
        if (ambientEffect && isActive)
        {
            ambientEffect.Play();
        }
        
        // Hide interaction prompt initially
        if (interactionPrompt)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    void Update()
    {
        HandleInteraction();
        UpdateVisuals();
    }
    
    void SetupComponents()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f;
        }
        
        // Setup sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
        }
        
        // Setup collider for detection
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            ((CircleCollider2D)col).radius = activationRadius;
        }
        col.isTrigger = true;
    }
    
    void InitializeVisuals()
    {
        // Store original light values
        if (restorationLight != null)
        {
            baseLightIntensity = restorationLight.intensity;
            baseLightRange = restorationLight.range;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<LightResourceHealth>();
            if (playerHealth != null)
            {
                isPlayerInRange = true;
                
                // Show interaction prompt if needed
                if (requiresInteraction && interactionPrompt)
                {
                    interactionPrompt.SetActive(true);
                }
                
                // Auto-activate if enabled and restoration source is active
                if (autoActivate && isActive && !requiresInteraction)
                {
                    StartHealing();
                }
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            StopHealing();
            
            // Hide interaction prompt
            if (interactionPrompt)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    void HandleInteraction()
    {
        if (requiresInteraction && isPlayerInRange && isActive)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                if (!isHealing)
                {
                    StartHealing();
                }
            }
        }
    }
    
    /// <summary>
    /// Start the healing process
    /// </summary>
    void StartHealing()
    {
        if (!isActive || isHealing || playerHealth == null)
            return;
        
        isHealing = true;
        
        // Play activation sound
        if (audioSource && activationSound)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // Start healing effects
        if (healingEffect)
        {
            healingEffect.Play();
        }
        
        // Start healing coroutine
        if (isContinuousHealing)
        {
            healingCoroutine = StartCoroutine(ContinuousHealingCoroutine());
        }
        else
        {
            // Instant healing
            playerHealth.RestoreLightResource(restorationAmount);
            
            // Play healing sound
            if (audioSource && healingSound)
            {
                audioSource.PlayOneShot(healingSound);
            }
            
            // Check if consumed
            if (consumeOnUse)
            {
                ConsumeSource();
            }
            
            isHealing = false;
        }
    }
    
    /// <summary>
    /// Stop the healing process
    /// </summary>
    void StopHealing()
    {
        if (!isHealing)
            return;
        
        isHealing = false;
        
        // Stop healing coroutine
        if (healingCoroutine != null)
        {
            StopCoroutine(healingCoroutine);
            healingCoroutine = null;
        }
        
        // Stop healing effects
        if (healingEffect && healingEffect.isPlaying)
        {
            healingEffect.Stop();
        }
        
        // Stop healing audio
        if (audioSource && audioSource.isPlaying && audioSource.loop)
        {
            audioSource.Stop();
        }
    }
    
    /// <summary>
    /// Continuous healing coroutine
    /// </summary>
    System.Collections.IEnumerator ContinuousHealingCoroutine()
    {
        // Start healing sound
        if (audioSource && healingSound && loopHealingSound)
        {
            audioSource.clip = healingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        float healingTime = 0f;
        
        while (isPlayerInRange && isHealing && isActive)
        {
            // Check if player is at full health
            if (playerHealth.LightResourcePercent >= 1f)
            {
                // Stop healing but continue effects for a moment
                yield return new WaitForSeconds(1f);
                break;
            }
            
            // Apply healing
            float healingThisTick = restorationPerSecond * Time.deltaTime;
            playerHealth.RestoreLightResource(healingThisTick);
            
            healingTime += Time.deltaTime;
            
            yield return null;
        }
        
        // Check if source should be consumed
        if (consumeOnUse && healingTime > 0.5f) // Only consume if actually healed for a bit
        {
            ConsumeSource();
        }
        
        StopHealing();
    }
    
    /// <summary>
    /// Consume the restoration source (make it temporarily unavailable)
    /// </summary>
    void ConsumeSource()
    {
        isActive = false;
        
        // Play depletion sound
        if (audioSource && depletedSound)
        {
            audioSource.PlayOneShot(depletedSound);
        }
        
        // Stop ambient effects
        if (ambientEffect)
        {
            ambientEffect.Stop();
        }
        
        // Dim visuals
        if (restorationLight)
        {
            restorationLight.intensity = baseLightIntensity * 0.2f;
        }
        
        if (spriteRenderer)
        {
            spriteRenderer.color = originalSpriteColor * 0.5f;
        }
        
        // Start recharge timer
        if (rechargeTime > 0)
        {
            rechargeCoroutine = StartCoroutine(RechargeCoroutine());
        }
    }
    
    /// <summary>
    /// Recharge the restoration source after consumption
    /// </summary>
    System.Collections.IEnumerator RechargeCoroutine()
    {
        yield return new WaitForSeconds(rechargeTime);
        
        // Reactivate source
        isActive = true;
        
        // Restore visuals
        if (restorationLight)
        {
            restorationLight.intensity = baseLightIntensity;
        }
        
        if (spriteRenderer)
        {
            spriteRenderer.color = originalSpriteColor;
        }
        
        // Restart ambient effects
        if (ambientEffect)
        {
            ambientEffect.Play();
        }
        
        // Play activation sound
        if (audioSource && activationSound)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        rechargeCoroutine = null;
    }
    
    /// <summary>
    /// Update visual effects
    /// </summary>
    void UpdateVisuals()
    {
        if (!isActive)
            return;
        
        // Pulse the light intensity
        if (restorationLight)
        {
            float pulseValue = healingPulse.Evaluate((Time.time * pulseSpeed) % 1f);
            restorationLight.intensity = baseLightIntensity * pulseValue;
            
            // Intensify during healing
            if (isHealing)
            {
                restorationLight.intensity *= 1.5f;
                restorationLight.range = baseLightRange * 1.2f;
            }
            else
            {
                restorationLight.range = baseLightRange;
            }
        }
        
        // Pulse sprite color
        if (spriteRenderer && isActive)
        {
            float pulseValue = healingPulse.Evaluate((Time.time * pulseSpeed * 0.5f) % 1f);
            Color pulseColor = originalSpriteColor;
            
            if (isHealing)
            {
                pulseColor = Color.Lerp(originalSpriteColor, healingGlowColor, pulseValue * 0.5f);
            }
            
            spriteRenderer.color = pulseColor;
        }
    }
    
    /// <summary>
    /// Manually trigger restoration (for scripted events)
    /// </summary>
    public void TriggerRestoration(LightResourceHealth targetHealth)
    {
        if (targetHealth != null && isActive)
        {
            targetHealth.RestoreLightResource(restorationAmount);
            
            if (audioSource && healingSound)
            {
                audioSource.PlayOneShot(healingSound);
            }
            
            if (healingEffect)
            {
                healingEffect.Play();
            }
            
            if (consumeOnUse)
            {
                ConsumeSource();
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw activation radius
        Gizmos.color = healingGlowColor;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
        
        // Draw healing indicator
        if (isActive)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawIcon(transform.position, "💚", true);
        }
        else
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawIcon(transform.position, "🔋", true);
        }
    }
    
    // Public properties for external systems
    public bool IsActive => isActive;
    public bool IsHealing => isHealing;
    public float RestorationAmount => restorationAmount;
    public float RestorationPerSecond => restorationPerSecond;
}