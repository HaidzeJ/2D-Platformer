using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Manages the collection of fragments and progression through ability unlock stages.
/// Tracks fragments collected and unlocks player abilities at specified thresholds.
/// </summary>
public class FragmentCollectionManager : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private int currentFragments = 0;
    
    [Header("Stage Progression")]
    [SerializeField] private List<StageUnlock> stageUnlocks = new List<StageUnlock>
    {
        new StageUnlock { stageIndex = 0, requiredFragments = 0, stageName = "Orb Mode", description = "Floating orb with pulse ability" },
        new StageUnlock { stageIndex = 1, requiredFragments = 4, stageName = "Movement", description = "Can move left and right" },
        new StageUnlock { stageIndex = 2, requiredFragments = 8, stageName = "Jump", description = "Can jump" },
        new StageUnlock { stageIndex = 3, requiredFragments = 12, stageName = "Double Jump", description = "Can double jump" },
        new StageUnlock { stageIndex = 4, requiredFragments = 16, stageName = "Dash", description = "Can dash through air" },
        new StageUnlock { stageIndex = 5, requiredFragments = 20, stageName = "Wall Jump", description = "Can wall jump (future)" }
    };
    
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    
    [Header("Events")]
    public UnityEvent<int> OnFragmentCollected;
    public UnityEvent<int, string> OnStageUnlocked;
    public UnityEvent<int, int> OnProgressUpdate; // current fragments, fragments needed for next stage
    
    [Header("Debug Info")]
    [SerializeField] private int currentStageIndex = 0;
    [SerializeField] private bool showDebugLogs = true;
    
    [System.Serializable]
    public class StageUnlock
    {
        public int stageIndex;
        public int requiredFragments;
        public string stageName;
        [TextArea(2, 4)]
        public string description;
    }
    
    void Start()
    {
        // Find player movement if not assigned
        if (playerMovement == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
            }
        }
        
        // Validate stage unlocks are properly configured
        ValidateStageConfiguration();
        
        // Set initial stage based on current fragments
        UpdatePlayerStage();
        
        // Initial progress update
        UpdateProgress();
        
        if (showDebugLogs)
        {
            Debug.Log($"Fragment Collection Manager initialized. Starting with {currentFragments} fragments.");
        }
    }
    
    void ValidateStageConfiguration()
    {
        // Sort stages by required fragments to ensure proper order
        stageUnlocks.Sort((a, b) => a.requiredFragments.CompareTo(b.requiredFragments));
        
        // Warn about configuration issues
        for (int i = 0; i < stageUnlocks.Count; i++)
        {
            if (stageUnlocks[i].stageIndex != i)
            {
                Debug.LogWarning($"Stage unlock at index {i} has stageIndex {stageUnlocks[i].stageIndex}. Consider updating for clarity.");
            }
        }
    }
    
    /// <summary>
    /// Called when a fragment is collected
    /// </summary>
    public void CollectFragment(int fragmentValue = 1)
    {
        currentFragments += fragmentValue;
        
        if (showDebugLogs)
        {
            Debug.Log($"Fragment collected! Total: {currentFragments} (+ {fragmentValue})");
        }
        
        // Fire fragment collected event
        OnFragmentCollected?.Invoke(currentFragments);
        
        // Check if we should unlock a new stage
        CheckForStageUnlock();
        
        // Update progress display
        UpdateProgress();
    }
    
    void CheckForStageUnlock()
    {
        // Find the highest stage we can unlock with current fragments
        int newStageIndex = currentStageIndex;
        
        for (int i = stageUnlocks.Count - 1; i >= 0; i--)
        {
            if (currentFragments >= stageUnlocks[i].requiredFragments)
            {
                newStageIndex = i;
                break;
            }
        }
        
        // If we found a higher stage, unlock it
        if (newStageIndex > currentStageIndex)
        {
            UnlockStage(newStageIndex);
        }
    }
    
    void UnlockStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= stageUnlocks.Count) return;
        
        currentStageIndex = stageIndex;
        StageUnlock unlockedStage = stageUnlocks[stageIndex];
        
        if (showDebugLogs)
        {
            Debug.Log($"STAGE UNLOCKED! {unlockedStage.stageName}: {unlockedStage.description}");
        }
        
        // Update player abilities
        UpdatePlayerStage();
        
        // Fire stage unlocked event
        OnStageUnlocked?.Invoke(stageIndex, unlockedStage.stageName);
    }
    
    void UpdatePlayerStage()
    {
        if (playerMovement != null && currentStageIndex < stageUnlocks.Count)
        {
            PlayerMovement.MovementStage stage = (PlayerMovement.MovementStage)currentStageIndex;
            playerMovement.SetMovementStage(stage);
        }
    }
    
    void UpdateProgress()
    {
        int fragmentsForNext = GetFragmentsNeededForNextStage();
        OnProgressUpdate?.Invoke(currentFragments, fragmentsForNext);
    }
    
    /// <summary>
    /// Get the number of fragments needed to unlock the next stage
    /// </summary>
    public int GetFragmentsNeededForNextStage()
    {
        int nextStageIndex = currentStageIndex + 1;
        if (nextStageIndex >= stageUnlocks.Count)
        {
            return -1; // All stages unlocked
        }
        
        return stageUnlocks[nextStageIndex].requiredFragments;
    }
    
    /// <summary>
    /// Get the current number of collected fragments
    /// </summary>
    public int GetCurrentFragmentCount()
    {
        return currentFragments;
    }
    
    /// <summary>
    /// Get the current stage information
    /// </summary>
    public StageUnlock GetCurrentStage()
    {
        if (currentStageIndex >= 0 && currentStageIndex < stageUnlocks.Count)
        {
            return stageUnlocks[currentStageIndex];
        }
        return null;
    }
    
    /// <summary>
    /// Get the next stage information
    /// </summary>
    public StageUnlock GetNextStage()
    {
        int nextIndex = currentStageIndex + 1;
        if (nextIndex >= 0 && nextIndex < stageUnlocks.Count)
        {
            return stageUnlocks[nextIndex];
        }
        return null;
    }
    
    /// <summary>
    /// Get progress percentage toward next stage (0-1)
    /// </summary>
    public float GetProgressToNextStage()
    {
        StageUnlock nextStage = GetNextStage();
        if (nextStage == null) return 1f; // All stages complete
        
        StageUnlock currentStage = GetCurrentStage();
        int currentStageFragments = currentStage != null ? currentStage.requiredFragments : 0;
        int fragmentsNeeded = nextStage.requiredFragments - currentStageFragments;
        int fragmentsProgress = currentFragments - currentStageFragments;
        
        return Mathf.Clamp01((float)fragmentsProgress / fragmentsNeeded);
    }
    
    /// <summary>
    /// Manually set fragment count (useful for testing or save/load)
    /// </summary>
    public void SetFragmentCount(int count)
    {
        int oldCount = currentFragments;
        currentFragments = count;
        
        if (showDebugLogs)
        {
            Debug.Log($"Fragment count set to {currentFragments} (was {oldCount})");
        }
        
        // Recalculate current stage
        CheckForStageUnlock();
        UpdateProgress();
    }
    
    /// <summary>
    /// Add fragments without collection effects (useful for loading saved progress)
    /// </summary>
    public void AddFragmentsSilent(int count)
    {
        currentFragments += count;
        CheckForStageUnlock();
        UpdateProgress();
    }
    
    /// <summary>
    /// Reset all progress (useful for new game)
    /// </summary>
    public void ResetProgress()
    {
        currentFragments = 0;
        currentStageIndex = 0;
        UpdatePlayerStage();
        UpdateProgress();
        
        if (showDebugLogs)
        {
            Debug.Log("Fragment collection progress reset.");
        }
    }
    
    /// <summary>
    /// Get all stage unlock configurations
    /// </summary>
    public List<StageUnlock> GetAllStages()
    {
        return new List<StageUnlock>(stageUnlocks);
    }
}