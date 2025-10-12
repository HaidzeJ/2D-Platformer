using UnityEngine;
using System.Collections;

/// <summary>
/// An echo-revealed door that can block or allow passage.
/// Perfect for creating hidden passages and puzzle elements.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EchoDoor : EchoObject
{
    [Header("Door-Specific Settings")]
    [SerializeField] private bool isOpenByDefault = false;
    [SerializeField] private bool staysOpenPermanently = false;
    [SerializeField] private Vector2 closedPosition;
    [SerializeField] private Vector2 openPosition;
    [SerializeField] private float doorMoveSpeed = 2f;
    
    [Header("Door Visual & Audio")]
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip doorCloseSound;
    [SerializeField] private AudioClip doorRevealSound;
    [SerializeField] private ParticleSystem doorParticles;
    
    private AudioSource audioSource;
    private bool isDoorOpen;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private Coroutine moveCoroutine;
    
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
        
        // Set initial positions if not set
        if (closedPosition == Vector2.zero && openPosition == Vector2.zero)
        {
            closedPosition = transform.position;
            openPosition = closedPosition + Vector2.up * 3f; // Default: door slides up
        }
        
        // Set initial state
        isDoorOpen = isOpenByDefault;
        transform.position = isDoorOpen ? openPosition : closedPosition;
        targetPosition = transform.position;
        
        // Door visibility follows open/closed state
        if (!startVisible)
        {
            UpdateVisibility(false, false);
        }
        
        Debug.Log($"🚪 EchoDoor '{name}' initialized - Open: {isDoorOpen}, Permanent: {staysOpenPermanently}");
    }
    
    protected override void OnEchoRevealed(float duration)
    {
        base.OnEchoRevealed(duration);
        
        // Play reveal sound
        if (audioSource != null && doorRevealSound != null)
        {
            audioSource.PlayOneShot(doorRevealSound);
        }
        
        // Toggle door state when revealed
        if (canToggle)
        {
            ToggleDoor();
        }
        else
        {
            // Non-toggle doors open when revealed
            if (!isDoorOpen)
            {
                OpenDoor();
            }
        }
        
        Debug.Log($"🚪 Door '{name}' revealed by echo pulse!");
    }
    
    /// <summary>
    /// Open the door
    /// </summary>
    public void OpenDoor()
    {
        if (isDoorOpen || isMoving) return;
        
        isDoorOpen = true;
        MoveDoorTo(openPosition);
        
        // Play sound
        if (audioSource != null && doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
        
        // Particle effect
        if (doorParticles != null)
        {
            doorParticles.Play();
        }
    }
    
    /// <summary>
    /// Close the door
    /// </summary>
    public void CloseDoor()
    {
        if (!isDoorOpen || isMoving) return;
        
        // Don't close if permanent
        if (staysOpenPermanently && hasCollider)
        {
            return;
        }
        
        isDoorOpen = false;
        MoveDoorTo(closedPosition);
        
        // Play sound
        if (audioSource != null && doorCloseSound != null)
        {
            audioSource.PlayOneShot(doorCloseSound);
        }
    }
    
    /// <summary>
    /// Toggle door state
    /// </summary>
    public void ToggleDoor()
    {
        if (isDoorOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }
    
    /// <summary>
    /// Move door to target position
    /// </summary>
    private void MoveDoorTo(Vector2 target)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        
        targetPosition = target;
        moveCoroutine = StartCoroutine(AnimateDoorMovement());
    }
    
    /// <summary>
    /// Animate the door movement
    /// </summary>
    private IEnumerator AnimateDoorMovement()
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = distance / doorMoveSpeed;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Smooth movement curve
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            
            yield return null;
        }
        
        transform.position = targetPosition;
        isMoving = false;
        moveCoroutine = null;
    }
    
    /// <summary>
    /// Override hide behavior - doors might behave differently
    /// </summary>
    protected override void HideObject()
    {
        if (staysOpenPermanently && isDoorOpen)
        {
            // Don't hide permanent open doors
            return;
        }
        
        base.HideObject();
        
        // Optionally close door when hidden
        if (isDoorOpen)
        {
            CloseDoor();
        }
    }
    
    /// <summary>
    /// Handle player collision with door
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isRevealed)
        {
            // Could add interaction prompt here
        }
    }
    
    /// <summary>
    /// For level designers - set door positions
    /// </summary>
    [ContextMenu("Set Closed Position")]
    public void SetClosedPosition()
    {
        closedPosition = transform.position;
    }
    
    [ContextMenu("Set Open Position")]  
    public void SetOpenPosition()
    {
        openPosition = transform.position;
    }
    
    [ContextMenu("Test Door Toggle")]
    public void TestDoorToggle()
    {
        if (!isRevealed)
        {
            RevealObject(5f);
        }
        ToggleDoor();
    }
    
    /// <summary>
    /// Get door status for debugging
    /// </summary>
    public string GetDoorStatus()
    {
        return $"Door '{name}': Revealed={isRevealed}, Open={isDoorOpen}, Moving={isMoving}, Permanent={staysOpenPermanently}";
    }
    
    /// <summary>
    /// Make door permanent (for progression systems)
    /// </summary>
    public void MakePermanent()
    {
        staysOpenPermanently = true;
        if (isDoorOpen && hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        Debug.Log($"🚪 Door '{name}' made permanent");
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw door positions in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(closedPosition, Vector3.one * 0.5f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(openPosition, Vector3.one * 0.5f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(closedPosition, openPosition);
    }
}