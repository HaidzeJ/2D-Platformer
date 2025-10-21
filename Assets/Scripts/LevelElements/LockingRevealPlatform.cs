using UnityEngine;
using System.Collections;

/// <summary>
/// A platform that starts hidden and permanently appears when the player passes over its trigger area.
/// Acts like a hidden locking gate that reveals and locks in place once activated.
/// Does not respond to echo pulses - only player contact triggers it.
/// </summary>
public class LockingRevealPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private bool startVisible = false;
    [SerializeField] private LayerMask playerLayer = -1;
    
    [Header("Tutorial Settings")]
    [SerializeField] private bool isTutorialPlatform = false;
    
    [Header("Visual Settings")]
    [SerializeField] private Color hiddenColor = new Color(1f, 1f, 1f, 0.1f);
    [SerializeField] private Color revealColor = Color.white;
    [SerializeField] private Color lockGlowColor = new Color(0.2f, 1f, 0.2f, 1f); // Green glow when locking
    [SerializeField] private float revealAnimationDuration = 1f;
    [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioClip revealSound;
    [SerializeField] private AudioClip lockSound;
    [SerializeField] private float soundVolume = 0.7f;
    
    [Header("Effects")]
    [SerializeField] private GameObject revealEffect;
    [SerializeField] private GameObject lockEffect;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private Collider2D triggerCollider;
    private AudioSource audioSource;
    
    // State
    private bool isRevealed = false;
    private bool isLocked = false;
    private bool isAnimating = false;
    
    // Player reference for control management
    private PlayerMovement playerMovement;
    
    void Awake()
    {
        SetupComponents();
        SetupTrigger();
        SetInitialState();
        SetupPlayerReference();
    }
    
    void SetupComponents()
    {
        // Get main components
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
        
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (revealSound != null || lockSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
        }
    }
    
    void SetupTrigger()
    {
        // Create a trigger collider for detection
        GameObject triggerObject = new GameObject($"{name}_Trigger");
        triggerObject.transform.SetParent(transform);
        triggerObject.transform.localPosition = Vector3.zero;
        
        // Copy the main collider but make it a trigger
        if (platformCollider != null)
        {
            if (platformCollider is BoxCollider2D boxCol)
            {
                BoxCollider2D triggerBox = triggerObject.AddComponent<BoxCollider2D>();
                triggerBox.size = boxCol.size;
                triggerBox.offset = boxCol.offset;
                triggerBox.isTrigger = true;
            }
            else if (platformCollider is CircleCollider2D circleCol)
            {
                CircleCollider2D triggerCircle = triggerObject.AddComponent<CircleCollider2D>();
                triggerCircle.radius = circleCol.radius;
                triggerCircle.offset = circleCol.offset;
                triggerCircle.isTrigger = true;
            }
            else
            {
                // Fallback: create a box trigger
                BoxCollider2D triggerBox = triggerObject.AddComponent<BoxCollider2D>();
                triggerBox.size = Vector2.one;
                triggerBox.isTrigger = true;
            }
        }
        else
        {
            // No main collider found, create default trigger
            BoxCollider2D triggerBox = triggerObject.AddComponent<BoxCollider2D>();
            triggerBox.size = Vector2.one;
            triggerBox.isTrigger = true;
        }
        
        // Add the trigger handler component
        LockingRevealTrigger triggerHandler = triggerObject.AddComponent<LockingRevealTrigger>();
        triggerHandler.Initialize(this, playerLayer);
        
        triggerCollider = triggerObject.GetComponent<Collider2D>();
    }
    
    void SetInitialState()
    {
        isRevealed = startVisible;
        isLocked = startVisible; // If it starts visible, consider it locked
        
        UpdateVisibility(isRevealed, false);
        
        Debug.Log($"🔒 LockingRevealPlatform '{name}' initialized - Start Visible: {startVisible}");
    }
    
    void SetupPlayerReference()
    {
        // Find the player for notification purposes
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogWarning($"LockingRevealPlatform '{name}': Player not found!");
        }
    }
    
    /// <summary>
    /// Called by the trigger when player enters the area
    /// </summary>
    public void OnPlayerEnter()
    {
        if (isLocked || isAnimating) return;
        
        // Notify player if this is a tutorial platform
        if (isTutorialPlatform && playerMovement != null)
        {
            playerMovement.OnTutorialPlatformActivated();
            Debug.Log($"� Tutorial platform '{name}' notified player");
        }
        
        Debug.Log($"🔓 Player activated LockingRevealPlatform '{name}' - Revealing and locking...");
        
        StartCoroutine(RevealAndLock());
    }
    
    IEnumerator RevealAndLock()
    {
        isAnimating = true;
        
        // Play reveal sound
        if (audioSource != null && revealSound != null)
        {
            audioSource.PlayOneShot(revealSound);
        }
        
        // Spawn reveal effect
        if (revealEffect != null)
        {
            GameObject effect = Instantiate(revealEffect, transform.position, transform.rotation);
            Destroy(effect, 3f);
        }
        
        // Animate reveal
        yield return StartCoroutine(AnimateReveal());
        
        // Lock the platform
        isLocked = true;
        isRevealed = true;
        
        // Play lock sound
        if (audioSource != null && lockSound != null)
        {
            audioSource.PlayOneShot(lockSound);
        }
        
        // Spawn lock effect
        if (lockEffect != null)
        {
            GameObject effect = Instantiate(lockEffect, transform.position, transform.rotation);
            Destroy(effect, 3f);
        }
        
        // Brief lock glow effect
        yield return StartCoroutine(AnimateLockGlow());
        
        // Tutorial notification is handled in OnPlayerEnter(), not here
        
        isAnimating = false;
        
        Debug.Log($"🔒 LockingRevealPlatform '{name}' is now permanently revealed and locked!");
    }
    
    IEnumerator AnimateReveal()
    {
        float elapsedTime = 0f;
        Color startColor = spriteRenderer != null ? spriteRenderer.color : hiddenColor;
        
        while (elapsedTime < revealAnimationDuration)
        {
            float progress = elapsedTime / revealAnimationDuration;
            float curvedProgress = revealCurve.Evaluate(progress);
            
            // Animate color
            if (spriteRenderer != null)
            {
                Color currentColor = Color.Lerp(startColor, revealColor, curvedProgress);
                spriteRenderer.color = currentColor;
            }
            
            // Enable collision partway through animation
            if (progress > 0.5f && platformCollider != null)
            {
                platformCollider.enabled = true;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final state
        UpdateVisibility(true, false);
    }
    
    IEnumerator AnimateLockGlow()
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = spriteRenderer.color;
        float glowDuration = 0.5f;
        float elapsedTime = 0f;
        
        // Glow to lock color
        while (elapsedTime < glowDuration * 0.5f)
        {
            float progress = elapsedTime / (glowDuration * 0.5f);
            spriteRenderer.color = Color.Lerp(originalColor, lockGlowColor, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Fade back to normal
        elapsedTime = 0f;
        while (elapsedTime < glowDuration * 0.5f)
        {
            float progress = elapsedTime / (glowDuration * 0.5f);
            spriteRenderer.color = Color.Lerp(lockGlowColor, revealColor, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        spriteRenderer.color = revealColor;
    }
    
    void UpdateVisibility(bool visible, bool animate = false)
    {
        // Update sprite color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = visible ? revealColor : hiddenColor;
        }
        
        // Update collision
        if (platformCollider != null)
        {
            if (visible)
            {
                // Platform is solid when visible
                platformCollider.enabled = true;
                platformCollider.isTrigger = false;
            }
            else
            {
                // Platform is passable when hidden (but trigger still works)
                platformCollider.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Check if the platform is currently revealed and locked
    /// </summary>
    public bool IsLocked => isLocked;
    
    /// <summary>
    /// Check if the platform is currently visible
    /// </summary>
    public bool IsRevealed => isRevealed;
    
    /// <summary>
    /// Manually trigger the reveal (useful for scripted events)
    /// </summary>
    public void ForceReveal()
    {
        if (!isLocked && !isAnimating)
        {
            StartCoroutine(RevealAndLock());
        }
    }
    
    /// <summary>
    /// Reset the platform to hidden state (useful for level reset)
    /// </summary>
    public void ResetPlatform()
    {
        if (isAnimating) return;
        
        isRevealed = startVisible;
        isLocked = startVisible;
        UpdateVisibility(isRevealed, false);
        
        Debug.Log($"🔄 LockingRevealPlatform '{name}' reset to initial state");
    }
}

/// <summary>
/// Helper component for handling trigger detection
/// </summary>
public class LockingRevealTrigger : MonoBehaviour
{
    private LockingRevealPlatform parentPlatform;
    private LayerMask playerLayer;
    
    public void Initialize(LockingRevealPlatform platform, LayerMask layers)
    {
        parentPlatform = platform;
        playerLayer = layers;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (parentPlatform == null) return;
        
        // Check if the collider is on the player layer
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            parentPlatform.OnPlayerEnter();
        }
    }
}