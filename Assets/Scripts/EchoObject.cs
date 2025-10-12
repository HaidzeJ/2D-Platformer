using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for objects that can be revealed/hidden by the Echo Pulse system.
/// Inherit from this to create specific echo object types like bridges, doors, switches, etc.
/// </summary>
public abstract class EchoObject : MonoBehaviour
{
    [Header("Echo Object Settings")]
    [SerializeField] protected float defaultRevealDuration = 2.5f;
    [SerializeField] protected bool startVisible = false;
    [SerializeField] protected bool canToggle = false; // If true, each echo pulse toggles visibility
    
    [Header("Visual Feedback")]
    [SerializeField] protected Color hiddenColor = new Color(1f, 1f, 1f, 0.1f); // Almost transparent
    [SerializeField] protected Color revealedColor = Color.white; // Fully visible
    [SerializeField] protected Color echoGlow = new Color(0.3f, 0.8f, 1f, 0.8f); // Cyan glow when revealed
    
    // State tracking
    protected bool isRevealed;
    protected Coroutine hideCoroutine;
    protected SpriteRenderer spriteRenderer;
    protected Collider2D objectCollider;
    
    // Visual components
    protected Color originalColor;
    protected bool hasCollider;
    protected bool originalIsTrigger; // Store original trigger state
    
    protected virtual void Awake()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<Collider2D>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        hasCollider = (objectCollider != null);
        if (hasCollider)
        {
            originalIsTrigger = objectCollider.isTrigger;
        }
        
        // Set initial state
        isRevealed = startVisible;
        UpdateVisibility(isRevealed, false); // false = no animation on start
        
        Debug.Log($"🔍 EchoObject '{name}' initialized - Start Visible: {startVisible}, Can Toggle: {canToggle}");
    }
    
    /// <summary>
    /// Called by the Echo Pulse system to reveal this object
    /// </summary>
    public virtual void RevealObject(float duration = -1f)
    {
        if (duration < 0f) duration = defaultRevealDuration;
        
        if (canToggle)
        {
            // Toggle mode - switch visibility state
            ToggleVisibility(duration);
        }
        else
        {
            // Reveal mode - always show when pulsed
            ShowObject(duration);
        }
        
        // Call custom reveal behavior
        OnEchoRevealed(duration);
    }
    
    /// <summary>
    /// Show the object for a specified duration
    /// </summary>
    protected virtual void ShowObject(float duration)
    {
        // Stop any existing hide coroutine
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        
        // Update state and visuals
        isRevealed = true;
        UpdateVisibility(true, true);
        
        // Start hide timer if duration is positive
        if (duration > 0f)
        {
            hideCoroutine = StartCoroutine(HideAfterDelay(duration));
        }
    }
    
    /// <summary>
    /// Toggle visibility state
    /// </summary>
    protected virtual void ToggleVisibility(float duration)
    {
        isRevealed = !isRevealed;
        UpdateVisibility(isRevealed, true);
        
        // If now revealed and has duration, start hide timer
        if (isRevealed && duration > 0f)
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
            hideCoroutine = StartCoroutine(HideAfterDelay(duration));
        }
    }
    
    /// <summary>
    /// Hide the object immediately
    /// </summary>
    protected virtual void HideObject()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        
        isRevealed = false;
        UpdateVisibility(false, true);
    }
    
    /// <summary>
    /// Update the visual appearance and collision based on visibility state
    /// </summary>
    protected virtual void UpdateVisibility(bool visible, bool animate = false)
    {
        if (spriteRenderer != null)
        {
            Color targetColor = visible ? 
                (animate ? echoGlow : revealedColor) : 
                hiddenColor;
            
            if (animate && visible)
            {
                // Quick glow effect when revealed
                StartCoroutine(AnimateRevealGlow());
            }
            else
            {
                spriteRenderer.color = targetColor;
            }
        }
        
        // Update collision - manage interaction behavior while keeping detection possible
        if (hasCollider)
        {
            if (visible)
            {
                // When visible, restore original functionality
                objectCollider.isTrigger = originalIsTrigger;
            }
            else
            {
                // When hidden, make it a trigger so player passes through but echo pulse can still detect it
                objectCollider.isTrigger = true;
            }
            // Always keep collider enabled for echo pulse detection
            objectCollider.enabled = true;
        }
        
        // Update any child renderers
        UpdateChildVisibility(visible);
    }
    
    /// <summary>
    /// Update visibility of child objects (for complex echo objects)
    /// </summary>
    protected virtual void UpdateChildVisibility(bool visible)
    {
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in childRenderers)
        {
            if (renderer != spriteRenderer) // Don't affect the main renderer again
            {
                Color childColor = visible ? revealedColor : hiddenColor;
                renderer.color = new Color(childColor.r, childColor.g, childColor.b, childColor.a);
            }
        }
        
        // Update child colliders
        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D childCollider in childColliders)
        {
            if (childCollider != objectCollider) // Don't affect main collider again
            {
                childCollider.enabled = visible;
            }
        }
    }
    
    /// <summary>
    /// Animate the glow effect when object is revealed
    /// </summary>
    protected virtual IEnumerator AnimateRevealGlow()
    {
        if (spriteRenderer == null) yield break;
        
        // Quick glow animation
        float glowDuration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < glowDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / glowDuration;
            
            // Interpolate from glow to normal revealed color
            Color currentColor = Color.Lerp(echoGlow, revealedColor, t);
            spriteRenderer.color = currentColor;
            
            yield return null;
        }
        
        spriteRenderer.color = revealedColor;
    }
    
    /// <summary>
    /// Coroutine to hide object after delay
    /// </summary>
    protected IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideObject();
        hideCoroutine = null;
    }
    
    /// <summary>
    /// Called when object is revealed by echo pulse - override for custom behavior
    /// </summary>
    protected virtual void OnEchoRevealed(float duration)
    {
        // Override in derived classes for specific behavior
        // Examples: play sound, trigger animation, activate mechanism, etc.
    }
    
    /// <summary>
    /// Called when object is hidden - override for custom behavior
    /// </summary>
    protected virtual void OnEchoHidden()
    {
        // Override in derived classes for specific behavior
    }
    
    /// <summary>
    /// Public method to check if object is currently revealed
    /// </summary>
    public bool IsRevealed => isRevealed;
    
    /// <summary>
    /// Public method to manually show/hide object (for level scripting)
    /// </summary>
    public void SetVisibility(bool visible, float duration = 0f)
    {
        if (visible)
        {
            ShowObject(duration);
        }
        else
        {
            HideObject();
        }
    }
}