using UnityEngine;

/// <summary>
/// Example platform that demonstrates the Hide mode functionality.
/// This platform will disappear when echo pulsed and reappear after a duration.
/// Perfect for creating temporary gaps or hidden passages.
/// </summary>
public class DisappearingPlatform : EchoObject
{
    [Header("Disappearing Platform Settings")]
    [SerializeField] private float hideGlowDuration = 0.3f;
    [SerializeField] private Color hideGlowColor = Color.red;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Configure for hiding behavior
        startVisible = true;           // Platform starts visible
        echoMode = EchoMode.Hide;      // Echo pulse will hide it
        defaultRevealDuration = 3f;     // Hidden for 3 seconds before reappearing
    }
    
    protected override void OnEchoRevealed(float duration)
    {
        base.OnEchoRevealed(duration);
        
        // Custom behavior when platform is about to disappear
        if (echoMode == EchoMode.Hide && isRevealed)
        {
            Debug.Log($"⚠️ Platform {name} is disappearing for {duration} seconds!");
            
            // You could add additional effects here:
            // - Play disappearing sound
            // - Particle effects
            // - Screen shake
            // - Warning flash
        }
    }
    
    protected override void UpdateVisibility(bool visible, bool animate = false)
    {
        // Use custom hide glow color for disappearing platforms
        if (spriteRenderer != null && !visible && animate)
        {
            // Show red warning glow when hiding
            StartCoroutine(AnimateHideGlow());
        }
        else
        {
            base.UpdateVisibility(visible, animate);
        }
    }
    
    private System.Collections.IEnumerator AnimateHideGlow()
    {
        if (spriteRenderer == null) yield break;
        
        Color startColor = spriteRenderer.color;
        float elapsedTime = 0f;
        
        // Flash to hide glow color
        while (elapsedTime < hideGlowDuration * 0.5f)
        {
            float progress = elapsedTime / (hideGlowDuration * 0.5f);
            spriteRenderer.color = Color.Lerp(startColor, hideGlowColor, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Fade to hidden
        elapsedTime = 0f;
        while (elapsedTime < hideGlowDuration * 0.5f)
        {
            float progress = elapsedTime / (hideGlowDuration * 0.5f);
            spriteRenderer.color = Color.Lerp(hideGlowColor, hiddenColor, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        spriteRenderer.color = hiddenColor;
    }
}