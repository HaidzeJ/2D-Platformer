using UnityEngine;

/// <summary>
/// Simple trigger to unlock player movement abilities.
/// Place this on GameObjects with Collider2D set as triggers.
/// </summary>
public class AbilityUnlockTrigger : MonoBehaviour
{
    [Header("Unlock Settings")]
    [SerializeField] PlayerMovement.MovementStage stageToUnlock = PlayerMovement.MovementStage.Stage2_Jump;
    [SerializeField] bool unlockOnTriggerEnter = true;
    [SerializeField] bool destroyAfterUnlock = true;
    [SerializeField] GameObject unlockEffect; // Optional particle effect or UI popup

    [Header("Audio")]
    [SerializeField] AudioClip unlockSound;

    private PlayerMovement playerMovement;
    private bool hasUnlocked = false;

    void Start()
    {
        // Find the player in the scene
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        
        if (playerMovement == null)
        {
            Debug.LogError($"AbilityUnlockTrigger ({name}): No PlayerMovement found in scene!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!unlockOnTriggerEnter || hasUnlocked) return;
        
        // Check if it's the player
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>())
        {
            UnlockAbility();
        }
    }

    /// <summary>
    /// Call this method to unlock the ability (useful for buttons, cutscenes, etc.)
    /// </summary>
    public void UnlockAbility()
    {
        if (hasUnlocked || playerMovement == null) return;

        // Check if player already has this ability
        if (playerMovement.IsAbilityUnlocked(stageToUnlock))
        {
            return;
        }

        // Unlock the ability
        playerMovement.SetMovementStage(stageToUnlock);
        hasUnlocked = true;

        // Play unlock effect
        if (unlockEffect != null)
        {
            Instantiate(unlockEffect, transform.position, Quaternion.identity);
        }

        // Play unlock sound
        if (unlockSound != null)
        {
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);
        }

        // Show unlock message
        ShowUnlockMessage();

        // Destroy this trigger if set to do so
        if (destroyAfterUnlock)
        {
            Destroy(gameObject);
        }
    }

    void ShowUnlockMessage()
    {
        string abilityName = GetAbilityName(stageToUnlock);
        Debug.Log($"🎉 ABILITY UNLOCKED: {abilityName}!");
        
        // You can replace this with UI popup, notification system, etc.
        // For now, just using Debug.Log for demonstration
    }

    string GetAbilityName(PlayerMovement.MovementStage stage)
    {
        switch (stage)
        {
            case PlayerMovement.MovementStage.Stage0_Orb: return "Orb Mode";
            case PlayerMovement.MovementStage.Stage1_Movement: return "Movement";
            case PlayerMovement.MovementStage.Stage2_Jump: return "Jump";
            case PlayerMovement.MovementStage.Stage3_DoubleJump: return "Double Jump";
            case PlayerMovement.MovementStage.Stage4_Dash: return "Dash";
            case PlayerMovement.MovementStage.Stage5_WallJump: return "Wall Jump";
            default: return "Unknown Ability";
        }
    }

    // Visual feedback in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = hasUnlocked ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        
        // Draw ability name above the trigger
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up, GetAbilityName(stageToUnlock));
        #endif
    }
}