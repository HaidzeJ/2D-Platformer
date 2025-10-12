using UnityEngine;

/// <summary>
/// Simple light-draining element that can be added to any GameObject.
/// Continuously drains the player's light resource while they are in contact with it.
/// </summary>
public class LightDrainZone : MonoBehaviour
{
    [Header("Drain Settings")]
    [SerializeField] private float lightDrainRate = 8f; // Light drained per second
    [SerializeField] private string drainSourceName = "Dark Zone"; // Name shown in damage logs
    
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 2f; // How far the drain effect reaches
    [SerializeField] private LayerMask playerLayer = -1; // What layers count as player
    
    [Header("Visual Effects")]
    [SerializeField] private bool createVisualEffects = true;
    [SerializeField] private Color drainColor = new Color(0.2f, 0.1f, 0.3f, 0.7f);
    [SerializeField] private bool addParticleEffect = true;
    [SerializeField] private bool addGlowEffect = false;
    
    [Header("Audio")]
    [SerializeField] private AudioClip drainSound;
    [SerializeField] private bool loopDrainSound = true;
    [SerializeField] private float drainVolume = 0.5f;
    
    private LightDamageSource damageSource;
    private bool isPlayerInRange = false;
    private AudioSource audioSource;
    
    void Awake()
    {
        SetupLightDrainZone();
    }
    
    void SetupLightDrainZone()
    {
        // Add LightDamageSource component if not already present
        damageSource = GetComponent<LightDamageSource>();
        if (damageSource == null)
        {
            damageSource = gameObject.AddComponent<LightDamageSource>();
        }
        
        // Configure the damage source using reflection (since fields are private)
        SetDamageSourceProperties();
        
        // Setup collider for detection
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
            circleCol.radius = detectionRadius;
            circleCol.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && drainSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = drainSound;
            audioSource.loop = loopDrainSound;
            audioSource.volume = drainVolume;
            audioSource.playOnAwake = false;
        }
        
        // Create visual effects if requested
        if (createVisualEffects)
        {
            CreateVisualEffects();
        }
    }
    
    void SetDamageSourceProperties()
    {
        // Use reflection to configure the LightDamageSource component
        var damagePerSecondField = typeof(LightDamageSource).GetField("damagePerSecond", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (damagePerSecondField != null)
            damagePerSecondField.SetValue(damageSource, lightDrainRate);
        
        var damageTypeField = typeof(LightDamageSource).GetField("damageType",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (damageTypeField != null)
            damageTypeField.SetValue(damageSource, LightDamageSource.DamageType.Continuous);
        
        var damageSourceNameField = typeof(LightDamageSource).GetField("damageSourceName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (damageSourceNameField != null)
            damageSourceNameField.SetValue(damageSource, drainSourceName);
        
        var radiusField = typeof(LightDamageSource).GetField("damageRadius",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (radiusField != null)
            radiusField.SetValue(damageSource, detectionRadius);
    }
    
    void CreateVisualEffects()
    {
        // Add sprite renderer if not present
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            // Create a simple circular sprite
            spriteRenderer.sprite = CreateDrainZoneSprite();
            spriteRenderer.color = drainColor;
        }
        
        // Add particle effect
        if (addParticleEffect)
        {
            CreateDrainParticles();
        }
        
        // Add glow effect (additional light component)
        if (addGlowEffect)
        {
            CreateGlowEffect();
        }
    }
    
    Sprite CreateDrainZoneSprite()
    {
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        Vector2 center = new Vector2(resolution * 0.5f, resolution * 0.5f);
        float radius = resolution * 0.4f;
        
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance < radius ? 
                    Mathf.Lerp(0.8f, 0f, distance / radius) : 0f;
                
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
    
    void CreateDrainParticles()
    {
        GameObject particleObj = new GameObject("Drain Particles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.startColor = new Color(drainColor.r, drainColor.g, drainColor.b, 1f);
        main.startSize = 0.1f;
        main.startSpeed = 0.5f;
        main.maxParticles = 20;
        main.startLifetime = 2f;
        
        var emission = ps.emission;
        emission.rateOverTime = 8f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = detectionRadius * 0.8f;
        
        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(-0.3f);
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(drainColor, 0.0f), 
                new GradientColorKey(Color.black, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;
    }
    
    void CreateGlowEffect()
    {
        Light glowLight = gameObject.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = drainColor;
        glowLight.intensity = 0.5f;
        glowLight.range = detectionRadius * 1.5f;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            isPlayerInRange = true;
            
            // Start drain sound
            if (audioSource && drainSound && loopDrainSound)
            {
                audioSource.Play();
            }
            
            Debug.Log($"Player entered {drainSourceName} - draining light at {lightDrainRate}/sec");
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            isPlayerInRange = false;
            
            // Stop drain sound
            if (audioSource && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            Debug.Log($"Player left {drainSourceName}");
        }
    }
    
    bool IsPlayer(Collider2D collider)
    {
        // Check by tag first
        if (collider.CompareTag("Player"))
            return true;
        
        // Check by layer mask
        return ((1 << collider.gameObject.layer) & playerLayer) != 0;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = drainColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw drain indicator
        Gizmos.color = Color.red;
        Gizmos.DrawIcon(transform.position, "⚡", true);
    }
    
    /// <summary>
    /// Manually trigger a light drain effect (for testing or scripted events)
    /// </summary>
    public void TriggerLightDrain()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            LightResourceHealth health = player.GetComponent<LightResourceHealth>();
            if (health != null)
            {
                health.TakeLightDamage(lightDrainRate, drainSourceName);
                Debug.Log($"Manually triggered light drain: -{lightDrainRate} from {drainSourceName}");
            }
        }
    }
    
    /// <summary>
    /// Update the drain rate at runtime
    /// </summary>
    public void SetDrainRate(float newRate)
    {
        lightDrainRate = newRate;
        SetDamageSourceProperties(); // Update the damage source
    }
    
    /// <summary>
    /// Update the detection radius at runtime
    /// </summary>
    public void SetDetectionRadius(float newRadius)
    {
        detectionRadius = newRadius;
        
        // Update collider
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.radius = newRadius;
        }
        
        SetDamageSourceProperties(); // Update the damage source
    }
    
    // Public properties for external access
    public float DrainRate => lightDrainRate;
    public float DetectionRadius => detectionRadius;
    public bool IsPlayerInRange => isPlayerInRange;
    public string DrainSourceName => drainSourceName;
}