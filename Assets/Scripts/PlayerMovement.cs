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

    Rigidbody2D rb;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction dashAction;

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

        // Horizontal movement with acceleration smoothing
        float targetVx = moveInput.x * maxSpeed;
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
        bool canUseCoyote = Time.time - lastGroundedTime <= coyoteTime;

        if (isGrounded || canUseCoyote)
        {
            DoJump();
        }
        else if (jumpsRemaining > 0)
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
        if (isDashing || dashOnCooldown) return;

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
}
