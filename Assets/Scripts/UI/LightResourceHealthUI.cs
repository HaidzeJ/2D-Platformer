using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI display for Light Resource Health system.
/// Shows current light level, health percentage, and visual feedback for player status.
/// </summary>
public class LightResourceHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider lightHealthSlider;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image healthIcon;
    
    [Header("Visual Settings")]
    [SerializeField] private Color highHealthColor = Color.cyan;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.orange;
    [SerializeField] private Color criticalHealthColor = Color.red;
    [SerializeField] private AnimationCurve pulseAnimation = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
    [SerializeField] private float pulseSpeed = 2f;
    
    [Header("Warning Effects")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private float warningFlashSpeed = 3f;
    
    private LightResourceHealth playerHealth;
    private float lastHealthValue = -1f;
    
    void Start()
    {
        // Find the player's health component
        FindPlayerHealth();
        
        // Initialize UI elements
        SetupUI();
    }
    
    void Update()
    {
        if (playerHealth != null)
        {
            UpdateHealthDisplay();
            UpdateWarningEffects();
        }
        else
        {
            // Try to find player health again if we lost it
            FindPlayerHealth();
        }
    }
    
    void FindPlayerHealth()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<LightResourceHealth>();
            if (playerHealth != null)
            {
                // Subscribe to health events
                playerHealth.OnLightResourceChanged += OnHealthChanged;
                playerHealth.OnLightWarning += OnLowLightWarning;
                playerHealth.OnLightCritical += OnCriticalLightWarning;
                playerHealth.OnLightExtinguished += OnPlayerDeath;
            }
        }
    }
    
    void SetupUI()
    {
        // Hide warning panel initially
        if (warningPanel)
        {
            warningPanel.SetActive(false);
        }
        
        // Setup slider if available
        if (lightHealthSlider)
        {
            lightHealthSlider.minValue = 0f;
            lightHealthSlider.maxValue = 1f;
            lightHealthSlider.value = 1f;
        }
    }
    
    void UpdateHealthDisplay()
    {
        float healthPercent = playerHealth.LightResourcePercent;
        float currentHealth = playerHealth.CurrentLightResource;
        float maxHealth = playerHealth.MaxLightResource;
        
        // Update slider
        if (lightHealthSlider)
        {
            lightHealthSlider.value = healthPercent;
        }
        
        // Update health bar color based on percentage
        Color healthColor = GetHealthColor(healthPercent);
        if (healthFillImage)
        {
            healthFillImage.color = healthColor;
        }
        
        // Update text displays
        if (healthText)
        {
            healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
            healthText.color = healthColor;
        }
        
        if (statusText)
        {
            string status = GetHealthStatus(healthPercent);
            statusText.text = status;
            statusText.color = healthColor;
        }
        
        // Update health icon with pulse effect
        if (healthIcon && healthPercent < 0.5f)
        {
            float pulseValue = pulseAnimation.Evaluate((Time.time * pulseSpeed) % 1f);
            healthIcon.transform.localScale = Vector3.one * pulseValue;
            healthIcon.color = healthColor;
        }
        else if (healthIcon)
        {
            healthIcon.transform.localScale = Vector3.one;
            healthIcon.color = healthColor;
        }
    }
    
    void UpdateWarningEffects()
    {
        if (playerHealth == null) return;
        
        float healthPercent = playerHealth.LightResourcePercent;
        
        // Show/hide warning panel based on health
        bool shouldShowWarning = healthPercent <= 0.3f; // Show warning at 30% or below
        
        if (warningPanel && warningPanel.activeSelf != shouldShowWarning)
        {
            warningPanel.SetActive(shouldShowWarning);
        }
        
        // Animate warning panel if active
        if (warningPanel && warningPanel.activeSelf)
        {
            float flashValue = Mathf.Abs(Mathf.Sin(Time.time * warningFlashSpeed));
            CanvasGroup canvasGroup = warningPanel.GetComponent<CanvasGroup>();
            if (canvasGroup)
            {
                canvasGroup.alpha = Mathf.Lerp(0.3f, 1f, flashValue);
            }
            
            // Update warning text
            if (warningText)
            {
                if (healthPercent <= 0.1f)
                {
                    warningText.text = "CRITICAL - LIGHT FAILING!";
                    warningText.color = criticalHealthColor;
                }
                else if (healthPercent <= 0.3f)
                {
                    warningText.text = "WARNING - LOW LIGHT";
                    warningText.color = lowHealthColor;
                }
            }
        }
    }
    
    Color GetHealthColor(float healthPercent)
    {
        if (healthPercent > 0.7f)
        {
            return highHealthColor;
        }
        else if (healthPercent > 0.4f)
        {
            return Color.Lerp(mediumHealthColor, highHealthColor, (healthPercent - 0.4f) / 0.3f);
        }
        else if (healthPercent > 0.2f)
        {
            return Color.Lerp(lowHealthColor, mediumHealthColor, (healthPercent - 0.2f) / 0.2f);
        }
        else
        {
            return Color.Lerp(criticalHealthColor, lowHealthColor, healthPercent / 0.2f);
        }
    }
    
    string GetHealthStatus(float healthPercent)
    {
        if (healthPercent > 0.8f)
        {
            return "Radiant";
        }
        else if (healthPercent > 0.6f)
        {
            return "Bright";
        }
        else if (healthPercent > 0.4f)
        {
            return "Dimming";
        }
        else if (healthPercent > 0.2f)
        {
            return "Fading";
        }
        else if (healthPercent > 0.1f)
        {
            return "Flickering";
        }
        else
        {
            return "Dying";
        }
    }
    
    // Event handlers for health system events
    void OnHealthChanged(float newHealth, float maxHealth)
    {
        // Health changed - UI will update automatically in Update()
        lastHealthValue = newHealth;
    }
    
    void OnLowLightWarning()
    {
        // Player's light is getting low
        if (statusText)
        {
            StartCoroutine(FlashText(statusText, "LIGHT LOW!", lowHealthColor, 2f));
        }
    }
    
    void OnCriticalLightWarning()
    {
        // Player's light is critically low
        if (statusText)
        {
            StartCoroutine(FlashText(statusText, "CRITICAL!", criticalHealthColor, 3f));
        }
    }
    
    void OnLightRestored(float restoredAmount)
    {
        // Light was restored - this method is now only called from other places since no event for it
        if (statusText)
        {
            StartCoroutine(FlashText(statusText, $"+{restoredAmount:F0} LIGHT", highHealthColor, 1.5f));
        }
    }
    
    void OnPlayerDeath()
    {
        // Player has died - show death UI
        if (statusText)
        {
            statusText.text = "LIGHT EXTINGUISHED";
            statusText.color = Color.black;
        }
        
        if (warningText)
        {
            warningText.text = "PRESS R TO RESPAWN";
            warningText.color = Color.white;
        }
        
        if (warningPanel)
        {
            warningPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Flash text with a message temporarily
    /// </summary>
    System.Collections.IEnumerator FlashText(TextMeshProUGUI text, string message, Color color, float duration)
    {
        string originalText = text.text;
        Color originalColor = text.color;
        
        text.text = message;
        text.color = color;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * 5f));
            text.color = new Color(color.r, color.g, color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        text.text = originalText;
        text.color = originalColor;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealth != null)
        {
            playerHealth.OnLightResourceChanged -= OnHealthChanged;
            playerHealth.OnLightWarning -= OnLowLightWarning;
            playerHealth.OnLightCritical -= OnCriticalLightWarning;
            playerHealth.OnLightExtinguished -= OnPlayerDeath;
        }
    }
}