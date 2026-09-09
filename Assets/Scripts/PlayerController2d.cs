using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Wall")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;
    [SerializeField] private int maxDashes = 1;

    private Rigidbody2D rb;

    // Movement
    private float moveInput;
    private bool isGrounded;

    // Jump
    private int jumpsRemaining;
    private float coyoteTimer;
    private float jumpBufferTimer;

    // Wall
    private bool isTouchingWall;
    private bool wasTouchingWall;

    // Dash
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private int dashesRemaining;
    private int facingDir = 1;

    [HideInInspector] public bool isSwinging;

    public void GrantJump(int amount = 1)
    {
        jumpsRemaining += amount;
        jumpsRemaining = Mathf.Min(jumpsRemaining, maxJumps);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        jumpsRemaining = maxJumps;
        dashesRemaining = maxDashes;
    }

    private void Update()
    {
        // =========================
        // MOVEMENT INPUT
        // =========================

        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0f)
        {
            facingDir = (int)Mathf.Sign(moveInput);

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDir;
            transform.localScale = scale;
        }


        // =========================
        // JUMP INPUT / BUFFER
        // =========================

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;
        }


        // =========================
        // DASH INPUT
        // =========================

        if (Input.GetKeyDown(KeyCode.LeftShift) ||
            Input.GetKeyDown(KeyCode.RightShift))
        {
            TryStartDash();
        }


        // =========================
        // DASH TIMERS
        // =========================

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isSwinging)
            return;


        // =========================
        // GROUND CHECK
        // =========================

        bool wasGrounded = isGrounded;

        if (groundCheck != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                groundCheck.position,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );

            isGrounded = hit.collider != null;

            if (isGrounded)
            {
                coyoteTimer = coyoteTime;

                // Reset jumps and dashes when actually landing.
                if (!wasGrounded)
                {
                    jumpsRemaining = maxJumps;
                    dashesRemaining = maxDashes;
                }
            }
            else
            {
                coyoteTimer -= Time.fixedDeltaTime;
            }
        }


        // =========================
        // WALL CHECK
        // =========================

        isTouchingWall = false;

        if (wallCheck != null)
        {
            RaycastHit2D wallHitRight = Physics2D.Raycast(
                wallCheck.position,
                Vector2.right,
                wallCheckDistance,
                wallLayer
            );

            RaycastHit2D wallHitLeft = Physics2D.Raycast(
                wallCheck.position,
                Vector2.left,
                wallCheckDistance,
                wallLayer
            );

            isTouchingWall =
                wallHitRight.collider != null ||
                wallHitLeft.collider != null;
        }


        // =========================
        // TOUCHING WALL FOR FIRST TIME
        // =========================

        if (isTouchingWall && !wasTouchingWall)
        {
            // Give the player an extra jump when touching a wall.
            jumpsRemaining++;

            // Prevent the number of jumps from going above max.
            jumpsRemaining = Mathf.Min(
                jumpsRemaining,
                maxJumps
            );

            // Reset dash.
            dashCooldownTimer = 0f;
            dashesRemaining = maxDashes;
        }

        wasTouchingWall = isTouchingWall;


        // =========================
        // DASH
        // =========================

        if (isDashing)
        {
            // Allow jump to cancel the dash.
            if (jumpBufferTimer > 0f && jumpsRemaining > 0)
            {
                isDashing = false;
            }
            else
            {
                rb.linearVelocity = new Vector2(
                    facingDir * dashSpeed,
                    0f
                );

                return;
            }
        }


        // =========================
        // HORIZONTAL MOVEMENT
        // =========================

        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );


        // =========================
        // JUMP
        // =========================

        bool canJumpNow =
            jumpBufferTimer > 0f &&
            jumpsRemaining > 0 &&
            (
                isGrounded ||
                coyoteTimer > 0f ||
                jumpsRemaining < maxJumps
            );

        if (canJumpNow)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            jumpsRemaining--;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
    }


    // =========================
    // DASH
    // =========================

    private void TryStartDash()
    {
        if (isDashing)
            return;

        if (dashCooldownTimer > 0f)
            return;

        if (dashesRemaining <= 0)
            return;

        isDashing = true;

        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        dashesRemaining--;
    }


    // =========================
    // DEBUG / GIZMOS
    // =========================

    private void OnDrawGizmosSelected()
    {
        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(
                groundCheck.position,
                groundCheck.position +
                Vector3.down * groundCheckDistance
            );
        }

        // Wall check
        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position +
                Vector3.right * wallCheckDistance
            );

            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position +
                Vector3.left * wallCheckDistance
            );
        }
    }
}