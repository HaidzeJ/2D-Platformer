using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;

    [Header("Movement")]
    [SerializeField] float maxSpeed = 8f;
    [SerializeField] float runAcceleration = 60f;
    [SerializeField] float runDeceleration = 80f;
    [SerializeField] float airAcceleration = 30f;
    [SerializeField] float airDeceleration = 40f;

    [Header("Jump")]
    [SerializeField] int maxJumps = 2; // double jump
    [SerializeField] float jumpVelocity = 12f;
    [SerializeField] float coyoteTime = 0.08f;
    [SerializeField] float jumpBufferTime = 0.08f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 22f;
    [SerializeField] float dashDuration = 0.14f;
    [SerializeField] float dashCooldown = 0.5f;

    [Header("Orb Mode Settings")]
    [SerializeField] float orbGravityScale = 0.3f;      // Reduced gravity for floating
    [SerializeField] float orbPulseForce = 8f;          // Upward pulse force
    [SerializeField] float orbPulseSideForce = 6f;      // Horizontal pulse force
    [SerializeField] float orbPulseCooldown = 0.5f;     // Time between pulses (prevents spam)
    
    [Header("Orb Inertia Settings")]
    [SerializeField] float orbMoveForce = 4f;           // Force applied for A/D movement
    [SerializeField] float orbAirResistance = 0.99f;    // Air resistance (0.99 = very little drag)
    [SerializeField] float orbMaxSpeed = 12f;           // Maximum speed to prevent runaway velocity
    [SerializeField] bool enableSpeedLimiting = true;   // Whether to limit maximum speed
    
    [Header("Orb Ground Repulsion")]
    [SerializeField] float groundRepulsionForce = 6f;   // Upward force when near ground
    [SerializeField] float repulsionDistance = 0.8f;    // Distance from ground to start repulsion
    [SerializeField] AnimationCurve repulsionCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)); // Force falloff curve
    
    [Header("Echo Pulse Settings")]
    [SerializeField] float echoPulseRadius = 10f;           // Maximum radius of echo detection
    [SerializeField] float echoPulseExpandSpeed = 25f;      // How fast the echo ring expands
    [SerializeField] LayerMask echoObjectLayers = -1;       // What layers contain echo objects
    
    [Header("Ability Unlocks")]
    [SerializeField] bool isOrbMode = true;            // Stage 0: Floating orb mode
    [SerializeField] bool canMove = false;             // Stage 1: Basic movement
    [SerializeField] bool canJump = false;             // Stage 2: Jump unlocked
    [SerializeField] bool canDoubleJump = false;       // Stage 3: Double jump
    [SerializeField] bool canDash = false;             // Stage 4: Dash ability
    [SerializeField] bool canWallJump = false;         // Stage 5: Wall jump (if you plan to add it)

    Rigidbody2D rb;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction dashAction;
    
    // Orb mode variables
    float lastPulseTime;
    float originalGravityScale;
    
    // Echo Pulse variables
    private Coroutine currentEchoPulse;
    private EchoPulseVisualEffect echoPulseVisual;
    
    // Light Resource Health
    private LightResourceHealth lightHealth;

    Vector2 moveInput = Vector2.zero;
    bool jumpHeld;

    // state
    bool isGrounded;
    float lastGroundedTime = -10f;
    float lastJumpPressedTime = -10f;
    int jumpsRemaining;
    bool isDashing;
    bool dashOnCooldown;
    int facing = 1; // 1 = right, -1 = left

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        
        // Get or create echo pulse visual effect
        echoPulseVisual = GetComponent<EchoPulseVisualEffect>();
        if (echoPulseVisual == null)
        {
            echoPulseVisual = gameObject.AddComponent<EchoPulseVisualEffect>();
        }
        
        // Get or create light resource health system
        lightHealth = GetComponent<LightResourceHealth>();
        if (lightHealth == null)
        {
            lightHealth = gameObject.AddComponent<LightResourceHealth>();
        }
    }

    void OnEnable()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            var actions = playerInput.actions;
            moveAction = actions.FindAction("Move", true);
            jumpAction = actions.FindAction("Jump", true);
            // Use the 'Sprint' action from the Input Actions asset as the dash binding.
            // This should match the action name in your InputSystem_Actions asset.
            dashAction = actions.FindAction("Sprint", true);

            if (moveAction != null)
            {
                moveAction.performed += OnMovePerformed;
                moveAction.canceled += OnMoveCanceled;
            }

            if (jumpAction != null)
            {
                jumpAction.performed += OnJumpPerformed;
                jumpAction.started += OnJumpStarted;
                jumpAction.canceled += OnJumpCanceled;
            }

            if (dashAction != null)
            {
                dashAction.performed += OnDashPerformed;
            }
        }
        ResetJumps();
        
        // Store original gravity for mode switching
        originalGravityScale = rb.gravityScale;
        
        // Set initial orb mode if enabled
        UpdateMovementMode();
    }

    void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.started -= OnJumpStarted;
            jumpAction.canceled -= OnJumpCanceled;
        }
        if (dashAction != null) dashAction.performed -= OnDashPerformed;
    }

    void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        OnJumpPressed();
    }

    void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        OnDashPressed();
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.12f, groundLayer);
        if (isGrounded) lastGroundedTime = Time.time;

        // jump buffering
        if (Time.time - lastJumpPressedTime <= jumpBufferTime)
        {
            TryJump();
        }

        // update facing from input (but not while dashing)
        if (!isDashing && Mathf.Abs(moveInput.x) > 0.01f)
        {
            facing = moveInput.x > 0 ? 1 : -1;
            Vector3 ls = transform.localScale;
            ls.x = Mathf.Abs(ls.x) * facing;
            transform.localScale = ls;
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return; // dash controls velocity during dash

        // Handle orb mode physics
        if (isOrbMode)
        {
            HandleOrbPhysics();
            return;
        }

        // Normal platformer movement
        // Horizontal movement with acceleration smoothing (affected by health)
        float effectiveMaxSpeed = maxSpeed * GetHealthSpeedModifier();
        float targetVx = canMove ? moveInput.x * effectiveMaxSpeed : 0f; // Check if movement is unlocked
        float accel = isGrounded ? runAcceleration : airAcceleration;
        float decel = isGrounded ? runDeceleration : airDeceleration;

        float vx = rb.linearVelocity.x;
        if (Mathf.Abs(targetVx) > Mathf.Abs(vx))
        {
            // accelerate
            vx = Mathf.MoveTowards(vx, targetVx, accel * Time.fixedDeltaTime);
        }
        else
        {
            // decelerate
            vx = Mathf.MoveTowards(vx, targetVx, decel * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);

        // Variable jump height: apply extra gravity when falling or when releasing jump early
        // Use Physics2D.gravity * rb.gravityScale to match physics gravity
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * rb.gravityScale * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * rb.gravityScale * Time.fixedDeltaTime;
        }
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    void OnJumpPressed()
    {
        // Handle orb mode light pulse
        if (isOrbMode)
        {
            PerformLightPulse();
            return;
        }
        
        if (!canJump) return; // Check if jump is unlocked
        lastJumpPressedTime = Time.time;
    }

    void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        jumpHeld = true;
    }

    void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        jumpHeld = false;
    }

    void TryJump()
    {
        if (!canJump) return; // Check if jump is unlocked

        bool canUseCoyote = Time.time - lastGroundedTime <= coyoteTime;

        if (isGrounded || canUseCoyote)
        {
            DoJump();
        }
        else if (jumpsRemaining > 0 && canDoubleJump && CanPerformAdvancedAbilities()) // Check if double jump is unlocked and have enough light
        {
            DoJump();
        }
    }

    void DoJump()
    {
        // consume either the grounded 'slot' or an aerial jump
        if (!isGrounded && jumpsRemaining <= 0) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

        if (!isGrounded) jumpsRemaining--;

        // clear buffer so we don't jump multiple times
        lastJumpPressedTime = -10f;
    }

    void ResetJumps()
    {
        // when grounded we effectively have used 0 jumps; allow (maxJumps - 1) extra in air
        jumpsRemaining = Mathf.Max(0, maxJumps - 1);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // landed
            ResetJumps();
        }
    }

    void OnDashPressed()
    {
        if (!canDash || isDashing || dashOnCooldown || !CanPerformAdvancedAbilities()) return; // Check if dash is unlocked and have enough light

        StartCoroutine(PerformDash());
    }

    IEnumerator PerformDash()
    {
        isDashing = true;
        dashOnCooldown = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(facing * dashSpeed, 0f);

        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        rb.gravityScale = originalGravity;
        isDashing = false;

        // start cooldown
        yield return new WaitForSeconds(dashCooldown);
        dashOnCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, 0.12f);
        }
    }

    // ===== ABILITY UNLOCK SYSTEM =====
    
    [System.Serializable]
    public enum MovementStage
    {
        Stage0_Orb = 0,         // Floating orb with light pulse
        Stage1_Movement = 1,    // Can move left/right
        Stage2_Jump = 2,        // Can jump
        Stage3_DoubleJump = 3,  // Can double jump
        Stage4_Dash = 4,        // Can dash
        Stage5_WallJump = 5     // Can wall jump (future)
    }

    [Header("Debug - Current Stage")]
    [SerializeField] MovementStage currentStage = MovementStage.Stage0_Orb;

    /// <summary>
    /// Set the player's current movement stage and unlock abilities up to that stage
    /// </summary>
    public void SetMovementStage(MovementStage stage)
    {
        currentStage = stage;
        
        // Set orb mode
        isOrbMode = stage == MovementStage.Stage0_Orb;
        
        // Unlock abilities based on stage
        canMove = stage >= MovementStage.Stage1_Movement;
        canJump = stage >= MovementStage.Stage2_Jump;
        canDoubleJump = stage >= MovementStage.Stage3_DoubleJump;
        canDash = stage >= MovementStage.Stage4_Dash;
        canWallJump = stage >= MovementStage.Stage5_WallJump;
        
        // Update movement mode
        UpdateMovementMode();
        
        // Reset jumps when unlocking abilities
        if (canJump) ResetJumps();
        
        Debug.Log($"Movement abilities updated to Stage {(int)stage}: OrbMode={isOrbMode}, Movement={canMove}, Jump={canJump}, DoubleJump={canDoubleJump}, Dash={canDash}");
    }

    /// <summary>
    /// Unlock specific abilities individually
    /// </summary>
    public void UnlockMovement() => canMove = true;
    public void UnlockJump() => canJump = true;
    public void UnlockDoubleJump() => canDoubleJump = true;
    public void UnlockDash() => canDash = true;
    public void UnlockWallJump() => canWallJump = true;

    /// <summary>
    /// Check if an ability is unlocked
    /// </summary>
    public bool IsAbilityUnlocked(MovementStage ability)
    {
        return currentStage >= ability;
    }

    /// <summary>
    /// Get current abilities as a readable string
    /// </summary>
    public string GetUnlockedAbilitiesString()
    {
        var abilities = new System.Collections.Generic.List<string>();
        if (isOrbMode) abilities.Add("Light Pulse");
        if (canMove) abilities.Add("Movement");
        if (canJump) abilities.Add("Jump");
        if (canDoubleJump) abilities.Add("Double Jump");
        if (canDash) abilities.Add("Dash");
        if (canWallJump) abilities.Add("Wall Jump");
        
        return abilities.Count > 0 ? string.Join(", ", abilities) : "None";
    }

    /// <summary>
    /// Check if player is currently in orb mode
    /// </summary>
    public bool IsInOrbMode => isOrbMode;

    /// <summary>
    /// Get the player's Rigidbody2D component for external physics interactions
    /// </summary>
    public Rigidbody2D GetRigidbody() => rb;
    
    // ===== LIGHT RESOURCE INTEGRATION =====
    
    /// <summary>
    /// Check if the player has enough light energy to perform advanced abilities
    /// </summary>
    bool CanPerformAdvancedAbilities()
    {
        if (lightHealth == null) return true;
        
        // At low health, disable advanced abilities (dash, double jump)
        return lightHealth.LightResourcePercent > 0.2f; // Need more than 20% health
    }
    
    /// <summary>
    /// Get movement speed modifier based on health (lower health = slower movement)
    /// </summary>
    float GetHealthSpeedModifier()
    {
        if (lightHealth == null) return 1f;
        
        float healthPercentage = lightHealth.LightResourcePercent;
        
        // Scale speed from 0.5x to 1.0x based on health (50% to 100% effectiveness)
        return Mathf.Lerp(0.5f, 1f, healthPercentage);
    }
    
    // ===== ORB MODE SYSTEM =====
    
    /// <summary>
    /// Update movement mode based on current stage
    /// </summary>
    void UpdateMovementMode()
    {
        if (rb == null) return;
        
        if (isOrbMode)
        {
            // Set reduced gravity for floating effect
            rb.gravityScale = orbGravityScale;
        }
        else
        {
            // Restore normal gravity
            rb.gravityScale = originalGravityScale;
        }
    }
    
    /// <summary>
    /// Handle orb physics - inertia-driven movement with ground repulsion
    /// </summary>
    void HandleOrbPhysics()
    {
        // Apply horizontal movement force (now works at all times with ground repulsion)
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            Vector2 moveForce = new Vector2(moveInput.x * orbMoveForce, 0f);
            rb.AddForce(moveForce, ForceMode2D.Force);
        }
        
        // Apply ground repulsion to keep orb floating
        ApplyGroundRepulsion();
        
        // Apply minimal air resistance to maintain momentum but prevent infinite acceleration
        Vector2 currentVelocity = rb.linearVelocity;
        Vector2 resistanceForce = currentVelocity * (1f - orbAirResistance);
        rb.AddForce(-resistanceForce, ForceMode2D.Force);
        
        // Optional speed limiting to prevent runaway velocity
        if (enableSpeedLimiting && currentVelocity.magnitude > orbMaxSpeed)
        {
            rb.linearVelocity = currentVelocity.normalized * orbMaxSpeed;
        }
    }
    
    /// <summary>
    /// Apply ground repulsion to keep orb floating above surfaces
    /// </summary>
    void ApplyGroundRepulsion()
    {
        if (groundCheck == null) return;
        
        // Cast downward to detect ground distance
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, repulsionDistance, groundLayer);
        
        if (hit.collider != null)
        {
            float distanceToGround = hit.distance;
            
            // Calculate repulsion strength based on distance (closer = stronger)
            float normalizedDistance = distanceToGround / repulsionDistance; // 0 = touching, 1 = at max distance
            float repulsionStrength = repulsionCurve.Evaluate(1f - normalizedDistance); // Invert so closer = stronger
            
            // Apply upward repulsion force
            Vector2 repulsionForce = Vector2.up * groundRepulsionForce * repulsionStrength;
            rb.AddForce(repulsionForce, ForceMode2D.Force);
            
            // Optional: Add slight damping to vertical velocity when very close to prevent bouncing
            if (distanceToGround < repulsionDistance * 0.3f && rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.8f);
            }
        }
    }
    
    /// <summary>
    /// Perform light pulse - main orb movement mechanic
    /// </summary>
    void PerformLightPulse()
    {
        // Check cooldown and provide feedback
        float timeSinceLastPulse = Time.time - lastPulseTime;
        if (timeSinceLastPulse < orbPulseCooldown)
        {
            return;
        }
        
        lastPulseTime = Time.time;
        
        // Get current movement input
        Vector2 pulseDirection = Vector2.up; // Default upward pulse
        
        // Add horizontal component if A or D is held
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            pulseDirection.x = moveInput.x;
            // Normalize to maintain consistent pulse strength
            pulseDirection = pulseDirection.normalized;
        }
        
        // Apply pulse force
        Vector2 pulseForce = new Vector2(
            pulseDirection.x * orbPulseSideForce,
            pulseDirection.y * orbPulseForce
        );
        
        rb.AddForce(pulseForce, ForceMode2D.Impulse);
        
        // Trigger Echo Pulse to reveal hidden objects
        TriggerEchoPulse();
        
        // Trigger visual effect
        if (echoPulseVisual != null)
        {
            echoPulseVisual.TriggerPulseEffect(transform.position);
        }
    }
    
    /// <summary>
    /// Check if light pulse is available (not on cooldown)
    /// </summary>
    public bool IsPulseAvailable()
    {
        return Time.time - lastPulseTime >= orbPulseCooldown;
    }
    
    /// <summary>
    /// Get remaining cooldown time for light pulse
    /// </summary>
    public float GetPulseCooldownRemaining()
    {
        float remaining = orbPulseCooldown - (Time.time - lastPulseTime);
        return Mathf.Max(0f, remaining);
    }
    
    // ===== ECHO PULSE SYSTEM =====
    
    /// <summary>
    /// Trigger an expanding echo pulse that reveals hidden echo objects
    /// </summary>
    void TriggerEchoPulse()
    {
        // Stop any current echo pulse
        if (currentEchoPulse != null)
        {
            StopCoroutine(currentEchoPulse);
        }
        
        // Start new echo pulse
        currentEchoPulse = StartCoroutine(ExpandEchoPulse());
        
        Debug.Log("🔊 Echo Pulse triggered!");
    }
    
    /// <summary>
    /// Coroutine that handles the expanding echo pulse detection
    /// </summary>
    System.Collections.IEnumerator ExpandEchoPulse()
    {
        Vector2 pulseCenter = transform.position;
        float currentRadius = 0f;
        System.Collections.Generic.HashSet<EchoObject> revealedObjects = new System.Collections.Generic.HashSet<EchoObject>();
        
        // Expand the pulse over time
        while (currentRadius < echoPulseRadius)
        {
            currentRadius += echoPulseExpandSpeed * Time.deltaTime;
            
            // Find all colliders within current radius
            Collider2D[] colliders = Physics2D.OverlapCircleAll(pulseCenter, currentRadius, echoObjectLayers);
            
            foreach (Collider2D collider in colliders)
            {
                EchoObject echoObj = collider.GetComponent<EchoObject>();
                if (echoObj != null && !revealedObjects.Contains(echoObj))
                {
                    // Calculate distance to determine if this object should be revealed now
                    float distanceToObject = Vector2.Distance(pulseCenter, collider.transform.position);
                    
                    // Only reveal objects that the pulse has reached (with small buffer for timing)
                    if (distanceToObject <= currentRadius + 0.5f)
                    {
                        // Use -1 to let each object use its own defaultRevealDuration
                        echoObj.RevealObject(-1f);
                        revealedObjects.Add(echoObj);
                        Debug.Log($"🔍 Echo revealed: {echoObj.name} at distance {distanceToObject:F1}");
                    }
                }
            }
            
            yield return null; // Wait for next frame
        }
        
        currentEchoPulse = null;
        Debug.Log($"🔊 Echo Pulse completed - revealed {revealedObjects.Count} objects");
    }
}
