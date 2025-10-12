using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Scene lighting configurator to make the Light Resource Health system visually apparent.
/// This script helps set up proper lighting conditions to showcase the health-based light effects.
/// </summary>
public class LightingSceneSetup : MonoBehaviour
{
    [Header("Lighting Configuration")]
    [SerializeField] private bool autoConfigureOnStart = true;
    [SerializeField] private bool createDarkEnvironment = true;
    
    [Header("Environment Settings")]
    [SerializeField] private Color ambientLightColor = new Color(0.1f, 0.1f, 0.2f, 1f); // Very dark blue
    [SerializeField] private float ambientLightIntensity = 0.2f; // Very dim ambient
    [SerializeField] private Color fogColor = new Color(0.05f, 0.05f, 0.1f, 1f); // Dark fog
    [SerializeField] private float fogDensity = 0.02f;
    
    [Header("Background")]
    [SerializeField] private bool createDarkBackground = true;
    [SerializeField] private Color backgroundColor = new Color(0.02f, 0.02f, 0.05f, 1f); // Very dark
    
    void Start()
    {
        if (autoConfigureOnStart)
        {
            ConfigureSceneLighting();
        }
    }
    
    [ContextMenu("Configure Scene Lighting")]
    public void ConfigureSceneLighting()
    {
        Debug.Log("Configuring scene lighting for Light Resource Health visibility...");
        
        ConfigureAmbientLighting();
        ConfigureCameraSettings();
        ConfigureRenderPipeline();
        
        if (createDarkBackground)
        {
            SetupDarkBackground();
        }
        
        if (createDarkEnvironment)
        {
            CreateEnvironmentalShadows();
        }
        
        OptimizePlayerLighting();
        
        Debug.Log("Scene lighting configuration complete! Light effects should now be clearly visible.");
    }
    
    void ConfigureAmbientLighting()
    {
        // Set very low ambient lighting so player's light stands out
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientLightColor;
        RenderSettings.ambientIntensity = ambientLightIntensity;
        
        // Configure fog for atmosphere
        RenderSettings.fog = true;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = fogDensity;
        
        Debug.Log("Ambient lighting configured - very dark environment created");
    }
    
