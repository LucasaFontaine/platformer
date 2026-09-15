using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Player Visual")]
    [SerializeField] private Transform playerVisual;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 5f;
    [SerializeField] private float staminaRegenRate = 0.2f;
    [SerializeField] private float staminaCostPerJump = 1f;
    [SerializeField] private float staminaCostPerDash = 1f;

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
    private int wallSide;
    private float wallCoyoteTimer;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;

    private float stamina;

    private bool hasSwingMomentum;

    [HideInInspector] public bool isSwinging;

    public int FacingDir => facingDir;

    public float CurrentStamina => stamina;

    public float MaxStamina => maxStamina;

    public void PreserveSwingMomentum()
    {
        hasSwingMomentum = true;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        jumpsRemaining = maxJumps;
        stamina = maxStamina;
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


        // STAMINA REGEN

        if (!isDashing && stamina < maxStamina)
        {
            stamina += staminaRegenRate * Time.deltaTime;
            stamina = Mathf.Min(stamina, maxStamina);
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
                }

                hasSwingMomentum = false;
            }
            else
            {
                coyoteTimer -= Time.fixedDeltaTime;
            }
        }


        // WALL CHECK
        // Only checks in the direction the player is facing

        isTouchingWall = false;
        int detectedWallSide = 0;

        if (wallCheck != null)
        {
            Vector2 wallDirection =
                facingDir == 1
                    ? Vector2.right
                    : Vector2.left;

            RaycastHit2D wallHit = Physics2D.Raycast(
                wallCheck.position,
                wallDirection,
                wallCheckDistance,
                wallLayer
            );

            if (wallHit.collider != null)
            {
                isTouchingWall = true;
                detectedWallSide = facingDir;
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


        // DASH

        if (isDashing)
        {
            if (jumpBufferTimer > 0f)
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
            float targetX = moveInput * moveSpeed;

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

            if (Mathf.Approximately(newX, targetX))
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


        // WALL JUMP

        bool canWallJump =
            jumpBufferTimer > 0f &&
            !isGrounded &&
            (
                isTouchingWall ||
                wallCoyoteTimer > 0f
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

            return;
        }


        // NORMAL / DOUBLE JUMP

        bool wantsToJump = jumpBufferTimer > 0f;

        if (wantsToJump)
        {
            // Ground jump is free

            if (isGrounded || coyoteTimer > 0f)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

                jumpsRemaining = maxJumps - 1;

                jumpBufferTimer = 0f;
                coyoteTimer = 0f;

                return;
            }


            // Extra jumps require stamina

            if (jumpsRemaining > 0 &&
                stamina >= staminaCostPerJump)
            {
                stamina -= staminaCostPerJump;

                jumpsRemaining--;

                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

                jumpBufferTimer = 0f;
            }
        }
    }


    // DASH

    private void TryStartDash()
    {
        if (isDashing)
            return;

        if (dashCooldownTimer > 0f)
            return;

        if (stamina < staminaCostPerDash)
            return;

        stamina -= staminaCostPerDash;

        isDashing = true;

        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
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

            Vector3 direction =
                facingDir == 1
                    ? Vector3.right
                    : Vector3.left;

            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position +
                direction *
                wallCheckDistance
            );
        }
    }
}