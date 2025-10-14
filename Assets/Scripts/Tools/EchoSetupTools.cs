using UnityEngine;

/// <summary>
/// Quick setup tools for creating Echo Pulse test objects in the scene.
/// Use the context menu items to rapidly prototype echo mechanics.
/// </summary>
public class EchoSetupTools : MonoBehaviour
{
    [Header("Prefab Creation Settings")]
    [SerializeField] private Material echoMaterial;
    [SerializeField] private Sprite bridgeSprite;
    [SerializeField] private Sprite doorSprite;
    [SerializeField] private Sprite switchSprite;
    
    /// <summary>
    /// Create a test echo bridge at the specified position
    /// </summary>
    [ContextMenu("Create Test Echo Bridge")]
    public void CreateTestEchoBridge()
    {
        CreateEchoBridge(transform.position + Vector3.right * 2f);
    }
    
    /// <summary>
    /// Create a test echo door
    /// </summary>
    [ContextMenu("Create Test Echo Door")]
    public void CreateTestEchoDoor()
    {
        CreateEchoDoor(transform.position + Vector3.right * 4f);
    }
    
    /// <summary>
    /// Create a test current switcher
    /// </summary>
    [ContextMenu("Create Test Current Switcher")]
    public void CreateTestCurrentSwitcher()
    {
        CreateEchoCurrentSwitcher(transform.position + Vector3.right * 6f);
    }
    
    /// <summary>
    /// Create an echo bridge GameObject
    /// </summary>
    public GameObject CreateEchoBridge(Vector3 position)
    {
        GameObject bridge = new GameObject("EchoBridge");
        bridge.transform.position = position;
        bridge.layer = LayerMask.NameToLayer("Default"); // You might want to create an "EchoObjects" layer
        
        // Add SpriteRenderer
        SpriteRenderer sr = bridge.AddComponent<SpriteRenderer>();
        if (bridgeSprite != null)
        {
            sr.sprite = bridgeSprite;
        }
        else
        {
            // Create a simple white square if no sprite assigned
            sr.sprite = CreateSquareSprite();
        }
        sr.color = new Color(0.5f, 0.8f, 1f, 0.7f); // Light blue
        
        // Add Collider
        BoxCollider2D collider = bridge.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(2f, 0.5f); // Platform size
        
        // Add EchoBridge component
        EchoBridge echoBridge = bridge.AddComponent<EchoBridge>();
        
        Debug.Log($"🌉 Created Echo Bridge at {position}");
        return bridge;
    }
    
    /// <summary>
    /// Create an echo door GameObject
    /// </summary>
    public GameObject CreateEchoDoor(Vector3 position)
    {
        GameObject door = new GameObject("EchoDoor");
        door.transform.position = position;
        door.layer = LayerMask.NameToLayer("Default");
        
        // Add SpriteRenderer
        SpriteRenderer sr = door.AddComponent<SpriteRenderer>();
        if (doorSprite != null)
        {
            sr.sprite = doorSprite;
        }
        else
        {
            sr.sprite = CreateSquareSprite();
        }
        sr.color = new Color(0.8f, 0.4f, 0.2f, 0.8f); // Orange
        
        // Add Collider (initially blocking)
        BoxCollider2D collider = door.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 3f); // Door size
        collider.isTrigger = false; // Solid when closed
        
        // Add EchoDoor component
        EchoDoor echoDoor = door.AddComponent<EchoDoor>();
        
        Debug.Log($"🚪 Created Echo Door at {position}");
        return door;
    }
    
    /// <summary>
    /// Create an echo current switcher GameObject
    /// </summary>
    public GameObject CreateEchoCurrentSwitcher(Vector3 position)
    {
        GameObject switcher = new GameObject("EchoCurrentSwitcher");
        switcher.transform.position = position;
        switcher.layer = LayerMask.NameToLayer("Default");
        
        // Add SpriteRenderer
        SpriteRenderer sr = switcher.AddComponent<SpriteRenderer>();
        if (switchSprite != null)
        {
            sr.sprite = switchSprite;
        }
        else
        {
            sr.sprite = CreateSquareSprite();
        }
        sr.color = new Color(1f, 0.2f, 0.2f, 0.9f); // Red
        
        // Add Trigger Collider
        CircleCollider2D collider = switcher.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        collider.isTrigger = true;
        
        // Add EchoCurrentSwitcher component
        EchoCurrentSwitcher echoSwitcher = switcher.AddComponent<EchoCurrentSwitcher>();
        
        Debug.Log($"🔌 Created Echo Current Switcher at {position}");
        return switcher;
    }
    
    /// <summary>
    /// Create a simple square sprite for testing
    /// </summary>
    private Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }
    
    /// <summary>
    /// Create a complete test area with various echo objects
    /// </summary>
    [ContextMenu("Create Echo Test Area")]
    public void CreateEchoTestArea()
    {
        Vector3 basePos = transform.position;
        
        // Create a series of platforms and interactive elements
        CreateEchoBridge(basePos + new Vector3(3f, 2f, 0f));
        CreateEchoBridge(basePos + new Vector3(6f, 4f, 0f));
        CreateEchoDoor(basePos + new Vector3(9f, 0f, 0f));
        CreateEchoCurrentSwitcher(basePos + new Vector3(0f, 2f, 0f));
        
        // Create a light current to connect with the switcher
        GameObject current = new GameObject("TestLightCurrent");
        current.transform.position = basePos + new Vector3(1.5f, 0f, 0f);
        
        SpriteRenderer currentSR = current.AddComponent<SpriteRenderer>();
        currentSR.sprite = CreateSquareSprite();
        currentSR.color = new Color(1f, 1f, 0f, 0.3f); // Yellow transparent
        
        BoxCollider2D currentCollider = current.AddComponent<BoxCollider2D>();
        currentCollider.size = new Vector2(2f, 4f);
        currentCollider.isTrigger = true;
        
        LightCurrent lightCurrent = current.AddComponent<LightCurrent>();
        
        // Connect the switcher to the current
        EchoCurrentSwitcher switcher = FindFirstObjectByType<EchoCurrentSwitcher>();
        if (switcher != null)
        {
            switcher.AddTargetCurrent(lightCurrent);
        }
        
        Debug.Log("🔊 Created complete Echo Test Area! Try using Echo Pulse to discover the hidden elements.");
    }
    
    /// <summary>
    /// Set up proper layers for echo objects
    /// </summary>
    [ContextMenu("Setup Echo Layers")]
    public void SetupEchoLayers()
    {
        Debug.Log("🔧 To setup Echo Layers:");
        Debug.Log("1. Go to Edit → Project Settings → Tags and Layers");
        Debug.Log("2. Create a new layer called 'EchoObjects'");
        Debug.Log("3. Set all echo objects to this layer");
        Debug.Log("4. Configure 'echoObjectLayers' in PlayerMovement to include this layer");
    }
}