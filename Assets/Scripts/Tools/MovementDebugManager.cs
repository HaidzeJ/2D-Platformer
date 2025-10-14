using UnityEngine;

/// <summary>
/// Debug helper to test movement ability unlocks during development.
/// Attach this to any GameObject in the scene.
/// </summary>
public class MovementDebugManager : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] PlayerMovement playerMovement;
    
    [Header("Debug Controls")]
    [SerializeField] KeyCode unlockNextStage = KeyCode.U;
    [SerializeField] KeyCode resetToStage1 = KeyCode.R;
    [SerializeField] KeyCode showCurrentAbilities = KeyCode.I;
    
    void Start()
    {
        // Auto-find player if not assigned
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }
        
        if (playerMovement == null)
        {
            Debug.LogError("MovementDebugManager: No PlayerMovement found!");
            return;
        }
        
        Debug.Log("🎮 Movement Debug Manager Active!");
        Debug.Log($"Press {unlockNextStage} to unlock next stage");
        Debug.Log($"Press {resetToStage1} to reset to Stage 1");
        Debug.Log($"Press {showCurrentAbilities} to show current abilities");
    }
    
    void Update()
    {
        if (playerMovement == null) return;
        
        // Unlock next stage
        if (Input.GetKeyDown(unlockNextStage))
        {
            UnlockNextStage();
        }
        
        // Reset to stage 1
        if (Input.GetKeyDown(resetToStage1))
        {
            ResetToStage1();
        }
        
        // Show current abilities
        if (Input.GetKeyDown(showCurrentAbilities))
        {
            ShowCurrentAbilities();
        }
    }
    
    void UnlockNextStage()
    {
        var currentStage = GetCurrentStage();
        var nextStage = (PlayerMovement.MovementStage)((int)currentStage + 1);
        
        // Cap at maximum stage
        if ((int)nextStage > 5)
        {
            Debug.Log("🔒 Already at maximum stage!");
            return;
        }
        
        playerMovement.SetMovementStage(nextStage);
        Debug.Log($"🔓 Unlocked Stage {(int)nextStage}: {nextStage}");
    }
    
    void ResetToStage1()
    {
        playerMovement.SetMovementStage(PlayerMovement.MovementStage.Stage0_Orb);
        Debug.Log("🔄 Reset to Stage 0: Orb Mode");
    }
    
    void ShowCurrentAbilities()
    {
        var currentStage = GetCurrentStage();
        var abilities = playerMovement.GetUnlockedAbilitiesString();
        
        Debug.Log($"📋 Current Stage: {(int)currentStage} ({currentStage})");
        Debug.Log($"📋 Unlocked Abilities: {abilities}");
    }
    
    PlayerMovement.MovementStage GetCurrentStage()
    {
        // Check abilities to determine current stage
        if (!playerMovement.IsAbilityUnlocked(PlayerMovement.MovementStage.Stage1_Movement))
            return PlayerMovement.MovementStage.Stage0_Orb;
        if (!playerMovement.IsAbilityUnlocked(PlayerMovement.MovementStage.Stage2_Jump))
            return PlayerMovement.MovementStage.Stage1_Movement;
        if (!playerMovement.IsAbilityUnlocked(PlayerMovement.MovementStage.Stage3_DoubleJump))
            return PlayerMovement.MovementStage.Stage2_Jump;
        if (!playerMovement.IsAbilityUnlocked(PlayerMovement.MovementStage.Stage4_Dash))
            return PlayerMovement.MovementStage.Stage3_DoubleJump;
        if (!playerMovement.IsAbilityUnlocked(PlayerMovement.MovementStage.Stage5_WallJump))
            return PlayerMovement.MovementStage.Stage4_Dash;
        
        return PlayerMovement.MovementStage.Stage5_WallJump;
    }
    
    void OnGUI()
    {
        if (playerMovement == null) return;
        
        // Simple on-screen debug info
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🎮 Movement Debug", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });
        GUILayout.Label($"Current Abilities: {playerMovement.GetUnlockedAbilitiesString()}");
        GUILayout.Space(5);
        
        if (GUILayout.Button("Unlock Next Stage"))
        {
            UnlockNextStage();
        }
        
        if (GUILayout.Button("Reset to Orb Mode"))
        {
            ResetToStage1();
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}