    void ConfigureCameraSettings()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera != null)
        {
            // Set dark background color
            mainCamera.backgroundColor = backgroundColor;
            
            // Ensure camera renders in the right mode
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            
            Debug.Log("Camera configured with dark background");
        }
        else
        {
            Debug.LogWarning("No camera found to configure!");
        }
    }
    
    void ConfigureRenderPipeline()
    {
        // Try to configure URP settings if available
        var urpAsset = UniversalRenderPipeline.asset;
        if (urpAsset != null)
        {
            Debug.Log("URP detected - lighting should work well with 2D lights");
        }
        else
        {
            Debug.LogWarning("URP not detected - consider switching to Universal Render Pipeline for better 2D lighting");
        }
    }
    
    void SetupDarkBackground()
    {
        // Create a dark background sprite if none exists
        GameObject background = GameObject.Find("DarkBackground");
        if (background == null)
        {
            background = new GameObject("DarkBackground");
            background.transform.SetParent(transform);
            
            SpriteRenderer bgRenderer = background.AddComponent<SpriteRenderer>();
            
            // Create a large dark texture
            Texture2D darkTexture = new Texture2D(1, 1);
            darkTexture.SetPixel(0, 0, backgroundColor);
            darkTexture.Apply();
            
            Sprite darkSprite = Sprite.Create(darkTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            bgRenderer.sprite = darkSprite;
            
            // Scale it to cover the screen
            background.transform.localScale = new Vector3(50, 50, 1);
            background.transform.position = new Vector3(0, 0, 10); // Behind everything
            
            // Set sorting order to be behind everything
            bgRenderer.sortingOrder = -1000;
            
            Debug.Log("Created dark background");
        }
    }
    
    void CreateEnvironmentalShadows()
    {
        // Create some shadow areas to demonstrate light penetration
        CreateShadowArea(new Vector3(-10, 0, 0), "Left Shadow Zone");
        CreateShadowArea(new Vector3(10, 0, 0), "Right Shadow Zone");
        CreateShadowArea(new Vector3(0, 5, 0), "Upper Shadow Zone");
    }
    
    void CreateShadowArea(Vector3 position, string name)
    {
        GameObject shadowArea = new GameObject(name);
        shadowArea.transform.SetParent(transform);
        shadowArea.transform.position = position;
        
        SpriteRenderer shadowRenderer = shadowArea.AddComponent<SpriteRenderer>();
        
        // Create semi-transparent dark sprite
        Texture2D shadowTexture = new Texture2D(1, 1);
        shadowTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f)); // Semi-transparent black
        shadowTexture.Apply();
        
        Sprite shadowSprite = Sprite.Create(shadowTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        shadowRenderer.sprite = shadowSprite;
        shadowRenderer.color = new Color(0, 0, 0, 0.7f);
        
        // Scale to create shadow areas
        shadowArea.transform.localScale = new Vector3(8, 6, 1);
        
        // Set sorting order to be in front of background but behind player
        shadowRenderer.sortingOrder = -50;
    }
    
    void OptimizePlayerLighting()
    {
        // Find and optimize player lighting
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Light playerLight = player.GetComponent<Light>();
            if (playerLight != null)
            {
                // Ensure proper light settings for visibility
                playerLight.type = LightType.Point;
                playerLight.color = Color.white;
                
                // Make sure light is bright enough to be seen
                if (playerLight.intensity < 1f)
                {
                    playerLight.intensity = 2f;
                }
                
                // Ensure good range
                if (playerLight.range < 3f)
                {
                    playerLight.range = 6f;
                }
                
                Debug.Log($"Player light optimized - Intensity: {playerLight.intensity}, Range: {playerLight.range}");
            }
            else
            {
                // Add light if missing
                playerLight = player.AddComponent<Light>();
                playerLight.type = LightType.Point;
                playerLight.color = Color.white;
                playerLight.intensity = 2f;
                playerLight.range = 6f;
                
                Debug.Log("Added missing light to player");
            }
            
            // Ensure player has proper sprite renderer settings
            SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                // Make sure material supports lighting
                if (playerSprite.material.name.Contains("Default"))
                {
                    Debug.LogWarning("Player using default material - consider using a material that responds to lighting");
                }
            }
        }
        else
        {
            Debug.LogWarning("No player found with 'Player' tag!");
        }
    }
    
    [ContextMenu("Test Light Dimming")]
    public void TestLightDimming()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Light playerLight = player.GetComponent<Light>();
            if (playerLight != null)
            {
                StartCoroutine(DimmingTest(playerLight));
            }
        }
    }
    
    System.Collections.IEnumerator DimmingTest(Light light)
    {
        float originalIntensity = light.intensity;
        float originalRange = light.range;
        
        Debug.Log("Starting light dimming test...");
        
        // Dim the light over 3 seconds
        for (float t = 0; t < 3f; t += Time.deltaTime)
        {
            float progress = t / 3f;
            light.intensity = Mathf.Lerp(originalIntensity, 0.2f, progress);
            light.range = Mathf.Lerp(originalRange, 1f, progress);
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        
        Debug.Log("Restoring light...");
        
        // Restore the light over 2 seconds
        for (float t = 0; t < 2f; t += Time.deltaTime)
        {
            float progress = t / 2f;
            light.intensity = Mathf.Lerp(0.2f, originalIntensity, progress);
            light.range = Mathf.Lerp(1f, originalRange, progress);
            yield return null;
        }
        
        Debug.Log("Light dimming test complete!");
    }
    
    [ContextMenu("Reset Scene Lighting")]
    public void ResetSceneLighting()
    {
        // Reset to Unity defaults
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientLight = Color.white;
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.fog = false;
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = new Color(0.19f, 0.3f, 0.47f);
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }
        
        Debug.Log("Scene lighting reset to defaults");
    }
}