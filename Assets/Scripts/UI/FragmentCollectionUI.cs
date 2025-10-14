using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI display for fragment collection progress and ability unlock status.
/// Shows current fragments, progress to next unlock, and stage information.
/// </summary>
public class FragmentCollectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI fragmentCountText;
    [SerializeField] private TextMeshProUGUI currentStageText;
    [SerializeField] private TextMeshProUGUI nextStageText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image progressFillImage;
    
    [Header("Visual Settings")]
    [SerializeField] private Color progressColor = Color.cyan;
    [SerializeField] private Color completeColor = Color.green;
    [SerializeField] private AnimationCurve collectAnimation = AnimationCurve.EaseInOut(0, 1, 0.5f, 1.5f);
    [SerializeField] private float animationDuration = 0.5f;
    
    [Header("Stage Unlock Effects")]
    [SerializeField] private GameObject stageUnlockEffect;
    [SerializeField] private AudioClip stageUnlockSound;
    [SerializeField] private float stageUnlockDisplayTime = 3f;
    
    private FragmentCollectionManager collectionManager;
    private AudioSource audioSource;
    private bool isAnimating = false;
    private Coroutine animationCoroutine;
    
    void Start()
    {
        // Find the collection manager
        collectionManager = FindFirstObjectByType<FragmentCollectionManager>();
        if (collectionManager == null)
        {
            Debug.LogWarning("FragmentCollectionUI: No FragmentCollectionManager found in scene!");
            return;
        }
        
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && stageUnlockSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Subscribe to events
        collectionManager.OnFragmentCollected.AddListener(OnFragmentCollected);
        collectionManager.OnStageUnlocked.AddListener(OnStageUnlocked);
        collectionManager.OnProgressUpdate.AddListener(OnProgressUpdate);
        
        // Initialize display
        UpdateDisplay();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent errors
        if (collectionManager != null)
        {
            collectionManager.OnFragmentCollected.RemoveListener(OnFragmentCollected);
            collectionManager.OnStageUnlocked.RemoveListener(OnStageUnlocked);
            collectionManager.OnProgressUpdate.RemoveListener(OnProgressUpdate);
        }
    }
    
    void OnFragmentCollected(int totalFragments)
    {
        // Animate the fragment count update
        AnimateFragmentCollection();
        UpdateDisplay();
    }
    
    void OnStageUnlocked(int stageIndex, string stageName)
    {
        // Show stage unlock effect
        ShowStageUnlockEffect(stageName);
        UpdateDisplay();
    }
    
    void OnProgressUpdate(int currentFragments, int fragmentsForNext)
    {
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        if (collectionManager == null) return;
        
        int currentFragments = collectionManager.GetCurrentFragmentCount();
        var currentStage = collectionManager.GetCurrentStage();
        var nextStage = collectionManager.GetNextStage();
        
        // Update fragment count
        if (fragmentCountText != null)
        {
            fragmentCountText.text = $"Fragments: {currentFragments}";
        }
        
        // Update current stage
        if (currentStageText != null && currentStage != null)
        {
            currentStageText.text = $"Current: {currentStage.stageName}";
        }
        
        // Update next stage info
        if (nextStageText != null)
        {
            if (nextStage != null)
            {
                int fragmentsNeeded = nextStage.requiredFragments - currentFragments;
                nextStageText.text = $"Next: {nextStage.stageName} ({fragmentsNeeded} more)";
            }
            else
            {
                nextStageText.text = "All abilities unlocked!";
            }
        }
        
        // Update progress bar
        if (progressSlider != null)
        {
            float progress = collectionManager.GetProgressToNextStage();
            progressSlider.value = progress;
            
            // Update fill color
            if (progressFillImage != null)
            {
                progressFillImage.color = progress >= 1f ? completeColor : progressColor;
            }
        }
    }
    
    void AnimateFragmentCollection()
    {
        if (fragmentCountText == null) return;
        
        // Stop any existing animation
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(AnimateFragmentText());
    }
    
    System.Collections.IEnumerator AnimateFragmentText()
    {
        if (fragmentCountText == null) yield break;
        
        isAnimating = true;
        Vector3 originalScale = fragmentCountText.transform.localScale;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            float progress = elapsedTime / animationDuration;
            float scaleMultiplier = collectAnimation.Evaluate(progress);
            
            fragmentCountText.transform.localScale = originalScale * scaleMultiplier;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Reset scale
        fragmentCountText.transform.localScale = originalScale;
        isAnimating = false;
    }
    
    void ShowStageUnlockEffect(string stageName)
    {
        // Play unlock sound
        if (audioSource != null && stageUnlockSound != null)
        {
            audioSource.PlayOneShot(stageUnlockSound);
        }
        
        // Show visual effect
        if (stageUnlockEffect != null)
        {
            GameObject effect = Instantiate(stageUnlockEffect, transform);
            Destroy(effect, stageUnlockDisplayTime);
        }
        
        // You could add more effects here like screen flash, particle effects, etc.
        Debug.Log($"🎉 STAGE UNLOCKED: {stageName}!");
    }
    
    /// <summary>
    /// Manually refresh the display (useful for editor testing)
    /// </summary>
    [ContextMenu("Refresh Display")]
    public void RefreshDisplay()
    {
        UpdateDisplay();
    }
    
    /// <summary>
    /// Toggle the visibility of the fragment UI
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    /// <summary>
    /// Get the current animation state
    /// </summary>
    public bool IsAnimating()
    {
        return isAnimating;
    }
}