using UnityEngine;
using System.Collections;

/// <summary>
/// Light Resource Health System - Player's health is represented by their light intensity.
/// As health decreases, the player becomes dimmer and their surrounding effects fade.
/// Perfect for atmospheric gameplay where light = life.
/// </summary>
public class LightResourceHealth : MonoBehaviour
{
    [Header("Light Resource Settings")]
    [SerializeField] private float maxLightResource = 100f;
    [SerializeField] private float currentLightResource = 100f;
    [SerializeField] private float lowLightThreshold = 25f; // When to start warning effects
    [SerializeField] private float criticalLightThreshold = 10f; // Danger zone
    
    [Header("Visual Light Components")]
    [SerializeField] private Light playerLight; // Main light component
    [SerializeField] private SpriteRenderer playerSprite; // Player's sprite
    [SerializeField] private ParticleSystem lightParticles; // Light particle effect
    [SerializeField] private GameObject lightAura; // Optional aura effect
    
    [Header("Light Intensity Settings")]
    [SerializeField] private float maxLightIntensity = 2f;
    [SerializeField] private float minLightIntensity = 0.2f;
    [SerializeField] private float maxLightRange = 8f;
    [SerializeField] private float minLightRange = 2f;
    [SerializeField] private AnimationCurve lightFalloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    [Header("Sprite Brightness Settings")]
    [SerializeField] private Color maxBrightnessColor = Color.white;
    [SerializeField] private Color minBrightnessColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private float brightnessTransitionSpeed = 2f;
    
    [Header("Particle Effects")]
    [SerializeField] private int maxParticleCount = 50;
    [SerializeField] private int minParticleCount = 5;
    [SerializeField] private float maxParticleSpeed = 3f;
    [SerializeField] private float minParticleSpeed = 0.5f;
    
    [Header("Warning Effects")]
    [SerializeField] private bool enableLowLightWarning = true;
    [SerializeField] private float warningFlickerSpeed = 3f;
    [SerializeField] private bool enableCriticalWarning = true;
    [SerializeField] private float criticalFlickerSpeed = 8f;
    
    [Header("Resource Regeneration")]
    [SerializeField] private bool enableNaturalRegeneration = false;
    [SerializeField] private float regenerationRate = 1f; // Per second
    [SerializeField] private float regenerationDelay = 3f; // Seconds after taking damage
    
    [Header("Audio")]
    [SerializeField] private AudioClip lightDamageSound;
    [SerializeField] private AudioClip lightWarningSound;
    [SerializeField] private AudioClip lightCriticalSound;
    [SerializeField] private AudioClip lightExtinguishedSound;
    
    // Private variables
    private float targetLightIntensity;
    private float targetLightRange;
    private Color targetSpriteColor;
    private bool isDead = false;
    private float lastDamageTime;
    private AudioSource audioSource;
    private Coroutine warningCoroutine;
    private Coroutine regenerationCoroutine;
    
    // Components
    private PlayerMovement playerMovement;
    
    // Events
    public System.Action<float, float> OnLightResourceChanged; // current, max
    public System.Action OnLightExtinguished; // Death event
    public System.Action OnLightWarning; // Low light warning
    public System.Action OnLightCritical; // Critical light warning
    
    void Awake()
    {
        // Get components
        playerMovement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f;
        }
        
        // Auto-find light components if not assigned
        if (playerLight == null)
            playerLight = GetComponentInChildren<Light>();
        
        if (playerSprite == null)
            playerSprite = GetComponent<SpriteRenderer>();
        
        if (lightParticles == null)
            lightParticles = GetComponentInChildren<ParticleSystem>();
        
        // Initialize values
        currentLightResource = maxLightResource;
        UpdateLightEffects();
        
