using UnityEngine;

/// <summary>
/// Light Damage Source - Objects that can drain the player's light resource.
/// Examples: Shadow zones, dark enemies, environmental hazards, etc.
/// </summary>
public class LightDamageSource : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damagePerSecond = 5f; // For continuous damage
    [SerializeField] private float damageInterval = 0.5f; // Time between damage ticks
    
    [Header("Damage Type")]
    [SerializeField] private DamageType damageType = DamageType.Contact;
    [SerializeField] private string damageSourceName = "Shadow";
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem damageEffect;
    [SerializeField] private Color damageGlowColor = Color.red;
    [SerializeField] private bool showDamageRadius = true;
    [SerializeField] private float damageRadius = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip continuousDamageSound;
    [SerializeField] private bool loopContinuousSound = true;
    
    private AudioSource audioSource;
    private bool isPlayerInRange = false;
    private LightResourceHealth playerHealth;
    private float lastDamageTime = -999f;
    private Coroutine continuousDamageCoroutine;
    
    public enum DamageType
    {
        Contact,        // Instant damage on touch
        Proximity,      // Damage when near (uses damageRadius)
        Continuous,     // Ongoing damage while in contact/proximity
        Periodic        // Damage at intervals while in range
    }
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.6f;
        }
        
        // Setup collider for damage detection
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            ((CircleCollider2D)col).radius = damageRadius;
        }
        col.isTrigger = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<LightResourceHealth>();
            if (playerHealth != null)
            {
                isPlayerInRange = true;
                
                switch (damageType)
                {
                    case DamageType.Contact:
                        DealInstantDamage();
                        break;
                        
                    case DamageType.Proximity:
                        DealInstantDamage();
                        break;
                        
                    case DamageType.Continuous:
                        StartContinuousDamage();
                        break;
                        
                    case DamageType.Periodic:
                        StartPeriodicDamage();
                        break;
                }
                
                TriggerDamageEffects();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            StopAllDamage();
        }
    }
    
    /// <summary>
    /// Deal instant damage to the player
    /// </summary>
    void DealInstantDamage()
    {
        if (playerHealth != null && Time.time - lastDamageTime >= damageInterval)
        {
            playerHealth.TakeLightDamage(damageAmount, damageSourceName);
            lastDamageTime = Time.time;
            
            // Play damage sound
            if (audioSource && damageSound)
            {
                audioSource.PlayOneShot(damageSound);
            }
        }
    }
    
    /// <summary>
    /// Start continuous damage over time
    /// </summary>
    void StartContinuousDamage()
    {
        if (continuousDamageCoroutine == null)
        {
            continuousDamageCoroutine = StartCoroutine(ContinuousDamageCoroutine());
        }
        
        // Play continuous sound
        if (audioSource && continuousDamageSound && loopContinuousSound)
        {
            audioSource.clip = continuousDamageSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// Start periodic damage at intervals
    /// </summary>
    void StartPeriodicDamage()
    {
        if (continuousDamageCoroutine == null)
        {
            continuousDamageCoroutine = StartCoroutine(PeriodicDamageCoroutine());
        }
    }
    
    /// <summary>
    /// Continuous damage coroutine
    /// </summary>
    System.Collections.IEnumerator ContinuousDamageCoroutine()
    {
        while (isPlayerInRange && playerHealth != null)
        {
            playerHealth.TakeLightDamage(damagePerSecond * Time.deltaTime, $"{damageSourceName} (Continuous)");
            yield return null;
        }
        
        StopAllDamage();
    }
    
    /// <summary>
    /// Periodic damage coroutine
    /// </summary>
    System.Collections.IEnumerator PeriodicDamageCoroutine()
    {
        while (isPlayerInRange && playerHealth != null)
        {
            playerHealth.TakeLightDamage(damageAmount, $"{damageSourceName} (Periodic)");
            
            // Play damage sound
            if (audioSource && damageSound)
            {
                audioSource.PlayOneShot(damageSound);
            }
            
            yield return new WaitForSeconds(damageInterval);
        }
        
        StopAllDamage();
    }
    
    /// <summary>
    /// Stop all damage effects
    /// </summary>
    void StopAllDamage()
    {
        if (continuousDamageCoroutine != null)
        {
            StopCoroutine(continuousDamageCoroutine);
            continuousDamageCoroutine = null;
        }
        
        // Stop continuous audio
        if (audioSource && audioSource.isPlaying && audioSource.loop)
        {
            audioSource.Stop();
        }
        
        // Stop damage effects
        if (damageEffect && damageEffect.isPlaying)
        {
            damageEffect.Stop();
        }
    }
    
    /// <summary>
    /// Trigger visual and audio damage effects
    /// </summary>
    void TriggerDamageEffects()
    {
        // Start particle effect
        if (damageEffect && !damageEffect.isPlaying)
        {
            damageEffect.Play();
        }
        
        // Could add screen shake, color flash, etc. here
    }
    
    /// <summary>
    /// Manually trigger damage (for scripted events)
    /// </summary>
    public void TriggerDamage(LightResourceHealth targetHealth)
    {
        if (targetHealth != null)
        {
            targetHealth.TakeLightDamage(damageAmount, damageSourceName);
            
            if (audioSource && damageSound)
            {
                audioSource.PlayOneShot(damageSound);
            }
            
            TriggerDamageEffects();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw damage radius
        if (showDamageRadius)
        {
            Gizmos.color = damageGlowColor;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
            
            // Draw damage type indicator
            Gizmos.color = Color.red;
            Gizmos.DrawIcon(transform.position, "🔥", true);
        }
    }
    
    // Public getters for configuration
    public float DamageAmount => damageAmount;
    public float DamagePerSecond => damagePerSecond;
    public DamageType GetDamageType => damageType;
    public string DamageSourceName => damageSourceName;
}