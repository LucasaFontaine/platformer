using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    [SerializeField] private float swingRadius = 5f;
    [SerializeField] private KeyCode swingKey = KeyCode.W;
    [SerializeField] private Transform player;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float swingDrag = 0.15f;

    [Header("Grapple Cooldown")]
    [SerializeField] private float grappleCooldown = 1f;

    private static readonly List<Hook> allHooks = new List<Hook>();
    private static Hook activeHook;

    private Rigidbody2D playerRb;
    private PlayerController2D playerController;

    private bool isSwinging;
    private float swingLength;
    private Vector2 radialDir;
    private float angularVelocity;

    // Cooldown timer
    private float grappleCooldownTimer;

    private void OnEnable()
    {
        allHooks.Add(this);
    }

    private void OnDisable()
    {
        allHooks.Remove(this);

        if (activeHook == this)
        {
            EndSwing();
        }
    }

    private void Awake()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");

            if (found != null)
            {
                player = found.transform;
            }
        }

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
            playerController = player.GetComponent<PlayerController2D>();
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (player == null || playerRb == null)
            return;

        // =========================
        // GRAPPLE COOLDOWN
        // =========================

        if (grappleCooldownTimer > 0f)
        {
            grappleCooldownTimer -= Time.deltaTime;
        }


        // =========================
        // DISTANCE CHECK
        // =========================

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        bool inRange = distance <= swingRadius;


        // =========================
        // START SWING
        // =========================

        if (
            !isSwinging &&
            activeHook == null &&
            grappleCooldownTimer <= 0f &&
            inRange &&
            Input.GetKey(swingKey) &&
            IsClosestInRange(distance)
        )
        {
            StartSwing();
        }


        // =========================
        // END SWING
        // =========================

        else if (
            isSwinging &&
            (
                !Input.GetKey(swingKey) ||
                Input.GetButtonDown("Jump")
            )
        )
        {
            EndSwing();
        }


        // =========================
        // LINE RENDERER
        // =========================

        if (lineRenderer != null)
        {
            lineRenderer.enabled = isSwinging;

            if (isSwinging)
            {
                lineRenderer.SetPosition(
                    0,
                    transform.position
                );

                lineRenderer.SetPosition(
                    1,
                    player.position
                );
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isSwinging)
            return;

        Vector2 anchor = transform.position;

        Vector2 tangent = new Vector2(
            -radialDir.y,
            radialDir.x
        );

        float gravity =
            Physics2D.gravity.y *
            playerRb.gravityScale;

        float angularAccel =
            (gravity * tangent.y) /
            swingLength;

        angularVelocity +=
            angularAccel *
            Time.fixedDeltaTime;

        angularVelocity *=
            Mathf.Clamp01(
                1f -
                swingDrag *
                Time.fixedDeltaTime
            );

        float deltaAngle =
            angularVelocity *
            Time.fixedDeltaTime;

        radialDir = Rotate(
            radialDir,
            deltaAngle
        );

        Vector2 newPos =
            anchor +
            radialDir *
            swingLength;

        playerRb.MovePosition(newPos);

        Vector2 newTangent = new Vector2(
            -radialDir.y,
            radialDir.x
        );

        playerRb.linearVelocity =
            newTangent *
            (angularVelocity * swingLength);
    }

    private void StartSwing()
    {
        // Extra safety check.
        if (grappleCooldownTimer > 0f)
            return;

        isSwinging = true;
        activeHook = this;

        swingLength = Vector2.Distance(
            transform.position,
            player.position
        );

        radialDir =
            (
                (Vector2)player.position -
                (Vector2)transform.position
            ).normalized;

        Vector2 tangent = new Vector2(
            -radialDir.y,
            radialDir.x
        );

        angularVelocity =
            Vector2.Dot(
                playerRb.linearVelocity,
                tangent
            ) / swingLength;

        if (playerController != null)
        {
            playerController.isSwinging = true;
        }
    }

    private void EndSwing()
    {
        // Don't do anything if we're already not swinging.
        if (!isSwinging)
            return;

        isSwinging = false;

        // START 1 SECOND COOLDOWN
        grappleCooldownTimer = grappleCooldown;

        if (activeHook == this)
        {
            activeHook = null;
        }

        if (playerController != null)
        {
            playerController.isSwinging = false;

            // Give player their jump back after releasing grapple.
            playerController.GrantJump();
        }
    }

    private bool IsClosestInRange(float myDistance)
    {
        foreach (Hook hook in allHooks)
        {
            if (hook == this || hook.player != player)
                continue;

            float otherDistance =
                Vector2.Distance(
                    hook.transform.position,
                    player.position
                );

            if (
                otherDistance <= hook.swingRadius &&
                otherDistance < myDistance
            )
            {
                return false;
            }
        }

        return true;
    }

    private static Vector2 Rotate(
        Vector2 v,
        float radians
    )
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            swingRadius
        );
    }
}