        Debug.Log($"💡 Light Resource Health initialized - Max: {maxLightResource}, Current: {currentLightResource}");
    }
    
    void Start()
    {
        // Start regeneration if enabled
        if (enableNaturalRegeneration)
        {
            regenerationCoroutine = StartCoroutine(RegenerateLightResource());
        }
    }
    
    void Update()
    {
        // Smoothly update visual effects
        UpdateVisualEffects();
        
        // Check for warning states
        CheckWarningStates();
    }
    
    /// <summary>
    /// Damage the player's light resource
    /// </summary>
    public void TakeLightDamage(float damage, string source = "Unknown")
    {
        if (isDead) return;
        
        currentLightResource = Mathf.Max(0f, currentLightResource - damage);
        lastDamageTime = Time.time;
        
        // Play damage sound
        if (audioSource && lightDamageSound)
        {
            audioSource.PlayOneShot(lightDamageSound);
        }
        
        // Update visual effects
        UpdateLightEffects();
        
        // Trigger events
        OnLightResourceChanged?.Invoke(currentLightResource, maxLightResource);
        
        Debug.Log($"💡 Light damage taken: {damage} from {source} - Remaining: {currentLightResource:F1}/{maxLightResource}");
        
        // Check for death
        if (currentLightResource <= 0f && !isDead)
        {
            ExtinguishLight();
        }
    }
    
    /// <summary>
    /// Restore light resource
    /// </summary>
    public void RestoreLightResource(float amount, string source = "Restoration")
    {
        if (isDead) return;
        
        float oldResource = currentLightResource;
        currentLightResource = Mathf.Min(maxLightResource, currentLightResource + amount);
        
        if (currentLightResource > oldResource)
        {
            UpdateLightEffects();
            OnLightResourceChanged?.Invoke(currentLightResource, maxLightResource);
            
            Debug.Log($"💡 Light restored: {amount} from {source} - Current: {currentLightResource:F1}/{maxLightResource}");
        }
    }
    
    /// <summary>
    /// Update all light-based visual effects based on current resource level
    /// </summary>
    void UpdateLightEffects()
    {
        float lightPercent = currentLightResource / maxLightResource;
        float curveValue = lightFalloffCurve.Evaluate(lightPercent);
        
        // Calculate target values
        targetLightIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, curveValue);
        targetLightRange = Mathf.Lerp(minLightRange, maxLightRange, curveValue);
        targetSpriteColor = Color.Lerp(minBrightnessColor, maxBrightnessColor, curveValue);
        
        // Update particle system if available
        if (lightParticles != null)
        {
            var main = lightParticles.main;
            var emission = lightParticles.emission;
            
            // Particle count
            int targetParticleCount = Mathf.RoundToInt(Mathf.Lerp(minParticleCount, maxParticleCount, lightPercent));
            emission.rateOverTime = targetParticleCount;
            
            // Particle speed
            float targetParticleSpeed = Mathf.Lerp(minParticleSpeed, maxParticleSpeed, lightPercent);
            main.startSpeed = targetParticleSpeed;
            
            // Particle color (fade to dimmer)
            Color particleColor = Color.Lerp(new Color(1f, 1f, 0.5f, 0.3f), Color.white, lightPercent);
            main.startColor = particleColor;
        }
        
        // Update aura effect
        if (lightAura != null)
        {
            lightAura.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.5f, curveValue);
            
            SpriteRenderer auraRenderer = lightAura.GetComponent<SpriteRenderer>();
            if (auraRenderer != null)
            {
                Color auraColor = auraRenderer.color;
                auraColor.a = Mathf.Lerp(0.1f, 0.5f, lightPercent);
                auraRenderer.color = auraColor;
            }
        }
    }
    
    /// <summary>
    /// Smoothly update visual effects each frame
    /// </summary>
    void UpdateVisualEffects()
    {
        // Update light component
        if (playerLight != null)
        {
            playerLight.intensity = Mathf.Lerp(playerLight.intensity, targetLightIntensity, brightnessTransitionSpeed * Time.deltaTime);
            playerLight.range = Mathf.Lerp(playerLight.range, targetLightRange, brightnessTransitionSpeed * Time.deltaTime);
        }
        
        // Update sprite color
        if (playerSprite != null)
        {
            playerSprite.color = Color.Lerp(playerSprite.color, targetSpriteColor, brightnessTransitionSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Check for warning states and trigger appropriate effects
    /// </summary>
    void CheckWarningStates()
    {
        float lightPercent = currentLightResource / maxLightResource;
        
        if (lightPercent <= (criticalLightThreshold / maxLightResource))
        {
            // Critical warning
            if (enableCriticalWarning && warningCoroutine == null)
            {
                warningCoroutine = StartCoroutine(CriticalLightWarning());
                OnLightCritical?.Invoke();
            }
        }
        else if (lightPercent <= (lowLightThreshold / maxLightResource))
        {
            // Low light warning
            if (enableLowLightWarning && warningCoroutine == null)
            {
                warningCoroutine = StartCoroutine(LowLightWarning());
                OnLightWarning?.Invoke();
            }
        }
        else
        {
            // Stop warning effects
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }
        }
    }
    
    /// <summary>
    /// Low light warning effect
    /// </summary>
    IEnumerator LowLightWarning()
    {
        if (audioSource && lightWarningSound)
        {
            audioSource.PlayOneShot(lightWarningSound);
        }
        
        while (currentLightResource > 0f && currentLightResource <= lowLightThreshold)
        {
            // Gentle flicker
            float flickerIntensity = 0.9f + 0.1f * Mathf.Sin(Time.time * warningFlickerSpeed);
            
            if (playerLight != null)
            {
                playerLight.intensity = targetLightIntensity * flickerIntensity;
            }
            
            yield return null;
        }
        
        warningCoroutine = null;
    }
    
    /// <summary>
    /// Critical light warning effect
    /// </summary>
    IEnumerator CriticalLightWarning()
    {
        if (audioSource && lightCriticalSound)
        {
            audioSource.PlayOneShot(lightCriticalSound);
        }
        
        while (currentLightResource > 0f && currentLightResource <= criticalLightThreshold)
        {
            // Intense flicker
            float flickerIntensity = 0.5f + 0.5f * Mathf.Sin(Time.time * criticalFlickerSpeed);
            
            if (playerLight != null)
            {
                playerLight.intensity = targetLightIntensity * flickerIntensity;
            }
            
            if (playerSprite != null)
            {
                Color flickerColor = Color.Lerp(targetSpriteColor, Color.red, 0.3f * (1f - flickerIntensity));
                playerSprite.color = flickerColor;
            }
            
            yield return null;
        }
        
        warningCoroutine = null;
    }
    
    /// <summary>
    /// Handle light extinguishing (death)
    /// </summary>
    void ExtinguishLight()
    {
        isDead = true;
        
        // Play death sound
        if (audioSource && lightExtinguishedSound)
        {
            audioSource.PlayOneShot(lightExtinguishedSound);
        }
        
        // Disable movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // Start death effect
        StartCoroutine(LightExtinguishEffect());
        
        // Trigger death event
        OnLightExtinguished?.Invoke();
        
        Debug.Log("💡 Light extinguished - Player has died");
    }
    
    /// <summary>
    /// Visual effect for light extinguishing
    /// </summary>
    IEnumerator LightExtinguishEffect()
    {
        float duration = 2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Fade all light effects to zero
            if (playerLight != null)
            {
                playerLight.intensity = Mathf.Lerp(targetLightIntensity, 0f, t);
                playerLight.range = Mathf.Lerp(targetLightRange, 0f, t);
            }
            
            if (playerSprite != null)
            {
                Color fadeColor = Color.Lerp(targetSpriteColor, Color.black, t);
                playerSprite.color = fadeColor;
            }
            
            if (lightParticles != null)
            {
                var emission = lightParticles.emission;
                emission.rateOverTime = Mathf.Lerp(minParticleCount, 0f, t);
            }
            
            yield return null;
        }
        
        // Completely disable light components
        if (playerLight != null) playerLight.enabled = false;
        if (lightParticles != null) lightParticles.Stop();
        if (lightAura != null) lightAura.SetActive(false);
    }
    
    /// <summary>
    /// Natural regeneration coroutine
    /// </summary>
    IEnumerator RegenerateLightResource()
    {
        while (!isDead)
        {
            // Wait for regeneration delay after damage
            if (Time.time - lastDamageTime >= regenerationDelay)
            {
                if (currentLightResource < maxLightResource)
                {
                    RestoreLightResource(regenerationRate * Time.deltaTime, "Natural Regeneration");
                }
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Respawn the player with full light
    /// </summary>
    public void RespawnWithFullLight()
    {
        isDead = false;
        currentLightResource = maxLightResource;
        
        // Re-enable components
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        if (playerLight != null) playerLight.enabled = true;
        if (lightParticles != null) lightParticles.Play();
        if (lightAura != null) lightAura.SetActive(true);
        
        // Stop any ongoing effects
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }
        
        // Update effects
        UpdateLightEffects();
        OnLightResourceChanged?.Invoke(currentLightResource, maxLightResource);
        
        Debug.Log("💡 Player respawned with full light resource");
    }
    
    // Public getters
    public float CurrentLightResource => currentLightResource;
    public float MaxLightResource => maxLightResource;
    public float LightResourcePercent => currentLightResource / maxLightResource;
    public bool IsLightExtinguished => isDead;
    public bool IsInLowLight => currentLightResource <= lowLightThreshold;
    public bool IsInCriticalLight => currentLightResource <= criticalLightThreshold;
}