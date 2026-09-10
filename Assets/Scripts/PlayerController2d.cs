using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Player Visual")]
    [Tooltip("The visual/sprite child of the player. Only this object is flipped when changing direction. Do NOT assign the Player root or the GunPivot here.")]
    [SerializeField] private Transform playerVisual;

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

    [Tooltip("Grace period after leaving a wall during which you can still wall-jump.")]
    [SerializeField] private float wallCoyoteTime = 0.15f;

    [Tooltip("Horizontal speed the wall jump kicks you off the wall with.")]
    [SerializeField] private float wallJumpHorizontalForce = 7f;

    [Tooltip("Vertical speed of a wall jump.")]
    [SerializeField] private float wallJumpVerticalForce = 6f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;
    [SerializeField] private int maxDashes = 1;

    [Header("Swing Momentum")]
    [Tooltip("lower = keep momentum, higher = lose momentum")]
    [SerializeField] private float swingMomentumDecayRate = 4f;

    private Rigidbody2D rb;

    private float moveInput;

    private int facingDir = 1;

    private bool isGrounded;
    private int jumpsRemaining;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool isTouchingWall;
    private bool wasTouchingWall;
    private int wallSide;
    private float wallCoyoteTimer;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private int dashesRemaining;

    private bool hasSwingMomentum;

    [HideInInspector] public bool isSwinging;

    public int FacingDir => facingDir;

    public void GrantJump(int amount = 1)
    {
        jumpsRemaining += amount;
        jumpsRemaining = Mathf.Min(
            jumpsRemaining,
            maxJumps
        );
    }

    public void PreserveSwingMomentum()
    {
        hasSwingMomentum = true;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        jumpsRemaining = maxJumps;
        dashesRemaining = maxDashes;
    }

    private void Update()
    {
        // MOVEMENT INPUT

        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0f)
        {
            facingDir = (int)Mathf.Sign(moveInput);

            UpdatePlayerVisual();
        }


        // JUMP INPUT / BUFFER

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;
        }


        // DASH INPUT

        if (Input.GetKeyDown(KeyCode.LeftShift) ||
            Input.GetKeyDown(KeyCode.RightShift))
        {
            TryStartDash();
        }


        // DASH TIMERS

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

    private void UpdatePlayerVisual()
    {
        if (playerVisual == null)
            return;

        Vector3 scale = playerVisual.localScale;

        scale.x = Mathf.Abs(scale.x) * facingDir;

        playerVisual.localScale = scale;
    }

    private void FixedUpdate()
    {
        if (isSwinging)
            return;


        // GROUND CHECK

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

                if (!wasGrounded)
                {
                    jumpsRemaining = maxJumps;
                    dashesRemaining = maxDashes;
                }

                hasSwingMomentum = false;
            }
            else
            {
                coyoteTimer -= Time.fixedDeltaTime;
            }
        }


        // WALL CHECK

        isTouchingWall = false;
        int detectedWallSide = 0;

        if (wallCheck != null)
        {
            RaycastHit2D wallHitRight = Physics2D.Raycast(
                wallCheck.position,
                Vector2.right,
                wallCheckDistance,
                wallLayer
            );

            if (wallHitRight.collider != null)
            {
                isTouchingWall = true;
                detectedWallSide = 1;
            }
            else
            {
                RaycastHit2D wallHitLeft = Physics2D.Raycast(
                    wallCheck.position,
                    Vector2.left,
                    wallCheckDistance,
                    wallLayer
                );

                if (wallHitLeft.collider != null)
                {
                    isTouchingWall = true;
                    detectedWallSide = -1;
                }
            }
        }

        if (isTouchingWall)
        {
            wallSide = detectedWallSide;
            wallCoyoteTimer = wallCoyoteTime;
        }
        else if (wallCoyoteTimer > 0f)
        {
            wallCoyoteTimer -= Time.fixedDeltaTime;
        }


        // TOUCHING WALL FOR FIRST TIME

        if (isTouchingWall && !wasTouchingWall)
        {
            jumpsRemaining++;

            jumpsRemaining = Mathf.Min(
                jumpsRemaining,
                maxJumps
            );

            dashCooldownTimer = 0f;
            dashesRemaining = maxDashes;
        }

        wasTouchingWall = isTouchingWall;


        // DASH

        if (isDashing)
        {
            if (jumpBufferTimer > 0f &&
                jumpsRemaining > 0)
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


        // HORIZONTAL MOVEMENT

        if (hasSwingMomentum)
        {
            float targetX =
                moveInput * moveSpeed;

            float newX = Mathf.MoveTowards(
                rb.linearVelocity.x,
                targetX,
                swingMomentumDecayRate *
                Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector2(
                newX,
                rb.linearVelocity.y
            );

            if (Mathf.Approximately(
                newX,
                targetX))
            {
                hasSwingMomentum = false;
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(
                moveInput * moveSpeed,
                rb.linearVelocity.y
            );
        }


        // JUMP

        bool canWallJump =
            jumpBufferTimer > 0f &&
            !isGrounded &&
            (
                isTouchingWall ||
                wallCoyoteTimer > 0f
            );

        bool canJumpNow =
            jumpBufferTimer > 0f &&
            jumpsRemaining > 0 &&
            (
                isGrounded ||
                coyoteTimer > 0f ||
                jumpsRemaining < maxJumps
            );

        if (canWallJump)
        {
            rb.linearVelocity = new Vector2(
                -wallSide * wallJumpHorizontalForce,
                wallJumpVerticalForce
            );

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            wallCoyoteTimer = 0f;

            isTouchingWall = false;
        }
        else if (canJumpNow)
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

    // DASH

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

    // DEBUG / GIZMOS

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(
                groundCheck.position,
                groundCheck.position +
                Vector3.down *
                groundCheckDistance
            );
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position +
                Vector3.right *
                wallCheckDistance
            );

            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position +
                Vector3.left *
                wallCheckDistance
            );
        }
    }
}