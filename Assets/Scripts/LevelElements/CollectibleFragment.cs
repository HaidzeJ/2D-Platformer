using UnityEngine;

/// <summary>
/// Collectible fragment that the player can pick up to progress ability unlocks.
/// Automatically adds itself to the FragmentCollectionManager when collected.
/// </summary>
public class CollectibleFragment : MonoBehaviour
{
    [Header("Fragment Settings")]
    [SerializeField] private int fragmentValue = 1;
    [SerializeField] private bool destroyOnPickup = true;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 2f;
    
    [Header("Collection Settings")]
    [SerializeField] private LayerMask playerLayer = -1;
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool hasBeenCollected = false;
    
    void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        // If no audio source exists but we have a sound, create one
        if (audioSource == null && collectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = collectSound;
            audioSource.playOnAwake = false;
        }
    }
    
    void Update()
    {
        if (!hasBeenCollected)
        {
            AnimateFragment();
        }
    }
    
    void AnimateFragment()
    {
        // Rotate the fragment
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenCollected) return;
        
        // Check if the collider is on the player layer
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            CollectFragment();
        }
    }
    
    void CollectFragment()
    {
        if (hasBeenCollected) return;
        
        hasBeenCollected = true;
        
        // Find and notify the collection manager
        FragmentCollectionManager collectionManager = FindFirstObjectByType<FragmentCollectionManager>();
        if (collectionManager != null)
        {
            collectionManager.CollectFragment(fragmentValue);
        }
        else
        {
            Debug.LogWarning("No FragmentCollectionManager found in scene! Fragment collected but not counted.");
        }
        
        // Play collect effects
        PlayCollectionEffects();
        
        // Destroy or hide the fragment
        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    void PlayCollectionEffects()
    {
        // Spawn visual effect
        if (collectEffect != null)
        {
            GameObject effect = Instantiate(collectEffect, transform.position, transform.rotation);
            
            // Auto-destroy effect after a few seconds if it doesn't destroy itself
            Destroy(effect, 5f);
        }
        
        // Play collection sound
        if (audioSource != null && collectSound != null)
        {
            // If we're about to destroy the object, play the sound from a different source
            if (destroyOnPickup)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
            else
            {
                audioSource.Play();
            }
        }
    }
    
    /// <summary>
    /// Get the value of this fragment (for display purposes)
    /// </summary>
    public int GetFragmentValue()
    {
        return fragmentValue;
    }
    
    /// <summary>
    /// Check if this fragment has been collected
    /// </summary>
    public bool IsCollected()
    {
        return hasBeenCollected;
    }
    
    /// <summary>
    /// Manually collect this fragment (useful for scripted events)
    /// </summary>
    public void ForceCollect()
    {
        CollectFragment();
    }
}