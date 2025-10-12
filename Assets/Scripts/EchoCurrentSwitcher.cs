using UnityEngine;

/// <summary>
/// An echo-revealed switch that can toggle Light Currents on/off.
/// Creates dynamic environmental puzzles where players must use echo pulse to find switches.
/// </summary>
public class EchoCurrentSwitcher : EchoObject
{
    [Header("Current Switcher Settings")]
    [SerializeField] private LightCurrent[] targetCurrents; // Currents to control
    [SerializeField] private bool togglesCurrentsOnReveal = true; // Auto-toggle when revealed
    [SerializeField] private bool requiresPlayerActivation = false; // Needs player to touch/activate
    [SerializeField] private float switchCooldown = 1f; // Prevent rapid toggling
    
    [Header("Switch Visual Feedback")]
    [SerializeField] private Color activeSwitchColor = Color.green;
    [SerializeField] private Color inactiveSwitchColor = Color.red;
    [SerializeField] private SpriteRenderer switchIndicator; // Optional separate indicator sprite
    
    [Header("Switch Audio")]
    [SerializeField] private AudioClip switchActivateSound;
    [SerializeField] private AudioClip switchDeactivateSound;
    [SerializeField] private AudioClip switchRevealSound;
    
    private bool switchIsActive = false;
    private float lastSwitchTime = -10f;
    private AudioSource audioSource;
    private bool playerInRange = false;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.8f;
        }
        
        // If no switch indicator assigned, try to find one
        if (switchIndicator == null)
        {
            switchIndicator = transform.Find("SwitchIndicator")?.GetComponent<SpriteRenderer>();
        }
        
        // Initialize switch state
        UpdateSwitchVisuals();
        
        Debug.Log($"🔌 EchoCurrentSwitcher '{name}' initialized - Auto-toggle: {togglesCurrentsOnReveal}, Requires Activation: {requiresPlayerActivation}");
    }
    
    protected override void OnEchoRevealed(float duration)
    {
        base.OnEchoRevealed(duration);
        
        // Play reveal sound
        if (audioSource != null && switchRevealSound != null)
        {
            audioSource.PlayOneShot(switchRevealSound);
        }
        
        // Auto-toggle currents if enabled and not requiring player activation
        if (togglesCurrentsOnReveal && !requiresPlayerActivation)
        {
            ToggleCurrents();
        }
        
        Debug.Log($"🔌 Switch '{name}' revealed by echo pulse!");
    }
    
    /// <summary>
    /// Toggle the connected light currents
    /// </summary>
    public void ToggleCurrents()
    {
        // Check cooldown
        if (Time.time - lastSwitchTime < switchCooldown)
        {
            return;
        }
        
        lastSwitchTime = Time.time;
        switchIsActive = !switchIsActive;
        
        // Apply to all target currents
        foreach (LightCurrent current in targetCurrents)
        {
            if (current != null)
            {
                current.SetCurrentActive(switchIsActive);
            }
        }
        
        // Update visuals and audio
        UpdateSwitchVisuals();
        PlaySwitchSound();
    }
    
    /// <summary>
    /// Update visual appearance based on switch state
    /// </summary>
    private void UpdateSwitchVisuals()
    {
        Color switchColor = switchIsActive ? activeSwitchColor : inactiveSwitchColor;
        
        // Update main sprite color
        if (spriteRenderer != null && isRevealed)
        {
            spriteRenderer.color = Color.Lerp(switchColor, echoGlow, 0.3f);
        }
        
        // Update indicator sprite if available
        if (switchIndicator != null)
        {
            switchIndicator.color = switchColor;
            switchIndicator.enabled = isRevealed; // Only show when switch is revealed
        }
    }
    
    /// <summary>
    /// Play appropriate switch sound
    /// </summary>
    private void PlaySwitchSound()
    {
        if (audioSource == null) return;
        
        AudioClip soundToPlay = switchIsActive ? switchActivateSound : switchDeactivateSound;
        if (soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }
    }
    
    /// <summary>
    /// Override visibility update to handle switch-specific visuals
    /// </summary>
    protected override void UpdateVisibility(bool visible, bool animate = false)
    {
        base.UpdateVisibility(visible, animate);
        UpdateSwitchVisuals();
    }
    
    /// <summary>
    /// Handle player entering switch area (for activation-required switches)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isRevealed)
        {
            playerInRange = true;
            
            // Auto-activate if requiring player activation
            if (requiresPlayerActivation)
            {
                ToggleCurrents();
            }
        }
    }
    
    /// <summary>
    /// Handle player leaving switch area
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    
    /// <summary>
    /// Manual activation method (can be called by other scripts or UI)
    /// </summary>
    public void ActivateSwitch()
    {
        if (isRevealed)
        {
            ToggleCurrents();
        }
    }
    
    /// <summary>
    /// Set specific current states (for level scripting)
    /// </summary>
    public void SetCurrentStates(bool active)
    {
        switchIsActive = active;
        
        foreach (LightCurrent current in targetCurrents)
        {
            if (current != null)
            {
                current.SetCurrentActive(active);
            }
        }
        
        UpdateSwitchVisuals();
    }
    
    /// <summary>
    /// Add a light current to the list of controlled currents
    /// </summary>
    public void AddTargetCurrent(LightCurrent current)
    {
        if (current == null) return;
        
        // Resize array and add new current
        LightCurrent[] newArray = new LightCurrent[targetCurrents.Length + 1];
        for (int i = 0; i < targetCurrents.Length; i++)
        {
            newArray[i] = targetCurrents[i];
        }
        newArray[targetCurrents.Length] = current;
        targetCurrents = newArray;
    }
    
    /// <summary>
    /// Get switch status for debugging
    /// </summary>
    public string GetSwitchStatus()
    {
        return $"Switch '{name}': Revealed={isRevealed}, Active={switchIsActive}, Controlling={targetCurrents.Length} currents, PlayerInRange={playerInRange}";
    }
    
    /// <summary>
    /// For level designers - test switch functionality
    /// </summary>
    [ContextMenu("Test Switch Toggle")]
    public void TestToggle()
    {
        if (!isRevealed)
        {
            RevealObject(5f); // Reveal for testing
        }
        ToggleCurrents();
    }
}