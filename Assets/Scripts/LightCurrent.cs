using UnityEngine;

/// <summary>
/// Light Current - Environmental element that propels the orb in a specific direction.
/// Perfect for creating guided navigation and atmospheric level design elements.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LightCurrent : MonoBehaviour
{
    [Header("Current Settings")]
    [SerializeField] Vector2 currentDirection = Vector2.up;
    [SerializeField] float currentForce = 5f;
    [SerializeField] bool normalizeDirection = true;
    
    [Header("Force Application")]
    [SerializeField] ForceMode2D forceMode = ForceMode2D.Force;
    [SerializeField] float maxVelocity = 15f; // Prevent excessive speeds
    [SerializeField] bool onlyAffectOrbs = true; // Only affect orb mode players
    
    [Header("Visual Feedback")]
    [SerializeField] bool showDirectionGizmo = true;
    [SerializeField] Color gizmoColor = Color.cyan;
    [SerializeField] float gizmoLength = 2f;
    [SerializeField] GameObject visualEffect; // Optional particle system or animation
    
    [Header("Audio")]
    [SerializeField] AudioClip enterSound;
    [SerializeField] AudioClip loopSound;
    [SerializeField] [Range(0f, 1f)] float volume = 0.5f;
    
    private Collider2D currentCollider;
    private AudioSource audioSource;
    private PlayerMovement currentPlayer;
    private bool isPlayerInCurrent = false;
    private LightCurrentVisualEffect visualEffectComponent;
    
    // Normalized direction for consistent force application
    public Vector2 NormalizedDirection => normalizeDirection ? currentDirection.normalized : currentDirection;
    
    void Awake()
    {
        // Ensure we have a trigger collider
        currentCollider = GetComponent<Collider2D>();
        if (!currentCollider.isTrigger)
        {
            Debug.LogWarning($"LightCurrent ({name}): Collider should be set as Trigger!");
            currentCollider.isTrigger = true;
        }
        
        // Setup audio source if we have sounds
        if (enterSound != null || loopSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = volume;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
        
        // Setup visual effect component
        visualEffectComponent = GetComponent<LightCurrentVisualEffect>();
        
        // Activate visual effect if provided
        if (visualEffect != null)
        {
            visualEffect.SetActive(false);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;
        
        // Check if we should only affect orb mode players
        if (onlyAffectOrbs && !player.IsInOrbMode)
        {
            return;
        }
        
        currentPlayer = player;
        isPlayerInCurrent = true;
        
        // Play enter sound
        if (audioSource != null && enterSound != null)
        {
            audioSource.PlayOneShot(enterSound);
        }
        
        // Start loop sound
        if (audioSource != null && loopSound != null)
        {
            audioSource.clip = loopSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Activate visual effect
        if (visualEffect != null)
        {
            visualEffect.SetActive(true);
        }
        
        // Activate visual effect component
        if (visualEffectComponent != null)
        {
            visualEffectComponent.Activate();
        }
        
        Debug.Log($"💨 Player entered Light Current: {name} (Direction: {NormalizedDirection})");
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != currentPlayer) return;
        
        currentPlayer = null;
        isPlayerInCurrent = false;
        
        // Stop loop sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Deactivate visual effect
        if (visualEffect != null)
        {
            visualEffect.SetActive(false);
        }
        
        // Deactivate visual effect component
        if (visualEffectComponent != null)
        {
            visualEffectComponent.Deactivate();
        }
        
        Debug.Log($"💨 Player exited Light Current: {name}");
    }
    
    void FixedUpdate()
    {
        if (!isPlayerInCurrent || currentPlayer == null) return;
        
        ApplyLightCurrent();
    }
    
    void ApplyLightCurrent()
    {
        Rigidbody2D playerRb = currentPlayer.GetRigidbody();
        if (playerRb == null) return;
        
        // Calculate force to apply
        Vector2 forceToApply = NormalizedDirection * currentForce;
        
        // Apply velocity limiting if enabled
        if (maxVelocity > 0f)
        {
            Vector2 currentVelocity = playerRb.linearVelocity;
            Vector2 projectedVelocity = currentVelocity + (forceToApply * Time.fixedDeltaTime);
            
            // Clamp the velocity component in the current direction
            float velocityInDirection = Vector2.Dot(projectedVelocity, NormalizedDirection);
            if (velocityInDirection > maxVelocity)
            {
                // Reduce force to prevent exceeding max velocity
                float excessVelocity = velocityInDirection - maxVelocity;
                forceToApply -= NormalizedDirection * (excessVelocity / Time.fixedDeltaTime);
            }
        }
        
        // Apply the force
        playerRb.AddForce(forceToApply, forceMode);
    }
    
    /// <summary>
    /// Set the current direction and force programmatically
    /// </summary>
    public void SetCurrentProperties(Vector2 direction, float force)
    {
        currentDirection = direction;
        currentForce = force;
    }
    
    /// <summary>
    /// Get the effective force that would be applied to a rigidbody
    /// </summary>
    public Vector2 GetEffectiveForce()
    {
        return NormalizedDirection * currentForce;
    }
    
    /// <summary>
    /// Check if a specific player is currently in this light current
    /// </summary>
    public bool IsPlayerInCurrent(PlayerMovement player)
    {
        return isPlayerInCurrent && currentPlayer == player;
    }
    
    void OnDrawGizmos()
    {
        if (!showDirectionGizmo) return;
        
        Gizmos.color = gizmoColor;
        Vector3 center = transform.position;
        Vector3 direction = NormalizedDirection;
        
        // Draw direction arrow
        Gizmos.DrawRay(center, direction * gizmoLength);
        
        // Draw arrow head
        float arrowSize = gizmoLength * 0.2f;
        Vector3 arrowHead1 = center + (direction * gizmoLength) + (Quaternion.Euler(0, 0, 135) * direction * arrowSize);
        Vector3 arrowHead2 = center + (direction * gizmoLength) + (Quaternion.Euler(0, 0, -135) * direction * arrowSize);
        
        Gizmos.DrawLine(center + direction * gizmoLength, arrowHead1);
        Gizmos.DrawLine(center + direction * gizmoLength, arrowHead2);
        
        // Draw trigger area outline
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            
            if (col is BoxCollider2D boxCol)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawSphere(circleCol.offset, circleCol.radius);
            }
            
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw more detailed info when selected
        Gizmos.color = Color.white;
        Vector3 center = transform.position;
        
        // Draw force magnitude as text would be nice, but we can show it with line thickness
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(center, NormalizedDirection * (currentForce * 0.3f));
        
        #if UNITY_EDITOR
        // Draw labels in editor
        UnityEditor.Handles.Label(center + Vector3.up * 0.5f, $"Force: {currentForce}");
        UnityEditor.Handles.Label(center + Vector3.up * 0.3f, $"Direction: {currentDirection}");
        UnityEditor.Handles.Label(center + Vector3.up * 0.1f, $"Max Vel: {maxVelocity}");
        #endif
    }
    
    void OnValidate()
    {
        // Ensure direction is not zero
        if (currentDirection.magnitude < 0.001f)
        {
            currentDirection = Vector2.up;
        }
        
        // Ensure reasonable force values
        currentForce = Mathf.Max(0f, currentForce);
        maxVelocity = Mathf.Max(0f, maxVelocity);
    }
}