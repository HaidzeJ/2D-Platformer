using UnityEngine;

/// <summary>
/// An invisible bridge that becomes solid when revealed by Echo Pulse.
/// Perfect for creating hidden pathways and secret routes.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class EchoBridge : EchoObject
{
    [Header("Bridge-Specific Settings")]
    [SerializeField] private bool isPermanentOnceRevealed = false; // Some bridges stay once found
    [SerializeField] private float fadeWarningDuration = 0.5f; // Warning before bridge disappears
    
    [Header("Bridge Visual Effects")]
    [SerializeField] private bool enableParticleEffect = true;
    [SerializeField] private ParticleSystem bridgeParticles; // Optional particle effect
    [SerializeField] private AudioClip bridgeRevealSound;
    [SerializeField] private AudioClip bridgeHideSound;
    
    private AudioSource audioSource;
    private bool hasBeenRevealed = false;
    private Coroutine fadeWarningCoroutine;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (bridgeRevealSound != null || bridgeHideSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f;
        }
        
        // If no particle system assigned, try to find one in children
        if (bridgeParticles == null && enableParticleEffect)
        {
            bridgeParticles = GetComponentInChildren<ParticleSystem>();
        }
        
        Debug.Log($"🌉 EchoBridge '{name}' initialized - Permanent: {isPermanentOnceRevealed}");
    }
    
    protected override void OnEchoRevealed(float duration)
    {
        base.OnEchoRevealed(duration);
        
        hasBeenRevealed = true;
        
        // Play reveal sound
        if (audioSource != null && bridgeRevealSound != null)
        {
            audioSource.PlayOneShot(bridgeRevealSound);
        }
        
        // Start particle effect
        if (bridgeParticles != null && enableParticleEffect)
        {
            bridgeParticles.Play();
        }
        
        Debug.Log($"🌉 Bridge '{name}' revealed with echo pulse!");
    }
    
    protected override void OnEchoHidden()
    {
        base.OnEchoHidden();
        
        // Don't hide if this bridge is permanent once revealed
        if (isPermanentOnceRevealed && hasBeenRevealed)
        {
            Debug.Log($"🌉 Bridge '{name}' remains visible (permanent after first reveal)");
            // Force back to revealed state
            isRevealed = true;
            UpdateVisibility(true, false);
            return;
        }
        
        // Play hide sound
        if (audioSource != null && bridgeHideSound != null)
        {
            audioSource.PlayOneShot(bridgeHideSound);
        }
        
        // Stop particle effect
        if (bridgeParticles != null)
        {
            bridgeParticles.Stop();
        }
    }
    
    protected override void ShowObject(float duration)
    {
        // If permanent bridge and already been revealed, ignore duration
        if (isPermanentOnceRevealed && hasBeenRevealed)
        {
            base.ShowObject(0f); // 0 duration = permanent
        }
        else
        {
            base.ShowObject(duration);
        }
        
        // Start fade warning if bridge will disappear
        if (duration > 0f && !isPermanentOnceRevealed)
        {
            // Stop any existing fade warning
            if (fadeWarningCoroutine != null)
            {
                StopCoroutine(fadeWarningCoroutine);
            }
            fadeWarningCoroutine = StartCoroutine(FadeWarning(duration));
        }
    }
    
    /// <summary>
    /// Give visual warning before bridge disappears
    /// </summary>
    private System.Collections.IEnumerator FadeWarning(float totalDuration)
    {
        // Wait until close to disappearing
        float warningTime = totalDuration - fadeWarningDuration;
        if (warningTime > 0f)
        {
            yield return new WaitForSeconds(warningTime);
        }
        
        // Flash warning if still revealed - fade between visible and transparent
        if (isRevealed && spriteRenderer != null)
        {
            float flashDuration = 0.15f; // Slightly slower for better visibility
            int flashCount = Mathf.FloorToInt(fadeWarningDuration / (flashDuration * 2));
            Color currentColor = spriteRenderer.color;
            Color transparentColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
            
            for (int i = 0; i < flashCount && isRevealed; i++)
            {
                // Flash to transparent
                spriteRenderer.color = transparentColor;
                yield return new WaitForSeconds(flashDuration);
                
                // Back to visible if still revealed
                if (isRevealed)
                {
                    spriteRenderer.color = currentColor;
                    yield return new WaitForSeconds(flashDuration);
                }
            }
        }
        
        fadeWarningCoroutine = null; // Clear reference when completed
    }
    
    /// <summary>
    /// Override hide behavior to stop fade warning
    /// </summary>
    protected override void HideObject()
    {
        // Stop any active fade warning
        if (fadeWarningCoroutine != null)
        {
            StopCoroutine(fadeWarningCoroutine);
            fadeWarningCoroutine = null;
        }
        
        base.HideObject();
    }
    
    /// <summary>
    /// Called when player steps on the bridge
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isRevealed)
        {
            // Play step sound effect
            if (audioSource != null && bridgeRevealSound != null)
            {
                audioSource.PlayOneShot(bridgeRevealSound, 0.3f); // Quieter step sound
            }
        }
    }
    
    /// <summary>
    /// Called when player leaves the bridge
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        // Player left bridge - could add effects here if needed
    }
    
    /// <summary>
    /// For level designers - make this bridge permanent
    /// </summary>
    [ContextMenu("Make Permanent")]
    public void MakePermanent()
    {
        isPermanentOnceRevealed = true;
        hasBeenRevealed = true;
        
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        
        ShowObject(0f); // Permanent visibility
    }
    
    /// <summary>
    /// Get bridge status for debugging
    /// </summary>
    public string GetBridgeStatus()
    {
        return $"Bridge '{name}': Revealed={isRevealed}, Permanent={isPermanentOnceRevealed}, HasBeenRevealed={hasBeenRevealed}";
    }
}